import json_utils

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