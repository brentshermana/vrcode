import zmq
import threading
import sys
import traceback
from json_utils import encode_json
from ActionableJsonMessage import makeMessage, messageString

IP = '127.0.0.1'
pull_status_port = 12346
push_controls_port = 12347
pull_output_port = 12348
rep_stdin_port = 12349

class dbtest:
    def __init__(self):

        self.stdout = sys.stdout

        self.encode_json = encode_json

        self.context = zmq.Context()

        self.pull_status = self.context.socket(zmq.PULL)
        self.pull_status.connect("tcp://{}:{}".format(IP, pull_status_port))

        self.pull_output = self.context.socket(zmq.PULL)
        self.pull_output.connect("tcp://{}:{}".format(IP, pull_output_port))

        self.push_controls = self.context.socket(zmq.PUSH)
        self.push_controls.bind("tcp://{}:{}".format(IP, push_controls_port))

        self.rep_stdin = self.context.socket(zmq.REP)
        self.rep_stdin.bind("tcp://{}:{}".format(IP, rep_stdin_port))

        self.timeout = 50 #50 millis
        self.poller = zmq.Poller()
        self.poller.register(self.pull_status, flags=zmq.POLLIN)
        self.poller.register(self.pull_output, flags=zmq.POLLIN)
        self.poller.register(self.rep_stdin, flags=zmq.POLLIN)

    def start(self):
        self.readthread = threading.Thread(target=self.loop)
        self.readthread.start()
        # self.writethread = threading.Thread(target=self.writeloop)
        # self.writethread.start()

    # def writeloop(self): # write from PULL sockets
    #     while True:
    #         d = dict(self.poller.poll()) #blocks until at least one socket is readable
    #         if self.pull_output in d:
    #             output_str = self.pull_output.recv_string(encoding='utf-8')
    #             self.stdout.write(output_str)
    #         if self.pull_status in d:
    #             status_msg = self.pull_status.recv_json()
    #             print(messageString(status_msg))
    #         if self.rep_stdin in d:
    #             self.rep_stdin.recv_string(encoding='utf-8') #ignore value
    #             reply = input("backend asking for stdin-> ")
    #             self.rep_stdin.send_string(reply, encoding='utf-8')



    def loop(self):
        while True:
            #read stuff:
            read_something = True
            while read_something:
                d = dict(self.poller.poll(self.timeout))
                read_something = len(d) > 0
                if self.pull_output in d:
                    output_str = self.pull_output.recv_string(encoding='utf-8')
                    self.stdout.write(output_str)
                if self.pull_status in d:
                    status_msg = self.pull_status.recv_json()
                    print(messageString(status_msg))
                if self.rep_stdin in d:
                    self.rep_stdin.recv_string(encoding='utf-8')  # ignore value
                    reply = input("backend asking for stdin-> ")
                    self.rep_stdin.send_string(reply, encoding='utf-8')

            # write something
            input_str = input("=> ")
            segments = input_str.split(" ")
            try:
                if segments[0] == 'stdin':
                    # expecting one more component: the input
                    if len(segments) == 1:
                        self.rep_stdin.send_string('/n', encoding='utf-8')
                    if len(segments) > 1:
                        message = ''.join(segments[1:])
                        if message[-1] != '/n':
                            message += '/n'
                        print("Sending stdin: " + message)
                        self.rep_stdin.send_string(message, encoding='utf-8')
                elif segments[0] == 'control':
                    # for File command, we want to have multiple lines.
                    if (segments[2] == 'File'):
                        segments[4:] = map( lambda l : l+"\n", segments[4:] )
                        segments[4] = ''.join(segments[4:])
                    msg = makeMessage(segments[1], segments[2], segments[3:5])
                    print(msg)
                    self.push_controls.send_string(self.encode_json(msg))
            except:
                print(traceback.format_exc())


if __name__ == '__main__':
    dbtest().start()