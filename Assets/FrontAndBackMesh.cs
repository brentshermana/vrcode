using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontAndBackMesh : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int[] prev_triangles = mesh.triangles;
        int[] additional_triangles = mesh.triangles.Clone() as int[];
        for (int i = 0; i < prev_triangles.Length; i+=3)
        {
            additional_triangles[i + 1] = prev_triangles[i + 2];
            additional_triangles[i + 2] = prev_triangles[i + 1];
        }
        int[] final_triangles = new int[prev_triangles.Length * 2];
        prev_triangles.CopyTo(final_triangles, 0);
        additional_triangles.CopyTo(final_triangles, prev_triangles.Length);
        mesh.triangles = final_triangles;

        


        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh; // just in case this doesn't happen automatically
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
