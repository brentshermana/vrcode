from json import JSONDecoder as decoder
from json import JSONEncoder as encoder

# import byte_buffer

ENCODING = "utf-8"

#returns A STRING, not bytes
def encode_json(json_obj):
    """
    :param json_obj: a json object
    :return: a string
    """
    return encoder().encode(json_obj)

def decode_json(str):
    """
    :param str: string holding a json message
    :return: json object
    """
    return decoder().decode(str)


# class json_buffer(byte_buffer.byte_buffer):
#
#     def __init__(self, num_bytes):
#         super.__init__(num_bytes)
#
#     def processing_action(self, bytes_like):
#         return decode_json(bytes_like)


