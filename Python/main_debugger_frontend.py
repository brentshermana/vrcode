import zmq
from debugger.debugger_frontend_cli import DebuggerFrontendCli
import zmq_wrapper

def run_cli_frontend(port=6000, authkey=b'secret password'):
    "Start the CLI server and wait connection from a running debugger backend"
    zmq_socket = zmq_wrapper.init_frontend_socket(port=port, service_name="Debugger")
    try:
        DebuggerFrontendCli(zmq_socket).run()
    except EOFError:
        pass
    finally:
        zmq_socket.close()

if __name__ == '__main__':
    run_cli_frontend()