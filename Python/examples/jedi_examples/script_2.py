import jedi

def print_completions(script):
    for completion in script.completions():
        print("name: {}   |  completes with: {}".format(completion.name, completion.complete))

source = '''
import datetime
datetime.datetime
import sy
'''

# jedi.Script(source=None, line=None, column=None, path=None)
#   lines start from 1, columns start from 0
script = jedi.Script(source, 3, len('datetime.da'), 'example.py')

# does changing the position on the fly work?
# in this case it appears to, but in general we're going to be inserting and
# removing text, so it isn't clear that we can rely on _pos being the only thing
# changed.
script._pos = (4, len("import sy"))

print_completions(script)

completions = script.completions()
