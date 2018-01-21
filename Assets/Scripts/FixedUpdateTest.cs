using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedUpdateTest : MonoBehaviour {
    public int x;

	// Use this for initialization
	void Start () {
        Debug.Log("hi");
        x = 4;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        Debug.Log("hello there");
        x = 5;
    }
}
