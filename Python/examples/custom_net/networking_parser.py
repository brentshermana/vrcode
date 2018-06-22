import queue

BYTES_FOR_LEN = 4

editor_actions = {
    1 : 'delete',
    2 : 'add_at',
    3 : 'change'
}

debug_actions = {
    1 : 'add_breakpoint',
    2 : 'remove_breakpoint',
    3 : 'continue',
    4 : 'next'
}

type_id_to_action_list = {
    1 : editor_actions,
    2 : debug_actions
}


class parser:

    def __init__(self):
        self.json_buffer = bytearray()
        self.len_buffer = bytearray()
        self.msg_len = None
        self.parsed_msgs = queue.Queue()

    def feed_data(self, somebytes):
        while len(somebytes > 0):
            bytes_used = None
            if self.msg_len: # we've fully parsed the length - just get the json data
                bytes_used = min(len(self.json_buffer) - self.msg_len, somebytes)
            else: # attempt to finish the length
                bytes_used =  min(len(somebytes), BYTES_FOR_LEN-len(self.len_buffer))# ... to append to the len_buffer
                self.len_buffer.extend(somebytes[:bytes_used])
                if len(self.len_buffer) == BYTES_FOR_LEN:
                    self.msg_len = int.from_bytes(self.len_buffer, byteorder='big')
                # don't yet have the four bytes representing the length


    def read_and_erase(self):
        pass



