using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

public class ButtonUtils : MonoBehaviour {

    public GameObject MeshBase; // all physical things to be scaled are childed to this object
    public GameObject ButtonFace; // textures should be applied here, generally
    public InteractionButton button;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetSize(float dim)
    {
        MeshBase.transform.localScale = Vector3.one * dim;
    }

    public void SetTravel(float travel)
    {
        button.setMinHeight(0f);
        button.setMaxHeight(travel);
    }

    public void SetMaterial (Material m)
    {
        ButtonFace.GetComponent<Renderer>().material = m;
    }
    public void SetTexture(Texture t)
    {
        Material m = new Material(ButtonFace.GetComponent<Renderer>().material);
        m.mainTexture = t;
        ButtonFace.GetComponent<Renderer>().material = m;
    }
}
