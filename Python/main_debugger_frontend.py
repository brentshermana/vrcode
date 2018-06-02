import zmq
from debugger_frontend_cli import DebuggerFrontendCli

def run_cli_frontend(port=6000, authkey=b'secret password'):
    "Start the CLI server and wait connection from a running debugger backend"
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    zmq_socket.bind("tcp://*:{}".format(port))
    # wait until the first message is sent - that's the handshake
    zmq_socket.recv()
    zmq_socket.send(b'handshake')
    print ('qdb debugger backend: connected')
    try:
        DebuggerFrontendCli(zmq_socket).run()
    except EOFError:
        pass
    finally:
        zmq_socket.close()

if __name__ == '__main__':
    run_cli_frontend()