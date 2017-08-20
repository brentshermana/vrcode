using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour {

	public Vector3 bracketDim;
	public Vector3 bridgeDim;

	ProceduralRectInfo lbi; // left bracket info
	ProceduralRectInfo rbi; // right bracket info
	ProceduralRectInfo bi; // bridge info
	GameObject lb;
	GameObject rb;
	GameObject b;



	// Use this for initialization
	void Start () {
		// children to add individual gate components to
		lb = makeRectChild();
		rb = makeRectChild ();
		b = makeRectChild ();

		//create rectangles
		lbi = initRect (bracketDim, lb);
		rbi = initRect (bracketDim, rb);
		bi = initRect (bridgeDim, b);

		//set local positions
		lb.transform.localPosition = Vector3.zero;
		b.transform.localPosition = Vector3.right*lbi.Dimensions.x;
		rb.transform.localPosition = b.transform.localPosition + Vector3.right*bi.Dimensions.x;
	}

	private GameObject makeRectChild() {
		GameObject go = new GameObject ();
		go.transform.position = transform.position;
		go.transform.parent = transform;
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		return go;
	}

	private ProceduralRectInfo initRect(Vector3 dim, GameObject go) {
		ProceduralRectInfo ri = new ProceduralRectInfo ();
		ri.Dimensions = dim;
		ri.Enforce (go);
		return ri;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
