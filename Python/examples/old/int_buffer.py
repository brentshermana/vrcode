import byte_buffer

class int_buffer(byte_buffer.byte_buffer):

    #byteorder is 'big' or 'little'
    #signed is a bool
    def __init__(self, bytes_for_int, byteorder, signed):
        super().__init__(bytes_for_int)
        self.byteorder = byteorder
        self.signed = signed

    def processing_action(self, bytes_like):
        return int.from_bytes(bytes_like, self.byteorder, self.signed)