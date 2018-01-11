
using System.Collections.Generic;

using DBNotif;


// This object is a combination of an RPC Request and Response object... fields which
// aren't initialized should just be ignored
public class RPCMessage
{

    // common to request and response
    public int id { get; set; }
    public string jsonrpc { get; set; }

    // request
    public string method { get; set; }
    public List<string> args { get; set; } //changed from 'params' because that's a reserved word here

    // response
    public string result { get; set; }
    public RPCError error { get; set; }

    public RPCMessage() {
        this.jsonrpc = "1.1";
        this.id = -1; // default value
    }

    public override string ToString()
    {
        return string.Format("RPCObject:\n\nid={0}\njsonrpc={1}\nmethod={2}\nargs={3}\nresult={4}\nerror={5}", id, jsonrpc, method, args, result, error);
    }

    // helper constructors
    public static RPCMessage Request(string m, List<string> a, int i) {
        RPCMessage rpc = new RPCMessage();
        rpc.method = m;
        rpc.args = a;
        rpc.id = i;
        return rpc;
    }
    public static RPCMessage Response(string r, RPCError e, int i)
    {
        RPCMessage rpc = new RPCMessage();
        rpc.result = r;
        rpc.error = e;
        rpc.id = i;
        return rpc;
    }
}

public class RPCError {
    public RPCError(int code_, string message_) {
        code = code_;
        message = message_;
    }

    public override string ToString()
    {
        return string.Format("[RPCError: code={0}, message={1}]", code, message);
    }

    public int code { get; set; }
    public string message { get; set; }
}