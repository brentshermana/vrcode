using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasRendererTest : MonoBehaviour
{

	public float uvfactor;

	private Mesh curvedScreen;
	public CanvasRenderer renderer;

	// Use this for initialization
	void Start ()
	{
		Mesh original = GetComponent<MeshFilter>().sharedMesh;
		
		curvedScreen = new Mesh();
		curvedScreen.vertices = original.vertices;
		curvedScreen.uv = original.uv;
		for (int i = 0; i < curvedScreen.uv.Length; i++)
		{
			curvedScreen.uv[i] = curvedScreen.uv[i] * uvfactor;
		}
		curvedScreen.triangles = original.triangles;
		curvedScreen.RecalculateBounds();
		curvedScreen.RecalculateNormals();
		curvedScreen.RecalculateTangents();
	}
	
	// Update is called once per frame
	void Update () {
		renderer.SetMesh(curvedScreen);
	}
}
