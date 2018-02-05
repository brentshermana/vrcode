using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class InertialScrollInstance
{
    protected Vector2 ScrollV;
    private float Deceleration;

    public InertialScrollInstance(Vector2 scrollV, float deceleration)
    {
        Deceleration = Mathf.Abs(deceleration);
        ScrollV = scrollV;
    }
    public Vector2 GetScroll(float TimeInterval)
    {
        Vector2 ret = ScrollV * TimeInterval;
        ScrollV = Vector2.MoveTowards(ScrollV, Vector2.zero, Deceleration*TimeInterval);
        return ret;
    }
}
