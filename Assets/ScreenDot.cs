using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ScreenDot : MonoBehaviour {

    /*
     * Attached to whatever gameobject serves as the browser cursor
     */

    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Awake () {
        meshRenderer = GetComponent<MeshRenderer>();
        SetActive(false);
	}

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetActive(bool active)
    {
        meshRenderer.enabled = active;
    }
}
