using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleJsonClass {

    public ExampleJsonClass(int i_, string str_)
    {
        i = i_;
        str = str_;
    }

    public string str;

    public int i;

    public string ToString()
    {
        return str + " " + i;
    }

}
