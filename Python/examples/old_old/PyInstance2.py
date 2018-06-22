# (brief) attempt 2 before realizing it was hopeless to use subprocess
from subprocess import Popen, PIPE, TimeoutExpired, STDOUT

class PyInstance2:

    def __init__(self):
        self.popen = Popen(args=["python3", "-m", "pdb", "test.py"], stdin=PIPE, stdout=PIPE, stderr=PIPE, shell=True)
        assert(not self.popen.stdout.isatty()) # assume not interactive

    def instance(self):
        return self.popen

    def communicate(self, command):
        self.popen.stdin.write(bytes(command+"\n", "latin1"))
        self.popen.stdin.flush()
        output = []
        while True:
            new_bytes = self.popen.stdout.readline()
            if len(new_bytes) is 0:
                break;
            output.extend(output, new_bytes)
            print(output.decode("latin1")) # for debugging

    def endSession(self):
        (output, err) = self.popen.communicate(input="quit()")
        try:
            self.popen.wait(timeout=1.0)
        except TimeoutExpired:
            self.popen.kill()

if __name__ == "__main__":
    meta = PyInstance2()
    try:
        while (True):
            command = input("Next?")
            out, err = meta.communicate(command)
            if (out):
                print("Output: " + out.decode("latin1"))
            if (err):
                print("Error: " + err.decode("latin1"))
    except KeyboardInterrupt:
        meta.endSession()
        print("Session Ended")


