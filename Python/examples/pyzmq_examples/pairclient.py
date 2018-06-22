import zmq
import random
import sys
import time

port = "5556"
context = zmq.Context()
socket = context.socket(zmq.PAIR)
socket.connect("tcp://127.0.0.1:%s" % port)

while True:
    msg = socket.recv()
    print(msg)
    socket.send("client message to server".encode())
    time.sleep(1)