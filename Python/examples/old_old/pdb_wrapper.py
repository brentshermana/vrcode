# This attempt has been better, but incomplete. I realized I need to dig in to the pdb source itself

from subprocess import Popen, PIPE, TimeoutExpired
from threading import Thread, Lock
import time

class PDBInstance:

    def readFromBufferedReaderForever(self, bufReader, readInto):
        while (True):
            # need another lock here
            with readInto.lock:
                readInto.extend(bufReader.read(1).decode("latin1")) # once they're in the queue, they are converted into characters

    def __init__(self):
        self.popen = Popen(args=["python3", "-m", "pdb", "test.py"], stdin=PIPE, stdout=PIPE) # , stdout=PIPE, stderr=PIPE
        self.__PDB_PROMPT = ['(', 'P', 'd', 'b',')', ' '] # printed by pdb every time
        self.__STDOUT_LOCK = Lock()
        self.__STDERR_LOCK = Lock()
        self.stdoutBuffer = []
        self.stderrBuffer = []

    def run_cmd(self, command):
        self.popen.stdin.write(bytes(command, "latin1"))
        self.popen.stdin.flush()
        self.

    def read(self):
        readBytes = []
        prevLen = -1
        while (prevLen != len(readBytes)):
            prevLen = len(readBytes) ### TODO: left off here, about to implement new read functionality
            ### we want to do the actual reading in a separate thread, started from the constructor!
            readBytes.extend(self.popen.stdout.read())
        print("Read: " + )
        print("line: " + self.popen.stdout.readline().decode("latin1"))

    def endSession(self):
        self.popen.kill()
        # (output, err) = self.popen.communicate(input="quit()")
        # try:
        #     self.popen.wait(timeout=1.0)
        # except TimeoutExpired:
        #     self.popen.kill()

if __name__ == "__main__":
    db = PDBInstance()
    try:
        while (True):
            time.sleep(5)
            command = "n" # run next line of program
            db.run_cmd(command)
            db.read()
            db.read()
    except KeyboardInterrupt:
        db.endSession()
        print("Session Interrupted")