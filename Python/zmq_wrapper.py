import zmq

import json_utils


def init_backend_socket(host='localhost', port=6000, service_name='', verbose=True):
    address = (host, port)  # family is deduced to be 'AF_INET'
    if verbose:
        print("{} backend: waiting for connection to {}".format(service_name, address))
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    if host == 'localhost':
        host = '127.0.0.1'
    zmq_socket.connect("tcp://{}:{}".format(host, port))
    zmq_socket.send(b'handshake')
    zmq_socket.recv()
    if verbose:
        print('{} backend: connected to {}'.format(service_name, address))
    return zmq_socket

def init_frontend_socket(host='localhost', port=6000, service_name='', verbose=True):
    address = (host, port)
    if verbose:
        print("{} frontend: binding to {}".format(service_name, address))
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    if host == 'localhost':
        host = '*'
    zmq_socket.bind("tcp://{}:{}".format(host, port))
    # wait until the first message is sent - that's the handshake
    if verbose:
        print("Waiting for handshake...")
    zmq_socket.recv()
    zmq_socket.send(b'handshake')
    if verbose:
        print('{} frontend: connected'.format(service_name))
    return zmq_socket


class ZmqRpcIO():
    """
    Base class for performing remote communication using
    ZeroMQ to send RPC-like messages
    """

    def __init__(self, zmq_socket, lock = None):
        self.__lock = lock
        if lock:
            self.__acquire = lambda : self.__lock.acquire()
            self.__release = lambda : self.__lock.release()
        else:
            # NOP functions
            self.__acquire = lambda : None
            self.__release = lambda : None
        self.zmq_socket = zmq_socket

    def recv(self):
        self.__acquire()
        data = json_utils.decode_json(self.zmq_socket.recv().decode())
        self.__release()
        if 'result' in data and data['result']:
            # returned objects will be returned as a string, which must be decoded here
            data['result'] = json_utils.decode_json(data['result'])
        return data

    def send(self, data):
        if 'result' in data and data['result'] != None:
            data['result'] = json_utils.encode_json(data['result'])
        self.__acquire()
        self.zmq_socket.send(json_utils.encode_json(data).encode())
        self.__release()

class ZmqFrontendCmdLoop(ZmqRpcIO):
    """
    Designed to pair well with ZmqBackendCmdLoop
    """

    def __init__(self, zmq_socket, lock=None):
        super().__init__(zmq_socket, lock=lock)
        self.call_id=1

    def run_cmd_loop(self):
        try:
            while True:
                args = input("=> ").split()
                if len(args) >= 1:
                    func = args[0]
                    if not func.startswith('do_'):
                        func = 'do_' + func
                    args = args[1:]

                    # some args should be strings, and others should be cast to ints.
                    # other args which are strings may contain escaped spaces, newlines,
                    # etc.
                    for i in range(len(args)):
                        if args[i].isdecimal(): # doesn't work on negative ints
                            args[i] = int(args[i])
                        else:
                            args[i].replace('\\\\', '\\')
                            args[i].replace('\\s', '\s')
                            args[i].replace('\\n', '\n')
                            args[i].replace('\\t', '\t')


                    request = {'method': func, 'args': args, 'id': self.call_id}
                    self.send(request)
                    response = self.recv()
                    assert request['id'] == response['id']
                    self.call_id += 1

                    if 'error' in response and response['error'] != None:
                        print("Error:\n{}".format(response['error']))
                    if 'result' in response and response['result'] != None:
                        print("Response:\n{}".format(response['result']))

                    if func == 'do_quit':
                        raise KeyboardInterrupt
        except KeyboardInterrupt:
            pass # expected method of quitting
        except Exception as e:
            print("Unexpected error:")
            print(e)
        finally:
            print("Quitting")
            self.zmq_socket.close()



class ZmqBackendCmdLoop(ZmqRpcIO):
    """
    Builds on ZmqRpcIO by enforcing the RPC-like
    request-response structure. Subclasses only need to
    implement functions of the form "do_FOO" which may be remotely
    called by the other end of the socket
    """

    def __init__(self, zmq_socket, lock=None):
        super().__init__(zmq_socket, lock=lock)
        self.should_quit = False

    def run_cmd_loop(self):
        """
        Main loop. Repeatedly runs commands until quit signal is given
        """
        while not self.should_quit:
            self.pull_action()
        self.zmq_socket.close()

    def do_quit(self):
        """
        Signals for the backend to respond, then close itself.
        We can't quit inside this method because a response must still be sent
        """
        self.should_quit = True
        return True

    def pull_action(self):
        """
        Receive a remote procedure call from the frontend, process it, then reply
        """

        request = self.recv()

        # template response object
        response = {
            'jsonrpc': '1.1',
            'id': request.get('id'),
            'method': request.get('method'),
            'result': None,
            'error': None
        }

        # we try to follow the convention that only methods starting with 'do_'
        # are fair game for RPC, as we may want to hide others
        if not request['method'].startswith("do_"):
            request['method'] = "do_" + request['method']

        try:
            # dispatch message (JSON RPC like)
            method = getattr(self, request['method'])
            # unpack 'args', a list of arguments
            response['result'] = method.__call__(*request['args'], )
        except Exception as e:
            response['error'] = {'code': 0, 'message': str(e)}

        self.send(response)