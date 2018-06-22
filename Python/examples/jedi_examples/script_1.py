# API DOCS: https://jedi.readthedocs.io/en/latest/docs/api.html

# API Overview: https://jedi.readthedocs.io/en/latest/docs/api.html#jedi.Script

import jedi

source = '''
import datetime
datetime.da'''

# jedi.Script(source=None, line=None, column=None, path=None)
#   lines start from 1, columns start from 0
script = jedi.Script(source, 3, len('datetime.da'), 'example.py')

completions = script.completions()
for completion in completions:
    print("name: {}   |  completes with: {}".format(completion.name, completion.complete))

print()
print("Script __dict__:")
for key in sorted(script.__dict__):
    print("\t{}  |  {}".format(key, type(script.__dict__[key])))
