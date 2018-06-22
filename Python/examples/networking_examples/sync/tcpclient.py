import socket

IP = '0.0.0.0'
PORT = 5556
START_SIZE = 100

clientsocket = socket.socket()
clientsocket.connect((IP, PORT))

buf = bytearray(START_SIZE)
index = 0
done = False
while not done:
    incoming = clientsocket.recv(4000)
    for b in incoming:
        if done:
            break
        if index == len(buf):
            buf.extend(bytearray(len(buf))) # double size
        if b == 1:
            done = True
        else:
            buf[index] = b
        index += 1

buf = buf[0:index] # remove empty space
print("client done reading")

clientsocket.sendall(buf)
print("client done sending")

clientsocket.close()