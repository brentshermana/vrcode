using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;


[RequireComponent (typeof (MeshFilter))] 
[RequireComponent (typeof (MeshRenderer))]
public class ProceduralRect2 : MonoBehaviour {

	private ProceduralRectInfo rectInfo;
	private TextMeshPro textPro;

	// Use this for initialization
	void Start () {
		rectInfo = new ProceduralRectInfo ();
		rectInfo.Enforce (gameObject);
	}


	private Vector3 dimensions;
	public void SetDimensions(Vector3 dim) {
		dimensions = dim;
	}
	void LateUpdate() {
		if (dimensions.x > 0f && dimensions.y > 0f) {
			rectInfo.Dimensions = dimensions;
			rectInfo.Enforce (gameObject);
		}
	}


}
