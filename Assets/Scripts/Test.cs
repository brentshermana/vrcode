using System.Collections;
using System.Collections.Generic;
using Leap;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class Test : MonoBehaviour {

    // Use this for initialization
    void Start ()
    {
	    var it = new ArcIterator(Vector3.up, Vector3.up, 1f)
		    .Iterator(0f, Mathf.PI/100f)
		    .GetEnumerator();
	    for (int i = 0; i < 100; i++)
	    {
		    Vector3 point = it.Current;
		    it.MoveNext();

		    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		    go.transform.position = point;
		    go.transform.localScale = Vector3.one * .01f;
	    }
	    
    }
	
	// Update is called once per frame
	void Update () {
    }

}
