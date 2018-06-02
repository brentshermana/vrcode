def makeMessage(Type, SubType, Args):
    d = { "Type":Type, "SubType":SubType, "Args":Args}
    return d

def printMessage(msg):
    print(messageString(msg))

def messageString(msg):
    return "Type: {} SubType: {} Args: {}".format(msg["Type"], msg["SubType"], msg["Args"])