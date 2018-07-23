from syntax_highlighting.highlighter_backend import Highlighter

def main():
    highlighter = Highlighter()
    highlighter.run_cmd_loop()

if __name__ == '__main__':
    main()