import zmq
import random
import sys
import time

port = "12345"
context = zmq.Context()
socket = context.socket(zmq.REQ)
socket.connect("tcp://127.0.0.1:%s" % port)

while True:
    socket.send("client message to server".encode())
    msg = socket.recv()
    print(msg)
    time.sleep(1)