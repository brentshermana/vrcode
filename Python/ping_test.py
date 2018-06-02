import zmq

import time

import zmq_rpc_io

def a():
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    zmq_socket.bind("tcp://*:{}".format("12345")) # todo: can we change the hostname?

    try:
        while True:
            zmq_socket.recv()
            zmq_socket.send(bytes(1000))
    except KeyboardInterrupt:
        zmq_socket.close()


def b():
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    zmq_socket.connect("tcp://{}:{}".format('127.0.0.1', "12345"))

    try:
        while True:
            before = time.clock()

            zmq_socket.send(bytes(1000))
            s = zmq_socket.recv()

            print(s)

            after = time.clock()

            print("Ping: {}".format(after - before))

            time.sleep(1)
    except KeyboardInterrupt:
        zmq_socket.close()

if __name__ == '__main__':
    option = input("a or b? =>")
    if option == 'a':
        a()
    elif option == 'b':
        b()

