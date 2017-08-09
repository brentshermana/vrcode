using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithC : MonoBehaviour {

	public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 direction = Vector3.zero;
		if (Input.GetKey (KeyCode.C)) {
			if (Input.GetKey (KeyCode.UpArrow)) {
				direction += Vector3.up;
			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				direction -= Vector3.up;
			}
			if (Input.GetKey (KeyCode.LeftArrow)) {
				direction -= Vector3.right;
			}
			if (Input.GetKey (KeyCode.RightArrow)) {
				direction += Vector3.right;
			}
			if (Input.GetKey (KeyCode.F)) { //forward
				direction += Vector3.forward;
			}
			if (Input.GetKey (KeyCode.B)) { //back
				direction += Vector3.back;
			}
		}
		Vector3 change = direction.normalized * speed * Time.deltaTime;
		transform.position = transform.position + change;
	}
}
