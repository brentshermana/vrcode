using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPUIVertexTest : MonoBehaviour
{
	public GameObject tmpObj;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		TMP_SubMeshUI ui = tmpObj.GetComponent<TMP_SubMeshUI>();
		if (ui != null)
		{
			Debug.Log("Got It!!!");
		}
	}
}
