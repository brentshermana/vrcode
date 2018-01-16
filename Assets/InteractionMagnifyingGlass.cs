using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

public class InteractionMagnifyingGlass : InteractionBehaviour {

    public GameObject FrontFace; // a quad which will bear the Magnifying glass's texture

    // external methods to be called by InteractionBrowser
    public void SetEnabled(bool status)
    {
        GetComponent<MeshRenderer>().enabled = status;
        GetComponent<Collider>().enabled = status;
        //GetComponent<Rigidbody>(). = status;
    }
    public void Reposition(Vector3 glassPos, Vector3 browserCurveCenter)
    {
        transform.position = glassPos; // go to requested position 
        transform.LookAt(browserCurveCenter); // reset rotation to be aligned with browser screen
    }
    public void SetSize(Vector3 size)
    {
        transform.localScale = size;
    }

	// Use this for initialization
	void Start () {
        // SetEnabled(false);
        Reposition(new Vector3(0f, 1.5f, .6f), new Vector3(0f, 1.5f, 0f));
        SetSize(Vector3.one * .2f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
