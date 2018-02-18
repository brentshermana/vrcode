using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VRUI_Panel : MonoBehaviour
{

	public VRUI_PanelGenerator generator;

	public void SetShapeParams(float w, float h, float r)
	{
		generator = new VRUI_PanelGenerator(w,h,r);

		Mesh m = generator.GeneratePanel();

		GetComponent<MeshFilter>().sharedMesh = m;
		GetComponent<MeshCollider>().sharedMesh = m;
	}

	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
