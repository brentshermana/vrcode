using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackboardBrowserInput : MonoBehaviour {

    /*
     * Just adds ViveBrowserUI component to the backboard
     */

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		foreach (Transform child in transform)
        {
            if (child.name == "Backboard")
            {
                child.gameObject.AddComponent<ViveBrowserUI>();
                Destroy(this); // the script has done its job, time to go away
            }
        }
	}
}
