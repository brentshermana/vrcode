from zmq_wrapper import ZmqFrontendCmdLoop, init_frontend_socket

def run_frontend(host='localhost', port=5999):
    zmq_socket = init_frontend_socket(host=host, port=port)
    frontend = ZmqFrontendCmdLoop(zmq_socket)
    frontend.run_cmd_loop()


if __name__ == '__main__':
    run_frontend()