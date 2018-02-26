using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using vrcode.networking.message;

namespace vrcode.networking
{
    public class MyConvert {
        // Deserialization example:
        // https://www.newtonsoft.com/json/help/html/DeserializeObject.htm

        public int str2int(string s) {
            return int.Parse(s.Trim());
        }
        public string int2str(int i) {
            return i.ToString();
        }

        public static string tojson(Object o) {
            return JsonConvert.SerializeObject(o);
        }
        public static string tojson(string s) {
            // replace existing double quotations with escaped quotations
            s.Replace("\"", "\\\"");
            // strings need to be wrapped in double quotations
            return "\"" + s + "\"";
        }

        public static T fromjson<T>(byte[] b) {
            return fromjson<T>(buf2str(b));
        }
        public static T fromjson<T>(string s) {
            try
            {
                return JsonConvert.DeserializeObject<T>(s);
            }
            catch (JsonReaderException e)
            {
                UnityEngine.Debug.LogError(string.Format("failed to parse:\n{0}", s));
                throw e;
            }
        }

        public static RPCMessage rpcobj(byte[] b) {
            string jsonStr = buf2str(b);
            try
            {
                return JsonConvert.DeserializeObject<RPCMessage>(jsonStr);
            }
            catch (JsonReaderException e) {
                UnityEngine.Debug.LogError("RPCObject failed to parse:\n" + jsonStr);
                throw e;
            }
        }
        public static byte[] rpcobj(RPCMessage rpc) {
            string jsonStr = JsonConvert.SerializeObject(rpc);
            return str2buf(jsonStr);
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
}