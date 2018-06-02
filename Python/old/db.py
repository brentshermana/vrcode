import zmq
from ActionableJsonMessage import makeMessage, messageString
import json_utils
import time
import os
import sys
import queue
import inspect
import traceback
import logging

import bdb

import __main__

from threading import Thread

IP = '127.0.0.1'
push_status_port = 12346
pull_controls_port = 12347
push_output_port = 12348
req_stdin_port = 12349

class zmq_input_stream:
    def __init__(self, zmq_req_socket):
        self.sock = zmq_req_socket

    # file-like functions for stdout, stdin, stderr IO:
    def readline(self):
        self.sock.send_string("readline_request")
        # assuming frontend breaks messages into individual lines
        str = self.sock.recv_string(encoding='utf-8')
        return str
    def readlines(self):
        lines = []
        while lines[-1:] != ['\n']:  # apparently, last line should be newline to indicate end of lines
            lines.append(self.readline())
        return lines
    def write(self, text):
        pass
    def writelines(self, l):
        pass
    def flush(self):
        pass
    def isatty(self):
        return 0
    def encoding(self):
        return 'utf-8'
    def close(self):
        pass
        # # revert redirections and close connection
        # sys.stdin, sys.stdout, sys.stderr = self.old_stdio
        # try:
        #     self.pipe.close()
        # except:
        #     pass
    def __del__(self):
        self.close()

class zmq_output_stream:
    def __init__(self, zmq_push_socket, prefix):
        self.prefix = prefix
        self.sock = zmq_push_socket

    # file-like functions for stdout, stdin, stderr IO:
    def readline(self):
        pass
    def readlines(self):
        pass
    def write(self, text):
        self.sock.send_string(self.prefix + text, encoding='utf-8')
    def writelines(self, l):
        for ls in l:
            self.write(l)
    def flush(self):
        pass
    def isatty(self):
        return 0
    def encoding(self):
        return 'utf-8'  # use default, 'utf-8' should be better...
    def close(self):
        pass
        # # revert redirections and close connection
        # sys.stdin, sys.stdout, sys.stderr = self.old_stdio
        # try:
        #     self.pipe.close()
        # except:
        #     pass
    def __del__(self):
        self.close()



