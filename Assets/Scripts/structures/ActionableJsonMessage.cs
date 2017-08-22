using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

// a basic template for encapsulating pretty much any kind of message that could be sent
public class ActionableJsonMessage {
    public string Type;
    public string SubType;
    public string[] Args;

    public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(Type);
        sb.Append(" : ");
        sb.Append(SubType);
        foreach (string s in Args)
        {
            sb.Append(" : ");
            sb.Append(s);
        }
        return sb.ToString();
    }
}
