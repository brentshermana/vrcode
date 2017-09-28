using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixellatedLetterAdjust : MonoBehaviour {

	// this is just an attribute of the models
	private static float pixel_dimension = .02f;

	public float y_offset;

	void Awake() {
		Debug.Log("Before " + transform.position.y);
		transform.position = transform.position + transform.up*y_offset*pixel_dimension;
		Debug.Log("After " + transform.position.y);
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}
