import asyncore, socket
import sys

import threading

PORT = 5556


class tcpclient_inst(asyncore.dispatcher):

    def __init__(self):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.attempt_connect()
        self.buffer = bytearray(0) # empty bytearray
        self.buffer_lock = threading.Lock()

    def handle_connect_event(self):
        print("client connected")

    def attempt_connect(self):
        self.connect(('localhost', PORT))

    def handle_error(self):
        error_type, error_value, tb = sys.exc_info()
        if not tb:  # Must have a traceback
            raise AssertionError("traceback does not exist")
        print("Error of type {}".format(error_type))
        self.close()


    def senddata(self, data): # data is a bytearray or equivalent
        self.buffer_lock.acquire()
        self.buffer.extend(data)
        self.buffer_lock.release()

    def handle_connect(self):
        pass

    def handle_close(self):
        self.close()

    def handle_read(self):
        data = self.recv(8192)
        print("Client Read {} bytes".format(len(data)))

    def writable(self):
        return (len(self.buffer) > 0)

    def handle_write(self):
        self.buffer_lock.acquire()
        sent = self.send(self.buffer)
        self.buffer = self.buffer[sent:]
        self.buffer_lock.release()
        print("Client sent {} bytes".format(sent))

if __name__ == '__main__':
    client = tcpclient_inst()
    data = bytes("hello", 'latin1')
    client.senddata(data)
    asyncore.loop()