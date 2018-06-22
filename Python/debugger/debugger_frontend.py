import threading

from errors import RPCError, DBQuitError
from zmq_wrapper import ZmqRpcIO


class Frontend(ZmqRpcIO):
    "Qdb generic Frontend interface"

    def __init__(self, zmq_socket):
        self.i = 1
        self.info = ()
        self.zmq_socket = zmq_socket
        ZmqRpcIO.__init__(self, self.zmq_socket, threading.RLock())
        self.notifies = []

    def startup(self, version, pid, thread_name, argv, filename):
        self.info = (version, pid, thread_name, argv, filename)
        self.send({'method': 'run', 'args': (), 'id': -1})

    def interaction(self, filename, lineno, line, context):
        raise NotImplementedError

    def exception(self, title, extype, exvalue, trace, request):
        "Show a user_exception"
        raise NotImplementedError

    def write(self, text):
        "Console output (print)"
        raise NotImplementedError

    def readline(self, text):
        "Console input/rawinput"
        raise NotImplementedError

    def run(self):
        "Main method dispatcher (infinite loop)"
        if self.zmq_socket:
            if not self.notifies:
                # wait for a message...
                request = self.recv()
            else:
                # process an asynchronous notification received earlier
                request = self.notifies.pop(0)
            return self.process_message(request)

    def process_message(self, request):
        if request:
            result = None
            if request.get("error"):
                # it is not supposed to get an error here
                # it should be raised by the method call
                raise RPCError(res['error']['message'])
            elif request.get('method') == 'dbquit':
                self.zmq_socket.send(b'okay, quitting')
                raise DBQuitError()
            elif request.get('method') == 'interaction':
                self.interaction(*request.get("args"))
            elif request.get('method') == 'startup':
                self.startup(*request['args'])
            elif request.get('method') == 'exception':
                self.exception(*request['args'])
            elif request.get('method') == 'write':
                self.write(*request.get("args"))
            elif request.get('method') == 'readline':
                result = self.readline()
            elif request.get('method') == 'ping':
                result = request['args']

            if result:
                response = {'jsonrpc': '1.1', 'id': request.get('id'),
                            'result': result,
                            'error': None}
                self.send(response)
            return True

    def call(self, method, *args):
        "Actually call the remote method (inside the thread)"
        req = {'method': method, 'args': args, 'id': self.i}
        self.send(req)
        self.i += 1  # increment the id
        while 1:
            # wait until command acknowledge (response id match the request)
            res = self.recv()
            if 'id' not in res or not res['id']:
                # nested notification received (i.e. write)! process it later...
                self.notifies.append(res)
            elif 'result' not in res:
                # nested request received (i.e. readline)! process it!
                self.process_message(res)
            elif int(req['id']) != int(res['id']):
                print("DEBUGGER wrong packet received: expecting id {} {}".format(req['id'], res['id']))
                # protocol state is unknown
            elif 'error' in res and res['error']:
                raise RPCError(res['error']['message'])
            else:
                return res['result']

    def do_step(self, arg=None):
        "Execute the current line, stop at the first possible occasion"
        self.call('do_step')

    def do_next(self, arg=None):
        "Execute the current line, do not stop at function calls"
        self.call('do_next')

    def do_continue(self, arg=None):
        "Continue execution, only stop when a breakpoint is encountered."
        self.call('do_continue')

    def do_return(self, arg=None):
        "Continue execution until the current function returns"
        self.call('do_return')

    def do_jump(self, arg):
        "Set the next line that will be executed (None if sucess or message)"
        res = self.call('do_jump', arg)
        return res

    def do_where(self, arg=None):
        "Print a stack trace, with the most recent frame at the bottom."
        return self.call('do_where')

    def do_quit(self, arg=None):
        "Quit from the debugger. The program being executed is aborted."
        self.call('do_quit')

    def do_eval(self, expr):
        "Inspect the value of the expression"
        return self.call('do_eval', expr)

    def do_environment(self):
        "List all the locals and globals variables (string representation)"
        return self.call('do_environment')

    # def do_list(self, arg=None):
    #     "List source code for the current file"
    #     return self.call('do_list', arg)

    def do_read(self, filename):
        "Read and send a local filename"
        return self.call('do_read', filename)

    def do_set_breakpoint(self, filename, lineno, temporary=0, cond=None):
        "Set a breakpoint at filename:breakpoint"
        self.call('do_set_breakpoint', filename, lineno, temporary, cond)

    def do_clear_breakpoint(self, filename, lineno):
        "Remove a breakpoint at filename:breakpoint"
        self.call('do_clear_breakpoint', filename, lineno)

    def do_clear_file_breakpoints(self, filename):
        "Remove all breakpoints at filename"
        self.call('do_clear_breakpoints', filename, lineno)

    def do_list_breakpoint(self):
        "List all breakpoints"
        return self.call('do_list_breakpoint')

    def do_exec(self, statement):
        return self.call('do_exec', statement)

    # def get_autocomplete_list(self, expression):
    #     return self.call('get_autocomplete_list', expression)

    def get_call_tip(self, expression):
        return self.call('get_call_tip', expression)

    def interrupt(self):
        "Immediately stop at the first possible occasion (outside interaction)"
        # this is a notification!, do not expect a response
        req = {'method': 'interrupt', 'args': ()}
        self.send(req)

    def set_burst(self, value):
        req = {'method': 'set_burst', 'args': (value,)}
        self.send(req)

    def set_params(self, params):
        req = {'method': 'set_params', 'args': (params,)}
        self.send(req)