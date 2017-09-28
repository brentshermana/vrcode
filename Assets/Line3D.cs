using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line3D : MonoBehaviour {

    public List<GameObject> words;

    public bool debug;

    private float x_offset = 0f;


    public float spacing;
    public Transform wordmodel;
    

	// Use this for initialization
	void Start () {
        words = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddWord(string word)
    {
        if (debug) Debug.Log("word " + word);
        Vector3 position;
        if (words.Count == 0)
        {
            position = transform.position;
        }
        else {
            position = words[words.Count-1].transform.position;
            position += transform.right * words[words.Count - 1].GetComponent<String3DPix>().Width;
            if (debug) Debug.Log("width " + words[words.Count - 1].GetComponent<String3DPix>().Width);
            //position += transform.right * spacing;
        }
        
        
        GameObject go = Instantiate(wordmodel, position, transform.rotation).gameObject;
        go.GetComponent<String3DPix>().SetContent(word);
        words.Add(go);
    }
}
