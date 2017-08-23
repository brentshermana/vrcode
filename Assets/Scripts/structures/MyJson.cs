using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class MyJson {
    public static ActionableJsonMessage[] fromBytes(byte[] b)
    {
        string jsonStr = buf2str(b);
        return JsonConvert.DeserializeObject<ActionableJsonMessage[]>(jsonStr);
    }

    public static byte[] toBytes(ActionableJsonMessage[] messages)
    {
        string jsonStr = JsonConvert.SerializeObject(messages);
        return str2buf(jsonStr);
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
