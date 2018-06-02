import bdb
import sys
import os
import traceback

from ActionableJsonMessage import makeMessage


class Mydb(bdb.Bdb):

    def __init__(self, file_to_debug, inq, outq):
        bdb.Bdb.__init__(self)

        self.inq = inq
        self.outq = outq

        self.file_to_debug = file_to_debug
        self.reached_scope = False
        if not os.path.exists(file_to_debug):
            pass
            # print("Error: " + file_to_debug + " Does not exist")
        # Replace pdb's dir with script's dir in front of module search path.
        sys.path[0] = os.path.dirname(file_to_debug)


    def start(self):
        try:
            ### see pdb _runscript() for where this code came from
            import __main__
            #preserve builtins
            builtins = __main__.__dict__["__builtins__"]
            __main__.__dict__.clear()
            # __main__.__dict__.update({"__name__": "__main__",
            #                           "__file__": self.file_to_debug,
            #                           "__builtins__": builtins,})
            __main__.__dict__["__name__"] = "__main__"
            __main__.__dict__["__file__"] = self.file_to_debug
            __main__.__dict__["__builtins__"] = builtins
            __main__.__dict__["__file__"] = self.file_to_debug
            #self.set_trace(sys._getframe().f_back)
            with open(self.file_to_debug, "rb") as fp:
                statement = "exec(compile(%r, %r, 'exec'))" % \
                            (fp.read(), self.file_to_debug)
            self.run(statement)
        except SystemExit:
            exitmsg = self.makeMessage("Runtime", "Exit", [str(sys.exc_info()[1])])
            self.outq.put(exitmsg)
            # print("Program Exited. Status", sys.exc_info()[1])
        except SyntaxError:
            exitmsg = self.makeMessage("Runtime", "Error", ["Syntax Error"])
            self.outq.put(exitmsg)
            # traceback.print_exc()
            sys.exit(1)
        except:
            exitmsg = self.makeMessage("Runtime", "Error", ["Uncaught Error"])
            self.outq.put(exitmsg)
            # traceback.print_exc()
            # print("Uncaught exception. Entering post mortem debugging")
            # t = sys.exc_info()[2] ## pdb uses this for something, but I don't see the use yet
            # stack, index = self.get_stack(None, t)
            # f = stack[index][0]
            # self.printframe(f)
            # self.interact(f)
            #print("Finished")


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
        #print("== USER RETURN")
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

    def interact(self, frame):
        #tell the frontend where we are
        statusmsg = makeMessage("Runtime", "Pause", [str(frame.f_lineno)])
        self.outq.put(statusmsg)
        done = False
        while not done:
            msg = self.inq.get_blocking()
            if msg["Type"] == "Runtime":
                done = self.runtime_msg(msg, frame)

    def runtime_msg(self, msg, frame):
        subt = msg["SubType"]
        args = msg["Args"]
        if subt == "Continue":
            self.set_continue()
            return True
        if subt == "Exec":
            result = self.run(args[0])
            resultmsg = makeMessage("Runtime", "Exec", [result])
            self.outq.put(resultmsg, frame.f_globals, frame.f_locals)
            return False
        if subt == "Next":
            self.set_step()
            return True
        if subt == "Quit":
            sys.exit(0)
        if subt == "BpSet":
            canon = self.canonic(self.file_to_debug)
            for n in args:
                self.set_break(canon, int(n))
            return False
        else:
            #TODO: log invalid message?
            return False

if __name__ == '__main__':
    db = Mydb("test.py")
    db.start()


