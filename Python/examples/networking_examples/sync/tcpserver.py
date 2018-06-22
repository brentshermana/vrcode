# adapted from https://docs.python.org/2/howto/sockets.html
import socket

IP = '0.0.0.0'
PORT = 5556

BYTES_TO_SEND = 1000000

serversocket = socket.socket()
# I believe this is what makes it a server socket
serversocket.bind(('localhost', PORT))
serversocket.listen(1) # will only queue one client
clientsocket, clientaddress = serversocket.accept()
print("Server connected to client")

data = bytearray(BYTES_TO_SEND) # creates that many bytes, all zero

clientsocket.sendall(data)
#send end of message signal (just a 1)
data = bytearray(1)
data[0] = 1
clientsocket.sendall(data)
print("Server sent data")
incoming_data = bytearray(BYTES_TO_SEND) # expect client to send same ammount back
i = 0
while i < len(incoming_data):
    temp = clientsocket.recv(4000) #read up to 4kb at once
    incoming_data[i:i+len(temp)] = temp
    i += len(temp)

print("Server done reading")

clientsocket.close()
serversocket.close()




