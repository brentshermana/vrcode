import threading
import asyncore
import sys
import socket
from json_utils import encode_json
from network_input_manager import network_input_manager

PORT = 5555

BYTES_FOR_SIZE = 4
BYTEORDER = 'big'
SIGNED = False

class iothread(threading.Thread, asyncore.dispatcher):

    def __init__(self, in_queue, out_queue):
        print("io thread start")
        threading.Thread.__init__(self, daemon=True)
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)


        self.json_in_queue = in_queue
        self.json_out_queue = out_queue

        self.network_input_manager = network_input_manager(
            self.json_in_queue,
            BYTES_FOR_SIZE,
            BYTEORDER,
            SIGNED
        )

        self.out_bytes = bytearray(0)

    #for threading.Thread:
    def run(self):
        self.connect( ('localhost', PORT) )
        asyncore.loop()

    #for asyncore.dispatcher:
    def handle_connect_event(self):
        pass
    def handle_connect(self):
        pass

    def handle_error(self):
        error_type, error_value, tb = sys.exc_info()
        if not tb:  # Must have a traceback
            raise AssertionError("traceback does not exist")
        print("Error of type {}".format(error_type))
        self.close()

    def handle_close(self):
        self.close()

    def handle_read(self):
        data = self.recv(8192)
        self.network_input_manager.feed_bytes(data)

    def writable(self):
        # check if there is any new json. If so, turn into bytes and add to buffer
        while self.json_out_queue.size() > 0:
            json_obj = self.json_out_queue.get()
            json_encoded = encode_json(json_obj)
            json_obj_size = len(json_encoded)
            size_bytes = json_obj_size.to_bytes(BYTES_FOR_SIZE, BYTEORDER, SIGNED)
            self.out_bytes.extend(size_bytes)
            self.out_bytes.extend(json_encoded)
        return len(self.out_bytes) > 0

    def handle_write(self):
        sent = self.send(self.out_bytes)
        self.out_bytes = self.buffer[sent:]