import bdb
import inspect
import linecache
import pydoc

import sys

import os
import threading
import traceback

import zmq

from errors import RPCError
from zmq_wrapper import ZmqRpcIO


class Qdb(bdb.Bdb, ZmqRpcIO):
    """
    Remote debugger backend
    """

    def __init__(self, zmq_socket, redirect_stdio=True, allow_interruptions=False,
                 use_speedups=True, skip=[__name__]):
        kwargs = {}
        if sys.version_info > (2, 7):
            kwargs['skip'] = skip
        bdb.Bdb.__init__(self, **kwargs)
        self.frame = None
        self.waiting = False
        self.zmq_socket = zmq_socket  # for communication
        ZmqRpcIO.__init__(self, self.zmq_socket)

        self.poller = zmq.Poller()
        self.poller.register(self.zmq_socket, zmq.POLLIN)
        self._wait_for_mainpyfile = False
        self._wait_for_breakpoint = False
        self.mainpyfile = ""
        self._lineno = None  # last listed line numbre
        # ignore filenames (avoid spurious interaction specially on py2)
        self.ignore_files = [self.canonic(f) for f in (__file__, bdb.__file__)]
        # replace system standard input and output (send them thru the pipe)
        self.old_stdio = sys.stdin, sys.stdout, sys.stderr
        if redirect_stdio:
            sys.stdin = self
            sys.stdout = self
            sys.stderr = self
        self.allow_interruptions = allow_interruptions
        self.burst = 0  # do not send notifications ("burst" mode)
        self.params = {}  # optional parameters for interaction

        # flags to reduce overhead (only stop at breakpoint or interrupt)
        self.use_speedups = use_speedups
        self.fast_continue = False

    def pull_actions(self):
        """
        receive a remote procedure call from the frontend

        :returns: True if the action was processed
        :returns: None when 'run' notification is received (see 'startup')
        """

        request = self.recv()

        if request.get('method') == 'run':
            return None

        response = {
            'jsonrpc': '1.1',
            'id': request.get('id'),
            'method': request.get('method'),
            'result': None,
            'error': None
        }
        try:
            # dispatch message (JSON RPC like)
            method = getattr(self, request['method'])
            response['result'] = method.__call__(*request['args'], )
        except Exception as e:
            response['error'] = {'code': 0, 'message': str(e)}
        # send the result for normal method calls, not for notifications
        if request.get('id'):
            self.send(response)
        return True

    def poll_socket(self):
        return len(self.poller.poll(1)) > 0

    # Override Bdb methods

    def trace_dispatch(self, frame, event, arg):
        # check for non-interaction rpc (set_breakpoint, interrupt)
        while self.allow_interruptions and self.poll_socket():  # one millisecond timeout for poll
            self.pull_actions()
            # check for non-interaction rpc (set_breakpoint, interrupt)
            while self.poll_socket():
                self.pull_actions()
        if not self.break_here(frame) and self.fast_continue:
            return self.trace_dispatch
        # process the frame (see Bdb.trace_dispatch)
        ##if self.fast_continue:
        ##    return self.trace_dispatch
        if self.quitting:
            return  # None
        if event == 'line':
            return self.dispatch_line(frame)
        if event == 'call':
            return self.dispatch_call(frame, arg)
        if event == 'return':
            return self.dispatch_return(frame, arg)
        if event == 'exception':
            return self.dispatch_exception(frame, arg)
        return self.trace_dispatch

    def user_call(self, frame, argument_list):
        """This method is called when there is the remote possibility
        that we ever need to stop in this function."""
        if self._wait_for_mainpyfile or self._wait_for_breakpoint:
            return
        if self.stop_here(frame):
            self.interaction(frame)

    def user_line(self, frame):
        """This function is called when we stop or break at this line."""
        if self._wait_for_mainpyfile:
            if (not self.canonic(frame.f_code.co_filename).startswith(self.mainpyfile) or frame.f_lineno <= 0):
                return
            self._wait_for_mainpyfile = 0
        if self._wait_for_breakpoint:
            if not self.break_here(frame):
                return
            self._wait_for_breakpoint = 0
        self.interaction(frame)

    def user_exception(self, frame, info):
        """This function is called if an exception occurs,
        but only if we are to stop at or just below this level."""
        if self._wait_for_mainpyfile or self._wait_for_breakpoint:
            return
        extype, exvalue, trace = info
        # pre-process stack trace as it isn't pickeable (cannot be sent pure)
        msg = ''.join(traceback.format_exception(extype, exvalue, trace))
        trace = traceback.extract_tb(trace)
        title = traceback.format_exception_only(extype, exvalue)[0]
        # send an Exception notification
        msg = {'method': 'exception',
               'args': (title, extype.__name__, str(exvalue), str(trace), msg),
               'id': -1}
        self.send(msg)
        self.interaction(frame)

    def run(self, code, interp=None, *args, **kwargs):
        try:
            return bdb.Bdb.run(self, code, *args, **kwargs)
        finally:
            pass

    def runcall(self, function, interp=None, *args, **kwargs):
        try:
            self.interp = interp
            return bdb.Bdb.runcall(self, function, *args, **kwargs)
        finally:
            pass

    def _runscript(self, filename):
        # The script has to run in __main__ namespace (clear it)
        import __main__
        import imp
        __main__.__dict__.clear()
        __main__.__dict__.update({"__name__": "__main__",
                                  "__file__": filename,
                                  "__builtins__": __builtins__,
                                  "imp": imp,  # need for run
                                  })

        # avoid stopping before we reach the main script
        self._wait_for_mainpyfile = 1
        self.mainpyfile = self.canonic(filename)
        self._user_requested_quit = 0
        if sys.version_info > (3, 0):
            statement = 'imp.load_source("__main__", "%s")' % filename
        else:
            statement = 'execfile(%r)' % filename
        self.startup()
        self.run(statement)

    def startup(self):
        """
        Send frontend the debugger's initial parameters.
        Processes incoming messages until the frontend
        calls 'run'.

        This is a good opportunity for the frontend to
        set initial breakpoints, etc.
        """

        # send some useful info to identify session
        thread = threading.current_thread()
        # get the caller module filename
        frame = sys._getframe()
        fn = self.canonic(frame.f_code.co_filename)
        while frame.f_back and self.canonic(frame.f_code.co_filename) == fn:
            frame = frame.f_back

        args = ["version", os.getpid(), thread.name, " ".join(sys.argv),
                frame.f_code.co_filename]
        self.send({'method': 'startup', 'args': args})
        while self.pull_actions() is not None:
            pass

    # General interaction function
    def interaction(self, frame):
        # cache frame locals to ensure that modifications are not overwritten
        self.frame_locals = frame and frame.f_locals or {}
        # extract current filename and line number
        code, lineno = frame.f_code, frame.f_lineno
        filename = self.canonic(code.co_filename)
        basename = os.path.basename(filename)
        # check if interaction should be ignored (i.e. debug modules internals)
        if filename in self.ignore_files:
            return
        message = "%s:%s" % (basename, lineno)
        if code.co_name != "?":
            message = "%s: %s()" % (message, code.co_name)

        # wait user events
        self.waiting = True
        self.frame = frame
        try:
            while self.waiting:
                #  sync_source_line()
                if frame and filename[:1] + filename[-1:] != "<>" and os.path.exists(filename):
                    line = linecache.getline(filename, self.frame.f_lineno,
                                             self.frame.f_globals)
                else:
                    line = ""
                # send the notification (debug event) - DOESN'T WAIT RESPONSE
                self.burst -= 1
                if self.burst < 0:
                    self.send(
                        {'method': 'interaction',
                         'id': -1,
                         'args': (filename, self.frame.f_lineno, line)
                         }
                    )

                self.pull_actions()
        finally:
            self.waiting = False
        self.frame = None

    def do_debug(self, mainpyfile=None, wait_breakpoint=1):
        self.reset()
        if not wait_breakpoint or mainpyfile:
            self._wait_for_mainpyfile = 1
            if not mainpyfile:
                frame = sys._getframe().f_back
                mainpyfile = frame.f_code.co_filename
            self.mainpyfile = self.canonic(mainpyfile)
        self._wait_for_breakpoint = wait_breakpoint
        sys.settrace(self.trace_dispatch)

    def set_trace(self, frame=None):
        # start debugger interaction immediately
        if frame is None:
            frame = sys._getframe().f_back
        self._wait_for_mainpyfile = frame.f_code.co_filename
        self._wait_for_breakpoint = 0
        # reinitialize debugger internal settings
        self.fast_continue = False
        bdb.Bdb.set_trace(self, frame)

    # Command definitions, called by interaction()

    def do_continue(self):
        self.set_continue()
        self.waiting = False
        self.fast_continue = self.use_speedups

    def do_step(self):
        self.set_step()
        self.waiting = False
        self.fast_continue = False

    def do_return(self):
        self.set_return(self.frame)
        self.waiting = False
        self.fast_continue = False

    def do_next(self):
        self.set_next(self.frame)
        self.waiting = False
        self.fast_continue = False

    def interrupt(self):
        self.set_trace()
        self.fast_continue = False

    def do_quit(self):
        self.set_quit()
        self.waiting = False
        self.fast_continue = False

    def do_jump(self, lineno):
        arg = int(lineno)
        try:
            self.frame.f_lineno = arg
        except ValueError as e:
            return str(e)

    ### function's purpose is just to print out the current file's source, so we don't really care about it
    # def do_list(self, arg):
    #     last = None
    #     if arg:
    #         if isinstance(arg, tuple):
    #             first, last = arg
    #         else:
    #             first = arg
    #     elif not self._lineno:
    #         first = max(1, self.frame.f_lineno - 5)
    #     else:
    #         first = self._lineno + 1
    #     if last is None:
    #         last = first + 10
    #     filename = self.frame.f_code.co_filename
    #     breaklist = self.get_file_breaks(filename)
    #     lines = []
    #     for lineno in range(first, last+1):
    #         line = linecache.getline(filename, lineno,
    #                                  self.frame.f_globals)
    #         if not line:
    #             lines.append((filename, lineno, '', "", "<EOF>\n"))
    #             break
    #         else:
    #             breakpoint = "B" if lineno in breaklist else ""
    #             current = "->" if self.frame.f_lineno == lineno else ""
    #             lines.append((filename, lineno, breakpoint, current, line))
    #             self._lineno = lineno
    #     return lines

    def do_read(self, filename):
        return open(filename, "Ur").read()

    def do_set_breakpoint(self, filename, lineno, temporary=0, cond=None):
        return self.set_break(filename, int(lineno), temporary, cond)

    def do_list_breakpoint(self):
        breaks = []
        for bp in bdb.Breakpoint.bpbynumber:
            if bp:
                breaks.append({
                    'number': bp.number,
                    'file': bp.file,
                    'lineno': bp.line,
                    'temporary': bp.temporary,
                    'enabled': bp.enabled,
                    'hits': bp.hits,
                    'condition': bp.cond
                })
        return breaks

    def do_clear_breakpoint(self, filename, lineno):
        self.clear_break(filename, int(lineno))

    def do_clear_file_breakpoints(self, filename):
        self.clear_all_file_breaks(filename)

    def do_clear(self, arg):
        # required by BDB to remove temp breakpoints!
        err = self.clear_bpbynumber(arg)
        if err:
            print('*** DO_CLEAR failed {}'.format(err))

    def do_eval(self, arg):
        if self.frame:
            ret = eval(arg, self.frame.f_globals,
                       self.frame_locals)
        else:
            ret = RPCError("No current frame available to eval")
        # if safe: # was originally an optional second argument
        ret = pydoc.cram(repr(ret), 255)
        return ret

    def do_exec(self, arg):
        if not self.frame:
            ret = RPCError("No current frame available to exec")
        else:
            locals = self.frame_locals
            globals = self.frame.f_globals
            code = compile(arg + '\n', '<stdin>', 'single')
            save_displayhook = sys.displayhook
            self.displayhook_value = None
            try:
                sys.displayhook = self.displayhook
                # exec code in globals, locals
                exec(code, globals, locals)
                ret = self.displayhook_value
            finally:
                sys.displayhook = save_displayhook
        # if safe: # was originally optional
        ret = pydoc.cram(repr(ret), 255)
        return ret

    def do_where(self):
        "print_stack_trace"
        stack, curindex = self.get_stack(self.frame, None)
        lines = {'trace': []}
        for frame, lineno in stack:
            filename = frame.f_code.co_filename
            line = linecache.getline(filename, lineno)
            lines['trace'].append({'fname': filename, 'lineno': lineno, 'line': line})
        return lines

    def do_environment(self):
        "return current frame local and global environment"
        env = {'locals': [], 'globals': []}
        # converts the frame global and locals to a short text representation:
        if self.frame:
            for scope, max_length, names, values in (
                    ("locals", 255, self.frame_locals.keys(), self.frame_locals.values()),
                    ("globals", 20, self.frame.f_globals.keys(), self.frame.f_globals.values())):
                for name, value in zip(names, values):
                    try:
                        short_repr = pydoc.cram(repr(value), max_length)
                    except Exception as e:
                        # some objects cannot be represented...
                        short_repr = "**exception** %s" % repr(e)
                    env[scope].append({'name': name, 'val': short_repr, 'type': repr(type(value))})
        return env

    # def get_autocomplete_list(self, expression):
    #     "Return list of auto-completion options for expression"
    #     try:
    #         obj = self.do_eval(expression, safe=False)
    #     except:
    #         return []
    #     else:
    #         return dir(obj)

    def get_call_tip(self, expression):
        "Return list of auto-completion options for expression"
        try:
            obj = self.do_eval(expression)
        except Exception as e:
            return ('', '', str(e))
        else:
            name = ''
            try:
                name = obj.__name__
            except AttributeError:
                pass
            argspec = ''
            drop_self = 0
            f = None
            try:
                if inspect.isbuiltin(obj):
                    pass
                elif inspect.ismethod(obj):
                    # Get the function from the object
                    f = obj.im_func
                    drop_self = 1
                elif inspect.isclass(obj):
                    # Get the __init__ method function for the class.
                    if hasattr(obj, '__init__'):
                        f = obj.__init__.im_func
                    else:
                        for base in object.__bases__:
                            if hasattr(base, '__init__'):
                                f = base.__init__.im_func
                                break
                    if f is not None:
                        drop_self = 1
                elif callable(obj):
                    # use the obj as a function by default
                    f = obj
                    # Get the __call__ method instead.
                    f = obj.__call__.im_func
                    drop_self = 0
            except AttributeError:
                pass
            if f:
                argspec = apply(inspect.formatargspec, inspect.getargspec(f))
            doc = ''
            if callable(obj):
                try:
                    doc = inspect.getdoc(obj)
                except:
                    pass
            return (name, argspec[1:-1], doc.strip())

    def set_burst(self, val):
        "Set burst mode -multiple command count- (shut up notifications)"
        self.burst = val

    def set_params(self, params):
        "Set parameters for interaction"
        self.params.update(params)

    def displayhook(self, obj):
        """Custom displayhook for the do_exec which prevents
        assignment of the _ variable in the builtins.
        """
        self.displayhook_value = repr(obj)

    def reset(self):
        bdb.Bdb.reset(self)
        self.waiting = False
        self.frame = None

    def post_mortem(self, info=None):
        "Debug an un-handled python exception"
        # check if post mortem mode is enabled:
        if not self.params.get('postmortem', True):
            return
        # handling the default
        if info is None:
            # sys.exc_info() returns (type, value, traceback) if an exception is
            # being handled, otherwise it returns None
            info = sys.exc_info()
        # extract the traceback object:
        t = info[2]
        if t is None:
            raise ValueError("A valid traceback must be passed if no "
                             "exception is being handled")
        self.reset()
        # get last frame:
        while t is not None:
            frame = t.tb_frame
            t = t.tb_next
            code, lineno = frame.f_code, frame.f_lineno
            filename = code.co_filename
            line = linecache.getline(filename, lineno)
            # (filename, lineno, "", current, line, )}
        # SyntaxError doesn't execute even one line, so avoid mainpyfile check
        self._wait_for_mainpyfile = False
        # send exception information & request interaction
        self.user_exception(frame, info)

    def ping(self):
        "Minimal method to test that the pipe (connection) is alive"
        try:
            # get some non-trivial data to compare:
            args = (id(object()),)
            msg = {'method': 'ping', 'args': args, 'id': -1}
            self.send(msg)
            msg = self.recv()
            # check if data received is ok (alive and synchonized!)
            success = msg['result'] == args[0]
            if not success:
                self.old_stdio[0].write("arg '{}' do not equal ping's return '{}'\n".format(args[0], msg['result']))
            else:
                self.old_stdio[0].write("ping success!\n")
            return success
        except (IOError, EOFError):
            return None

    # Stdin, Stdout, Stderr are all just file-like objects.
    # The following methods implement that interface
    # so we can forward stdio requests over the network
    def readline(self):
        "Replacement for stdin.readline()"
        msg = {'method': 'readline', 'ARGS': (), 'id': -1}
        self.send(msg)
        msg = self.recv()
        return msg['result']

    def readlines(self):
        "Replacement for stdin.readlines()"
        lines = []
        while lines[-1:] != ['\n']:
            lines.append(self.readline())
        return lines

    def write(self, text):
        "Replacement for stdout.write()"
        msg = {'method': 'write', 'args': (text,), 'id': -1}
        self.send(msg)

    def writelines(self, l):
        map(self.write, l)

    def flush(self):
        pass

    def isatty(self):
        return 0

    def encoding(self):
        return None  # use default, 'utf-8' should be better...

    def close(self):
        # revert redirections and close connection
        sys.stdin, sys.stdout, sys.stderr = self.old_stdio
        try:
            self.zmq_socket.close()
        except:
            pass

    def __del__(self):
        self.close()