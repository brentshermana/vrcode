from zmq_wrapper import init_backend_socket, ZmqBackendCmdLoop
import jedi


class FileEditorBackend(ZmqBackendCmdLoop):
    """
    Encapsulates functions for manipulating a text file.

    As with most text editors, rows are index-1 based
    and columns are index-0 based.
    """

    def __init__(self, host='localhost', port=5999, service_name='File Editor'):
        zmq_socket = init_backend_socket(host=host, port=port, service_name=service_name)
        super().__init__(zmq_socket)
        self.lines = []

    def do_save(self, file_path):
        f = open(file_path, 'wt')
        f.writelines(self.lines)
        f.close()

    def on_cursor_change(self, lineno, colno):
        """
        To be overridden by subclasses
        """
        print('=' * 40)
        for line in self.lines:
            print(line)
        return True

    def do_set_cursor(self, lineno, colno):
        self.ensure_line_exists(lineno)
        self.ensure_col_exists(lineno, colno)
        return self.on_cursor_change(lineno, colno)


    def line_exists(self, lineno):
        return lineno >= 1 and lineno <= len(self.lines)

    def ensure_line_exists(self, lineno):
        if lineno < 1:
            raise Exception("Line numbers start at 1! ({} given)".format(lineno))
        while lineno > len(self.lines):
            self.lines.append('')

    def ensure_col_exists(self, lineno, colno):
        self.ensure_line_exists(lineno)
        if len(self.lines[lineno-1]) < colno:
            diff = colno - len(self.lines[lineno-1])
            self.lines[lineno-1] += " " * diff

    # def do_replace_line(self, lineno, new_line):
    #     """
    #     I don't think we'll need this
    #     TODO: remove?
    #     """
    #     self.ensure_line_exists(lineno)
    #     self.lines[lineno-1] = new_line

    def do_newline(self, lineno, col):
        self.ensure_line_exists(lineno)

        # anything to the right of the cursor is carried over to the next line
        carryover = self.lines[lineno-1][col+1:]
        self.lines[lineno-1] = self.lines[lineno-1][:col]
        self.lines.insert(lineno, carryover)

        return self.on_cursor_change(lineno+1, 0)

    def do_insert(self, lineno, col, text):
        """
        to keep code simple, text must not contain newlines. do_newline is for inserting a newline
        """
        if len(text) == 0: return

        lines_to_insert = text.splitlines()
        if len(lines_to_insert) > 1 or lines_to_insert[0] != text:
            raise Exception("The text contained newlines!")

        self.ensure_col_exists(lineno, col)
        cur_line = self.lines[lineno-1]
        self.lines[lineno-1] = cur_line[:col] + text + cur_line[col:]

        return self.on_cursor_change(lineno, col+len(text))



    def do_set_source(self, source):
        self.lines = source.splitlines()
        return True

    def do_backspace(self, lineno, col):
        if self.line_exists(lineno):
            if col == 0:
                if lineno == 1:
                    # at beginning of file, no effect
                    return
                else:
                    # contents of cursor's line is appended to line above
                    self.lines[lineno-2] = self.lines[lineno-1]
                    del self.lines[lineno-1]
                    return self.on_cursor_change(lineno-1, len(self.lines[lineno-2]))
            else:
                line = self.lines[lineno-1]
                self.lines[lineno-1] = line[:col-1] + line[col:]
                return self.on_cursor_change(lineno, col-1)

class JediBackend(FileEditorBackend):
    """
    Adds autocompletion functionality to a file editor
    """

    def __init__(self, host='localhost', port=5999, default_completions=5):
        super().__init__(host=host, port=port, service_name="Jedi")
        self.num_completions = default_completions

    def do_set_num_completions(self, num_completions):
        self.num_completions = num_completions
        return True

    def on_cursor_change(self, lineno, colno):
        super().on_cursor_change(lineno, colno)
        script = jedi.Script('\n'.join(self.lines), lineno, colno)

        json_completions = [] # list of {name: foo, completion:bar})
        for completion in script.completions()[:self.num_completions]:
            json_completions.append({'name':completion.name, 'complete':completion.complete})
        return json_completions

