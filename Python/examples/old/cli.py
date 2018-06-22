import frontend.Frontend as Frontend

class Cli(Frontend):
    "Qdb Front-end command line interface"

    do_s = Frontend.do_step
    do_n = Frontend.do_next
    do_c = Frontend.do_continue
    do_r = Frontend.do_return
    do_q = Frontend.do_quit

    do_b = do_set_breakpoint
    do_l = do_list
    do_p = do_eval
    do_w = do_where
    do_e = do_environment
    do_j = do_jump

    def __init__(self, pipe):
        Frontend.__init__(self, pipe)

    # redefine Frontend methods:
    def run(self):
        while 1:
            try:
                Frontend.run(self)
            except KeyboardInterrupt:
                print("Interupting...")
                self.interrupt()
    def interaction(self, filename, lineno, line):
        print("> %s(%d)\n-> %s" % (filename, lineno, line), end=' ')
        self.filename = filename
        self.cmdloop()
    def exception(self, title, extype, exvalue, trace, request):
        print("=" * 80)
        print("Exception", title)
        print(request)
        print("-" * 80)
    def write(self, text):
        print(text, end=' ')
    def readline(self):
        return input()
    def do_eval(self, args):
        "Inspect the value of the expression"
        print(Frontend.do_eval(self, args))
    def do_list(self, args):
        "List source code for the current file"
        lines = Frontend.do_list(self, eval(args, {}, {}) if args else None)
        self.print_lines(lines)
    def do_where(self, args):
        "Print a stack trace, with the most recent frame at the bottom."
        lines = Frontend.do_where(self)
        self.print_lines(lines)
    def do_environment(self, args=None):
        env = Frontend.do_environment(self)
        for key in env:
            print("=" * 78)
            print(key.capitalize())
            print("-" * 78)
            for name, value in list(env[key].items()):
                print("%-12s = %s" % (name, value))
    def do_list_breakpoint(self, arg=None):
        "List all breakpoints"
        breaks = Frontend.do_list_breakpoint(self)
        print("Num File                          Line Temp Enab Hits Cond")
        for bp in breaks:
            print('%-4d%-30s%4d %4s %4s %4d %s' % bp)
        print()
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
        if ret:     # show error message if failed
            print("cannot jump:", ret)
    def print_lines(self, lines):
        for filename, lineno, bp, current, source in lines:
            print("%s:%4d%s%s\t%s" % (filename, lineno, bp, current, source), end=' ')
        print()