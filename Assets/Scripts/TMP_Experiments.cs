using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TMP_Experiments : MonoBehaviour {

    [SerializeField]
    TextMeshPro tmp;

	// Use this for initialization
	void Start () {
        tmp.havePropertiesChanged = true;
        tmp.ForceMeshUpdate();

        float maxy = float.NegativeInfinity;
        float miny = float.PositiveInfinity;
    
        Vector3[] verts = tmp.textInfo.meshInfo[0].vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            maxy = Mathf.Max(verts[i].y, maxy);
            miny = Mathf.Min(verts[i].y, miny);
        }

        for (int i = 0; i < verts.Length; i+=2)
        {
            Vector3 newv = new Vector3(verts[i].x, verts[i].y + (maxy-miny)/2, verts[i].z);
            verts[i] = newv;
        }
        tmp.UpdateVertexData();

        Debug.Log(miny + " " + maxy + " " + verts.Length);
        //tmp.ForceMeshUpdate();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
