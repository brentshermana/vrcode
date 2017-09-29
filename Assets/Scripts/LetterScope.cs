using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterScope : MonoBehaviour {

    public bool debug;

	//extrema
	private float miny = float.MaxValue;
	private float maxy = float.MinValue;

	private float minx = float.MaxValue;
	private float maxx = float.MinValue;

	private float minz = float.MaxValue;
	private float maxz = float.MinValue;

	public float Width {
		get { return maxx-minx; }
	}
    public float Height
    {
        get { return maxy - miny; }
    }
    public float MinY
    {
        get{ return miny; }
    }
    public float MaxY
    {
        get { return maxy; }
    }
    public float MinX
    {
        get { return minx; }
    }
    public float MaxX
    {
        get { return maxx; }
    }

	// Use this for initialization
	void Awake () {
		//Quaternion rotation = transform.rotation;

		//transform.rotation = Quaternion.identity;
		SetExtents (); //We assume the characters have no rotation\
        //transform.rotation = rotation;

        // Debug.Log ("Letter says its width is " + width);
        if (debug)
        {
            //Debug.Log("I am born with rotation " + transform.rotation);
            //Debug.Log("Width: " + Width + "Height: " + Height);
        }
	}

    private void Start()
    {
        //Debug.Log("I start with rotation " + transform.rotation);
        Debug.Log(Width);
    }

    public void SetMaterial(Material mat)
    {
        //get all meshrenderers in current object or any children, and change all materials
        Queue<Transform> q = new Queue<Transform>();
        q.Enqueue(transform);
        while (q.Count > 0)
        {
            Transform current = q.Dequeue();
            MeshRenderer mr = current.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = mat;
            }
            foreach (Transform child in current)
            {
                q.Enqueue(child);
            }
        }
    }

	// for demonstrative purposes
	private void MakeExtentCube() {
		float xd = Mathf.Abs(minx-maxx);
		float yd = Mathf.Abs(miny-maxy);
		float zd = Mathf.Abs(minz-maxz);
		Vector3 scale = new Vector3 (xd, yd, zd);

		float xc = (maxx + minx) / 2f;
		float yc = (maxy + miny) / 2f;
		float zc = (maxz + minz) / 2f;
		Vector3 center = new Vector3(xc, yc, zc);

		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = center;
		cube.transform.localScale = scale; //local scale is fine - no parent
		cube.transform.rotation = transform.rotation;
	}

	private void SetExtents() {
		List<Vector3> vertices = new List<Vector3> ();
		Queue<Transform> q = new Queue<Transform> ();
		q.Enqueue (transform);
		while (q.Count > 0) {
			Transform current = q.Dequeue ();
			// if the current transform has a mesh, get it
			MeshFilter mf = current.gameObject.GetComponent<MeshFilter> ();
			if (mf != null) {
				Mesh m = mf.mesh;
				foreach (Vector3 vl in m.vertices) {
                    Vector3 v = vl;//current.TransformPoint (vl); //conv to global space

					float x = v.x;
					maxx = Mathf.Max (maxx, x);
					minx = Mathf.Min (minx, x);

					float y = v.y;
					maxy = Mathf.Max (maxy, y);
					miny = Mathf.Min (miny, y);

					float z = v.z;
					maxz = Mathf.Max (maxz, z);
					minz = Mathf.Min (minz, z);
				}
			}
			// add all children to queue
			foreach (Transform child in current) {
				q.Enqueue (child);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("I update with rotation " + transform.rotation);
    }
}
