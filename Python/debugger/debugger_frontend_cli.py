import cmd

from debugger.debugger_frontend import Frontend
from errors import DBQuitError, RPCError


class DebuggerFrontendCli(Frontend, cmd.Cmd):
    "Qdb Front-end command line interface"

    def __init__(self, zmq_socket, completekey='tab', stdin=None, stdout=None, skip=None):
        cmd.Cmd.__init__(self, completekey, stdin, stdout)
        Frontend.__init__(self, zmq_socket)

    # redefine Frontend methods:
    def run(self):
        while 1:
            try:
                Frontend.run(self)
            except DBQuitError:
                break
            except RPCError as rpce:
                # I want to know if there's any invalid usage of the protocol
                print("RPC Error: ")
                print(rpce)
                print()
            except KeyboardInterrupt:
                # TODO: this won't work if the debugged program's been terminated
                print("Interupting...")
                self.interrupt()

    def print_env(self, env):
        for scope in env:
            print("=" * 78)
            print(scope.capitalize())
            print("-" * 78)
            for var in env[scope]:
                print("%-12s = %s : %s" % (var['name'], var['val'], var['type']))

    def print_call_stack(self, stack):
        for line in stack['trace']:
            print("{}:{}    {}".format(line['fname'], line['lineno'], line['line']))

    def interaction(self, filename, lineno, line):
        print("> %s(%d)\n-> %s" % (filename, lineno, line))
        self.filename = filename
        self.cmdloop()

    def exception(self, title, extype, exvalue, trace, request):
        print("=" * 80)
        print("Exception {}".format(title))
        print(request)
        print("-" * 80)

    def write(self, text):
        print(text)

    def readline(self):
        return input()

    # https://stackoverflow.com/questions/15537427/how-to-exit-the-cmd-loop-of-cmd-module-cleanly
    # postcmd returns true when you want Cmd.cmdloop() to quit looping
    #   'line' is the input that was just interpreted and executed
    def postcmd(self, stop, line):
        # interpretation: quit the loop if any command besides 'help' was run
        return not line.startswith("h")  # stop

    do_h = cmd.Cmd.do_help

    do_s = Frontend.do_step
    do_n = Frontend.do_next
    do_c = Frontend.do_continue
    do_r = Frontend.do_return
    do_q = Frontend.do_quit

    def do_eval(self, args):
        "Inspect the value of the expression"
        print(Frontend.do_eval(self, args))

    # def do_list(self, args):
    #     "List source code for the current file"
    #     lines = Frontend.do_list(self, eval(args, {}, {}) if args else None)
    #     self.print_lines(lines)

    def do_where(self, args):
        "Print a stack trace, with the most recent frame at the bottom."
        lines = Frontend.do_where(self)
        self.print_call_stack(lines)

    def do_environment(self, args=None):
        env = Frontend.do_environment(self)
        self.print_env(env)

    def do_list_breakpoint(self, arg=None):
        "List all breakpoints"
        breaks = Frontend.do_list_breakpoint(self)
        print("Num File                         Line Temp Enab Hits Cond")
        for bp in breaks:
            print('%-4d%-30s%4d %4s %4s %4d %s' % (
                bp['number'],
                bp['file'],
                bp['lineno'],
                bp['temporary'],
                bp['enabled'],
                bp['hits'],
                bp['condition'],
            ))

    def do_set_breakpoint(self, arg):
        "Set a breakpoint at filename:breakpoint"
        if arg:
            if ':' in arg:
                args = arg.split(":")
            else:
                args = (self.filename, arg)
            Frontend.do_set_breakpoint(self, *args)
        else:
            self.do_list_breakpoint()

    def do_jump(self, args):
        "Jump to the selected line"
        ret = Frontend.do_jump(self, args)
        if ret:  # show error message if failed
            print("cannot jump: {}".format(ret))

    do_b = do_set_breakpoint
    # do_l = do_list
    do_p = do_eval
    do_w = do_where
    do_e = do_environment
    do_j = do_jump

    def default(self, line):
        "Default command"
        if line[:1] == '!':
            print(self.do_exec(line[1:]))
        else:
            print("*** Unknown command: {}".format(line))

    def print_lines(self, lines):
        for filename, lineno, source in lines:
            print("%s:%4d\t%s" % (filename, lineno, source))

# def f(pipe):
#     "test function to be debugged"
#     print ("creating debugger")
#     qdb_test = Qdb(pipe=pipe, redirect_stdio=False, allow_interruptions=True)
#     print ("set trace")
#
#     my_var = "Mariano!"
#     qdb_test.set_trace()
#     print ("hello world!")
#     for i in range(100000):
#         pass
#     print ("good by!")


# def test():
#     "Create a backend/frontend and time it"
#     if '--process' in sys.argv:
#         from multiprocessing import Process, Pipe
#         front_conn, child_conn = Pipe()
#         p = Process(target=f, args=(child_conn,))
#     else:
#         from threading import Thread
#         from queue import Queue
#         parent_queue, child_queue = Queue(), Queue()
#         front_conn = QueuePipe("parent", parent_queue, child_queue)
#         child_conn = QueuePipe("child", child_queue, parent_queue)
#         p = Thread(target=f, args=(child_conn,))
#
#     p.start()
#     import time
#
#     class Test(Frontend):
#         def interaction(self, *args):
#             print ("interaction! {}".format(args))
#             ##self.do_next()
#         def exception(self, *args):
#             print ("exception: {}".format(args))
#
#     qdb_test = Test(front_conn)
#     time.sleep(1)
#     t0 = time.time()
#
#     print ("running...")
#     while front_conn.poll():
#         Frontend.run(qdb_test)
#     qdb_test.do_continue()
#     p.join()
#     t1 = time.time()
#     print ("took {} seconds".format(t1 - t0))
#     sys.exit(0)