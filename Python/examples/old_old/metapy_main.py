from subprocess import Popen, PIPE, TimeoutExpired

popen = Popen(args=["python3"], stdin=PIPE, stdout=PIPE, stderr=PIPE)
(out, err) = popen.communicate(bytes("print()", "latin1"))
print(out)
print(err)
