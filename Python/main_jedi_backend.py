from autocomplete.jedi_backend import JediBackend

def run_backend(host='localhost', port=5999):
    backend = JediBackend(host=host, port=port)
    backend.run_cmd_loop()

if __name__ == '__main__':
    run_backend()