class db(bdb.Bdb):
    #Todo: debugging stuff

    def __init__(self):
        # TODO: possibly add other files upon which db depends?
        # files to ignore within debugger
        self.ignore_files = [self.canonic(f) for f in (__main__.__file__, bdb.__file__)]

        # fake breakpoint to prevent removing trace_dispatch on set_continue
        self.breaks[None] = []

        # zeromq stuff
        #   rule: pusher BINDS, puller CONNECTS
        self.context = zmq.Context()

        self.push_status = self.context.socket(zmq.PUSH)
        self.push_status.bind("tcp://{}:{}".format(IP, push_status_port))

        self.push_output = self.context.socket(zmq.PUSH)
        self.push_output.bind("tcp://{}:{}".format(IP, push_output_port))

        self.pull_controls = self.context.socket(zmq.PULL)
        self.pull_controls.connect("tcp://{}:{}".format(IP, pull_controls_port))

        self.req_stdin = self.context.socket(zmq.REQ)
        self.req_stdin.connect("tcp://{}:{}".format(IP, req_stdin_port))

        # STDOUT, STDERR redirection
        # TODO: assign to self (refer to qdb)
        self.original_stdout = sys.stdout
        self.original_stderr = sys.stderr

        sys.stdin = zmq_input_stream(self.req_stdin)
        sys.stdout = zmq_output_stream(self.push_output, "OUT")
        sys.stderr = zmq_output_stream(self.push_output, "ERR")

        # need to store references to any external functions
        # as instance variables to avoid dict flushing from
        # running a debugged file
        logging.basicConfig(filename="log.log", level=logging.DEBUG)
        self.encode_json = json_utils.encode_json
        self.decode_json = json_utils.decode_json
        self.make_message = makeMessage
        self.messageString = messageString
        self.joinpaths = os.path.join
        self._ismethod = inspect.ismethod
        self.ismethod = lambda s : hasattr(self, s) and self._ismethod( getattr(self, s) )
        self.isfile = os.path.isfile
        # self.direxists = os.direxists
        self.print_out = self.original_stdout.write
        self.log_debug = logging.debug
        self.print_err = self.original_stderr.write
        self.log_err = logging.exception
        self.exception_traceback = traceback.format_exc
        self.dirname_of = os.path.dirname


        #initialize bdb, other debugger stuff
        self.reached_scope = False
        bdb.Bdb.__init__(self)

    #output functions
    def log(self, s):
        self.log_debug(s)
        self.print_out(s)
    def logerr(self):
        err_str = self.exception_traceback()
        self.log_err('')
        self.print_err(err_str)


    def start(self):
        self.thread = Thread(target=self.message_processor)
        self.thread.start()


    def message_processor(self):
        # main loop
        while True:
            msg = self.pull_controls.recv_json()
            self.process_message(msg, "Editor")

    # delegates args from msg to appropriate function, returning the return value if applicable
    def process_message(self, msg, type_filter):
        if msg["Type"] == type_filter and self.ismethod(msg["SubType"]):  # valid message in this context
            try:
                method = getattr(self, msg["SubType"])
                # convert the args to necessary types, if supported
                args = msg["Args"]
                if hasattr(self, msg["SubType"] + "_argconv") and self.ismethod(msg["SubType"] + "_argconv"):
                    self.log("Converting args through {}".format(msg["SubType"] + "_argconv"))
                    args = getattr(self, msg["SubType"] + "_argconv")(args)
                self.log("Calling {}".format(msg["SubType"]))
                ret = getattr(self, msg["SubType"])(*args)  # args unpacked here
                if ret:
                    return ret
            except Exception as e:
                self.logerr()


    #EDITOR MESSAGES:
    def Directory(self, dir):
        #if self.direxists(dir):
        self.wd = dir
        # else:
        #     self.log("dir {} does not exist".format(dir))
    def File(self, name, content):
        # TODO: ensure self.wd has already been assigned
        path = self.joinpaths(self.wd, name)
        f = open(path, "w+")
        f.write(content)
        f.close()
    def Run(self, file):
        self.reset() # might be necessary when run is done more than once
        file_to_debug = self.joinpaths(self.wd, file)
        try:
            ### see pdb _runscript() for where this code came from
            import __main__
            # set search path to where the file to debug is
            sys.path[0] = self.dirname_of(file_to_debug)
            #preserve builtins
            builtins = __main__.__dict__["__builtins__"]
            __main__.__dict__.clear()
            # __main__.__dict__.update({"__name__": "__main__",
            #                           "__file__": self.file_to_debug,
            #                           "__builtins__": builtins,})
            __main__.__dict__["__name__"] = "__main__"
            __main__.__dict__["__file__"] = file_to_debug
            __main__.__dict__["__builtins__"] = builtins
            #self.set_trace(sys._getframe().f_back)
            with open(file_to_debug, "rb") as fp:
                statement = "exec(compile(%r, %r, 'exec'))" % \
                            (fp.read(), file_to_debug)
            self.run(statement)
        except SystemExit:
            self.log("Exit from Debugged Program")
            exitmsg = self.make_message("Runtime", "Exit", [str(sys.exc_info()[1])])
            self.outq.put(exitmsg)
            # print("Program Exited. Status", sys.exc_info()[1])
        except SyntaxError:
            self.logerr()
        except:
            self.logerr()
            # traceback.print_exc()
            # print("Uncaught exception. Entering post mortem debugging")
            # t = sys.exc_info()[2] ## pdb uses this for something, but I don't see the use yet
            # stack, index = self.get_stack(None, t)
            # f = stack[index][0]
            # self.printframe(f)
            # self.interact(f)
            #print("Finished")

    #runtime stuff:
    def printframe(self, frame):
        pass
        # print("Frame:")
        # print("\tF_Code: ", frame.f_code)
        # print("\tF_Lineno", frame.f_lineno)
    def pause(self):
        pass
        #input("Press Enter to continue...")

    def user_call(self, frame, argument_list):
        # based on what has been observed so far it seems that we can ignore
        # this function
        #print("== USER CALL")
        self.printframe(frame)
    def user_line(self,frame):
        #print("== USER LINE")
        if (self.break_here(frame)):
            pass
            #print("BREAK HERE")
        if (self.stop_here(frame)):
            #print("STOP HERE")
            pass
        self.printframe(frame)
        # if (not self.stop_here(frame)):
        #     print("BREAK HERE")
        #     if self.reached_scope: # entered and then left scope
        #         print("Done Debugging")
        #         quit()
        if self.break_here(frame):
            pass
            #print("Breakpoint hit at line", frame.f_lineno)
        self.interact(frame)



    def user_return(self, frame, return_value):
        pass
        # self.printframe(frame)
        # self.pause()
    def user_exception(self, frame, exc_info):
        pass
        #print("== USER EXCEPTION")
        #self.printframe(frame)
        #self.pause()
    def do_clear(self, arg):
        pass
        # a breakpoint is removed
        #print("== BREAKPOINT IS REMOVED AT", arg)
        #self.pause()

    #loop for runtime operations. loop engages whenever we want to process commands during runtime
    #TODO: we may want different control flows based on whether interaction occurs due to
    #   a breakpoint or not...
    def interact(self, frame):
        filename = self.canonic(frame.f_code.co_filename)
        # check if interaction should be ignored (i.e. debug modules internals)
        if filename in self.ignore_files:
            return

        #tell the frontend where we are
        statusmsg = self.make_message("Runtime", "Pause", [filename, str(frame.f_lineno)])
        self.push_status.send_string(self.encode_json(statusmsg), encoding="utf-8")
        # TODO: (should I be setting this attribute elsewhere?)
        # todo: use in runtime functions
        self.current_frame = frame
        done = False
        while not done:
            msg = self.pull_controls.recv_json()
            ret = self.process_message(msg, "Runtime") #TODO: all runtime messages should return true if the intent is to continue execution
            if ret == True:
                done = ret

    # runtime functions:
    def Continue(self):
        self.set_continue()
        return True
    def Exec(self, expression):
        result = self.run(expression)
        resultmsg = self.make_message("Runtime", "Exec", [result])
        self.outq.put(resultmsg, self.current_frame.f_globals, self.current_frame.f_locals)
    def Next(self):
        self.set_step()
        return True
    def Quit(self):
        sys.exit(0)
    def BpSet(self, filename, *bps):
        canon = self.canonic(self.joinpaths(self.wd, filename))
        for bp in bps:
            self.set_break(canon, int(bp))


if __name__ == '__main__':
    db().start()



