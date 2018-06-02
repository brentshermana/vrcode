import sys

import os
import traceback

import zmq

import json_utils
from debugger_backend import Qdb


def init(host='localhost', port=6000, redirect=True):
    "Simplified interface to debug running programs"

    # TODO: pretty sure we don't need this
    # global qdb, listener, zmq_socket

    # # destroy the debugger if the previous connection is lost (i.e. broken pipe)
    # if qdb and not qdb.ping():
    #     qdb.close()
    #     qdb = None

    address = (host, port)  # family is deduced to be 'AF_INET'
    print("qdb debugger backend: waiting for connection to {}".format(address))
    # conn = Client(address, authkey=authkey)
    zmq_context = zmq.Context()
    zmq_socket = zmq_context.socket(zmq.PAIR)
    if host == 'localhost':
        host = '127.0.0.1'
    zmq_socket.connect("tcp://{}:{}".format(host, port))
    zmq_socket.send(b'handshake')
    zmq_socket.recv()
    print('qdb debugger backend: connected to'.format(address))
    # create the backend
    qdb = Qdb(zmq_socket, redirect_stdio=redirect, allow_interruptions=True)
    # initial handshake
    qdb.startup()
    return qdb

def run_backend(host='localhost', port=6000):
    "Debug a script (running under the backend) and connect to remote frontend"

    import sys

    if not sys.argv[1:] or sys.argv[1] in ("--help", "-h"):
        print("usage: qdb.py scriptfile [arg] ...")
        sys.exit(2)

    mainpyfile = sys.argv[1]  # Get script filename
    if not os.path.exists(mainpyfile):
        print('Error:', mainpyfile, 'does not exist')
        sys.exit(1)

    del sys.argv[0]  # Hide "pdb.py" from argument list

    # Replace pdb's dir with script's dir in front of module search path.
    sys.path[0] = os.path.dirname(mainpyfile)

    db = init(host, port)
    try:
        print("running {}".format(mainpyfile))
        # create the backend
        db._runscript(mainpyfile)
        print("The program finished")
    except SystemExit:
        # In most cases SystemExit does not warrant a post-mortem session.
        print("The program exited via sys.exit(). Exit status: {}".format(sys.exc_info()[1]))
        raise
    except Exception:
        traceback.print_exc()
        print("Uncaught exception. Entering post mortem debugging")
        import sys
        info = sys.exc_info()
        db.post_mortem(info)
        print("Program terminated!")
    finally:
        zmq_socket = db.zmq_socket
        import json_utils, sys
        # closing handshake:
        zmq_socket.send(json_utils.encode_json({'method': 'dbquit'}).encode())
        zmq_socket.recv()  # just gibberish
        zmq_socket.close()
        # reset stdio values for final print
        sys.stdin, sys.stdout, sys.stderr = sys.__stdin__, sys.__stdout__, sys.__stderr__
        print("qdb debbuger backend: connection closed")

if __name__ == '__main__':
    run_backend()
