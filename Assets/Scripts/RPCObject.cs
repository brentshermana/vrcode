
using System.Collections.Generic;

public class RPCObject
{
    public RPCObject(string m, List<string> a, string i) {
        this.method = m;
        this.args = a;
        this.id = i;
        this.jsonrpc = "1.1";
    }

    public string jsonrpc { get; set; }
    public string method { get; set; }
    public List<string> args { get; set; } //changed from 'params' because that's a reserved word here
    public string id { get; set; }
}