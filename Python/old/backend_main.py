import os
import sys
import qdb.Qdb as Qdb
import traceback

def init(host='localhost', port=6000, authkey=b'hello', redirect=True):
    "Simplified interface to debug running programs"
    global qdb, listener, conn

    # destroy the debugger if the previous connection is lost (i.e. broken pipe)
    if qdb and not qdb.ping():
        qdb.close()
        qdb = None

    from multiprocessing.connection import Client
    # only create it if not currently instantiated
    if not qdb:
        address = (host, port)     # family is deduced to be 'AF_INET'
        print("qdb debugger backend: waiting for connection to", address)
        conn = Client(address, authkey=authkey)
        print('qdb debugger backend: connected to', address)
        # create the backend
        qdb = Qdb(conn, redirect_stdio=redirect, allow_interruptions=True)
        # initial hanshake
        qdb.startup()

if not sys.argv[1:] or sys.argv[1] in ("--help", "-h"):
    print("usage: pdb.py scriptfile [arg] ...")
    sys.exit(2)

mainpyfile = sys.argv[1]  # Get script filename
if not os.path.exists(mainpyfile):
    print('Error:', mainpyfile, 'does not exist')
    sys.exit(1)

del sys.argv[0]  # Hide "pdb.py" from argument list

# Replace pdb's dir with script's dir in front of module search path.
sys.path[0] = os.path.dirname(mainpyfile)

# create the backend
init()
try:
    print("running", mainpyfile)
    qdb._runscript(mainpyfile)
    print("The program finished")
except SystemExit:
    # In most cases SystemExit does not warrant a post-mortem session.
    print("The program exited via sys.exit(). Exit status: ", end=' ')
    print(sys.exc_info()[1])
    raise
except Exception:
    traceback.print_exc()
    print("Uncaught exception. Entering post mortem debugging")
    info = sys.exc_info()
    qdb.post_mortem(info)
    print("Program terminated!")
finally:
    conn.close()
    print("qdb debbuger backend: connection closed")