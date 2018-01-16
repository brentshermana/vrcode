using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity.Interaction;
using Leap.Unity;

public class InteractionBrowser : InteractionBehaviour
{
    public float maxraycast = 1f;
    public InteractionMagnifyingGlass magnifyingGlass;

	// Use this for initialization
	void Start () {
        OnPrimaryHoverBegin = onHoverBegin;
        OnPrimaryHoverEnd = onHoverEnd;
        //OnHoverBegin = onHoverBegin;
        //OnHoverEnd = onHoverEnd;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void onHoverBegin()
    {
        Vector3 rayDirection = UnityVectorExtension.ToVector3(primaryHoveringFinger.Direction);
        Ray ray = new Ray(UnityVectorExtension.ToVector3(primaryHoveringFinger.StabilizedTipPosition), rayDirection);
        RaycastHit hit;
        int mask = 1 << gameObject.layer; // the goal is to fire a raycast at this object
        if (Physics.Raycast(ray, out hit, maxraycast, mask)) // && hit.collider.gameObject.Equals(gameObject))
        {
            float r = GetComponent<ProceduralCurvedScreen>().CurveRadius;
            Vector3 pointToFace = transform.position - transform.forward * r;
            // the following line prevents the glass from rotating along this transform's
            // x axis ( try commenting out the following line and see what happens!
            pointToFace += transform.up * (hit.point.y - transform.position.y);
            
            magnifyingGlass.SetEnabled(true);
            magnifyingGlass.Reposition(hit.point, pointToFace);
            //clickCoord = hit.textureCoord;
            Debug.Log("Texture Coord: " + hit.textureCoord);
            //click = true;
        }
        else
        {
            //Debug.Log("Failed Raycast from " + otherObject.position + " To " + impactPoint);
            //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //a.transform.localScale = new Vector3(.01f, .01f, .01f);
            //b.transform.localScale = new Vector3(.01f, .01f, .01f);
            //a.transform.position = impactPoint;
            //b.transform.position = otherObject.position;
        }

        Debug.Log("Hover Begin");
        //GetComponent<MeshRenderer>().enabled = false;
    }
    void onHoverEnd()
    {
        //GetComponent<MeshRenderer>().enabled = true;
    }
}
