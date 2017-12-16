using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour {

    public float radius;
    public float width;

    public Material normal, selected, pressed;

	// Use this for initialization
	void Start () {
        GetComponent<KeyboardCreator>().Init(radius,width, normal, selected, pressed);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
