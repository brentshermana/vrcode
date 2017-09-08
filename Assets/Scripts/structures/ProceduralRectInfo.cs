using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRectInfo {

	public Material material;

	private static Vector3[] BASE_VERTICES = new Vector3[]{
		// face 1 (xy plane, z=0)
		new Vector3(0,0,0), 
		new Vector3(1,0,0), 
		new Vector3(1,-1,0), 
		new Vector3(0,-1,0), 
		// face 2 (zy plane, x=1)
		new Vector3(1,0,0), 
		new Vector3(1,0,1), 
		new Vector3(1,-1,1), 
		new Vector3(1,-1,0), 
		// face 3 (xy plane, z=1)
		new Vector3(1,0,1), 
		new Vector3(0,0,1), 
		new Vector3(0,-1,1), 
		new Vector3(1,-1,1), 
		// face 4 (zy plane, x=0)
		new Vector3(0,0,1), 
		new Vector3(0,0,0), 
		new Vector3(0,-1,0), 
		new Vector3(0,-1,1), 
		// face 5  (zx plane, y=1)
		new Vector3(0,-1,0), 
		new Vector3(1,-1,0), 
		new Vector3(1,-1,1), 
		new Vector3(0,-1,1), 
		// face 6 (zx plane, y=0)
		new Vector3(0,0,0), 
		new Vector3(0,0,1), 
		new Vector3(1,0,1), 
		new Vector3(1,0,0),
	};

	private Mesh mesh;

	private Vector3[] vertices;
	private int[] triangles;
	private Vector2[] uvs;

	private Vector3 origin;
	public Vector3 Origin {
		set {
			if (origin == value)
				return;
			else {
				origin = value;
				staleMesh = true;
			}
		}
	}

	private bool staleMesh = true;
	private Vector3 dimensions;
	public Vector3 Dimensions {
		set {
			if (dimensions == value)
				return;
			else {
				dimensions = value;
				staleMesh = true;
			}
		}

		get { return dimensions; }
	}

	private List<int> vertsWithX = new List<int>();

	public ProceduralRectInfo() {
		origin = Vector3.zero; //default

		if (!material) {
			material = new Material (Shader.Find ("Diffuse"));
		}

		this.mesh = new Mesh ();

		//copy base vertices over
		vertices = new Vector3[BASE_VERTICES.Length];
		for (int i = 0; i < vertices.Length; i++)
			vertices [i] = BASE_VERTICES [i];

		Mesh mesh = new Mesh ();

		dimensions = new Vector3(1f,1f,1f);
		vertices = BASE_VERTICES;
		mesh.vertices = vertices;

		for (int i = 0; i < BASE_VERTICES.Length; i++) {
			if (BASE_VERTICES [i].x != 0f)
				vertsWithX.Add (i);
		}

		int faces = 6; // here a face = 2 triangles

		List<int> _triangles = new List<int>();
		List<Vector2> _uvs = new List<Vector2>();

		for (int i = 0; i < faces; i++) {
			int triangleOffset = i*4;
			_triangles.Add(1+triangleOffset);
			_triangles.Add(2+triangleOffset);
			_triangles.Add(0+triangleOffset);

			_triangles.Add(2+triangleOffset);
			_triangles.Add(3+triangleOffset);
			_triangles.Add(0+triangleOffset);

			// same uvs for all faces
			_uvs.Add(new Vector2(0,0));
			_uvs.Add(new Vector2(1,0));
			_uvs.Add(new Vector2(1,1));
			_uvs.Add(new Vector2(0,1));
		}

		triangles = _triangles.ToArray ();

		uvs = _uvs.ToArray();
		mesh.uv = _uvs.ToArray();

		mesh.RecalculateNormals(); 
		mesh.RecalculateBounds (); 
	}

	public void Enforce(GameObject go) {
		if (staleMesh) {
			MeshFilter meshFilter = go.GetComponent<MeshFilter> ();
            if (meshFilter == null)
            {
                meshFilter = go.AddComponent<MeshFilter>();
            }

			MeshRenderer meshrenderer = go.GetComponent<MeshRenderer> ();
            if (meshrenderer == null)
            {
                meshrenderer = go.AddComponent<MeshRenderer>();
            }
            meshrenderer.material = material;

			vertices = calculateVertices ();

			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uvs;

			meshFilter.mesh = mesh;
			mesh.RecalculateNormals (); 
			mesh.RecalculateBounds ();
		}
	}

	private Vector3[] calculateVertices() {
		Vector3[] newV = new Vector3[BASE_VERTICES.Length];
		for (int i = 0; i < newV.Length; i++) {
			newV [i] = BASE_VERTICES [i];
			newV [i].Scale (dimensions);
			newV [i] += origin;
		}
		return newV;
	}

}
