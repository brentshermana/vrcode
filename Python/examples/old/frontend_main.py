import main_debugger_frontend.Cli as Cli

def start(host="localhost", port=6000, authkey=b'hello'):
    "Start the CLI server and wait connection from a running debugger backend"

    address = (host, port)
    from multiprocessing.connection import Listener
    address = (host, port)     # family is deduced to be 'AF_INET'
    #key_bytes = bytes(authkey)
    #auth_encoding = cchardet.detect(key_bytes)['encoding']
    listener = Listener(address, authkey=authkey)
    print("qdb debugger backend: waiting for connection at", address)
    conn = listener.accept()
    print('qdb debugger backend: connected to', listener.last_accepted)
    try:
        Cli(conn).run()
    except EOFError:
        pass
    finally:
        conn.close()