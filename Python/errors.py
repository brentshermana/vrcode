class RPCError(RuntimeError):
    "Remote Error (not user exception)"
    pass

class DBQuitError(RuntimeError):
    pass