using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveHandCollisions : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private bool click = false;
    public float maxraycast = 1f;
    private Vector2 clickCoord = Vector2.zero;
    public int intersectingLeapBones = 0;
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("LeapHands"))
        {
            Debug.Log("Hooray!");
            if (intersectingLeapBones == 0)
            {
                RaycastHit hit = new RaycastHit();
                int mask = 1 << gameObject.layer; // the goal is to fire a raycast at this object
                Transform otherObject = col.gameObject.transform;

                // get the point of impact by averaging all contact points
                Vector3 impactPoint = Vector3.zero;
                foreach (ContactPoint cpt in col.contacts)
                {
                    Debug.Log("Impact Point " + cpt.point);
                    impactPoint += cpt.point;
                }
                impactPoint /= col.contacts.Length;

                Vector3 rayDirection = impactPoint - otherObject.position;
                Ray ray = new Ray(otherObject.position, rayDirection);
                if (Physics.Raycast(ray, out hit, maxraycast, mask)) // && hit.collider.gameObject.Equals(gameObject))
                {
                    clickCoord = hit.textureCoord;
                    Debug.Log("Texture Coord: " + hit.textureCoord);
                    click = true;
                }
                else
                {
                    Debug.Log("Failed Raycast from " + otherObject.position + " To " + impactPoint);
                    GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    a.transform.localScale = new Vector3(.01f, .01f, .01f);
                    b.transform.localScale = new Vector3(.01f, .01f, .01f);
                    a.transform.position = impactPoint;
                    b.transform.position = otherObject.position;
                }
            }

            intersectingLeapBones += 1;
        }
    }
    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("LeapHands"))
        {
            intersectingLeapBones -= 1;
        }
    }
}
