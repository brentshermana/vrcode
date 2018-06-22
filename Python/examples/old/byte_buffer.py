class byte_buffer:

    def __init__(self, num_bytes):
        self.bytes_needed = num_bytes
        self.counter = 0
        self.byte_array = bytearray(num_bytes)

    # new_bytes: a bytes-like object
    # returns data which exceeds the bounds of
    # self.num_bytes, or None if no such data exists
    def feed_bytes(self, new_bytes):
        ret = None
        if len(new_bytes) > self.bytes_needed:
            ret = new_bytes[self.bytes_needed:]
            new_bytes = new_bytes[:self.bytes_needed]
        self.byte_array[self.counter:self.counter+len(new_bytes)] = new_bytes
        self.counter += len(new_bytes)
        self.bytes_needed -= len(new_bytes)
        return ret

    def ready(self):
        return self.bytes_needed == 0

    def parse(self):
        if not self.ready():
            raise Exception("Not yet ready to be parsed")
        else:
            return self.processing_action(self.byte_array)

    # to be redefined by subclasses
    def processing_action(self, bytes_like):
        return bytes_like
