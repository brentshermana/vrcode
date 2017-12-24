using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class MyConvert {
    // Deserialization example:
    // https://www.newtonsoft.com/json/help/html/DeserializeObject.htm

    public static RPCObject rpcobj(byte[] b) {
        string jsonStr = buf2str(b);
        return JsonConvert.DeserializeObject<RPCObject>(jsonStr);
    }

    public static ActionableJsonMessage[] actionableMessageFromBytes(byte[] b)
    {
        string jsonStr = buf2str(b);
        return JsonConvert.DeserializeObject<ActionableJsonMessage[]>(jsonStr);
    }

    public static byte[] actionableMessageToBytes(ActionableJsonMessage[] messages)
    {
        string jsonStr = JsonConvert.SerializeObject(messages);
        return str2buf(jsonStr);
    }

    public static string stringFromBytes(byte[] b) {
        string str = buf2str(b);
        return buf2str(b);
    }

    public static byte[] stringToBytes(string s)
    {
        return str2buf(s);
    }

    #region encoding
    private static string buf2str(byte[] buf)
    {
        return System.Text.Encoding.UTF8.GetString(buf);
    }
    private static byte[] str2buf(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
    #endregion
}
