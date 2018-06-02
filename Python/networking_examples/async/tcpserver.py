import asyncore
import socket

PORT = 5556

class clienthandler(asyncore.dispatcher_with_send):
    # just echoes whatever it recieved
    def handle_read(self):
        data = self.recv(8192)
        if data:
            self.send(data)

class tcpserver(asyncore.dispatcher):

    def __init__(self):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.set_reuse_addr()
        self.bind(('localhost', PORT))
        self.listen(5)

    def handle_accept(self):
        pair = self.accept()
        if pair is not None:
            sock, addr = pair
            print( 'Incoming connection from %s' % repr(addr))
            handler = clienthandler(sock)

if __name__ == '__main__':
    server = tcpserver()
    asyncore.loop()