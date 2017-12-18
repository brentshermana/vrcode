using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

// a basic template for encapsulating pretty much any kind of message that could be sent
public class ActionableJsonMessage {
    public string Type;
    public string SubType;
    public string[] Args;

    public ActionableJsonMessage(string t, string st, string[] a)
    {
        Type = t;
        SubType = st;
        Args = a;
    }

    public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        if (Type != null)
        {
            sb.Append(Type);
           
        }
        if (SubType != null)
        {
            sb.Append(" : ");
            sb.Append(SubType);
        }
        if (Args != null)
        {
            foreach (string s in Args)
            {
                sb.Append(" : ");
                sb.Append(s);
            }
        }
        return sb.ToString();
    }
}
