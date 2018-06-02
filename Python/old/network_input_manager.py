from json_utils import json_buffer
from int_buffer import int_buffer

class network_input_manager:

    def __init__(self, json_queue, bytes_for_size, int_byteorder, int_signed):
        self.json_queue = json_queue
        self.json_buffer = None
        self.int_signed = int_signed

        self.bytes_for_size = bytes_for_size
        self.int_byteorder = int_byteorder
        self.refresh_size_buffer()

    def refresh_size_buffer(self):
        self.int_buffer = int_buffer(self.bytes_for_size, self.int_byteorder, self.int_signed)

    def feed_bytes(self, bytes_like):
        while bytes_like and len(bytes_like) > 0:
            if not self.json_buffer: #don't currently have size
                bytes_like = self.size_buffer.feed_bytes(bytes_like)
                if self.size_buffer.ready():
                    size = self.size_buffer.parse()
                    self.json_buffer = json_buffer(size)
                    self.size_buffer = None
            else:
                bytes_like = self.json_buffer.feed_bytes(bytes_like)
                if self.json_buffer.ready():
                    json = self.json_buffer.parse()
                    self.json_queue.put(json)
                    self.json_buffer = None
                    self.refresh_size_buffer()
