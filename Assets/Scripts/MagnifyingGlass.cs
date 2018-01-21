using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;
using Leap;

public class MagnifyingGlass : MonoBehaviour {

    public GameObject Face; // face is a child of button
    public GameObject Button;
    public float PositionLerp;
    public float RotationLerp;

    public float maxV;
    public float maxA;

    public Vector3 PositionGoal;
    public Quaternion RotationGoal;

    private bool Frozen { get; set; }

    public void SetFrozen(bool status)
    {
        Frozen = status;
    }

    private List<Transform> children = new List<Transform>();
    
    // external methods to be called by InteractionBrowser
    public void SetEnabled(bool status)
    {
        //Debug.Log("Enabled " + status);
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = status;
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            collider.enabled = status;
        GetComponentInChildren<InteractiveMagnifyingGlassButton>().controlEnabled = status;
        //TODO: ENABLE, DISABLE OTHER THINGS?
        //GetComponent<Rigidbody>(). = status;
    }
    public void Reposition(Vector3 position, Quaternion rotation)
    {
        Debug.Log("Reposition");
        //InteractionButton b = GetComponentInChildren<InteractionButton>();
        // interactionbehaviours have onenable, ondisable callbacks which hopefully handle teleportation
        //b.enabled = false;
        //transform.position = position; // go to requested position 
        //transform.rotation = rotation; // reset rotation to be aligned with browser screen
        //b.enabled = true;
        PositionGoal = position;
        RotationGoal = rotation;
    }
    public void SetSize(Vector3 size)
    {
        Button.transform.localScale = size;
    }

    void FixedUpdate()
    {
        if (!Frozen)
        {
            transform.position = Vector3.Lerp(transform.position, PositionGoal, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, RotationGoal, RotationLerp);
        }
        // Physics BS isn't working!
       
        //Rigidbody body = GetComponent<Rigidbody>();
        //Debug.Log("FixedUpdate with V " + body.velocity);
        //InteractionButton button = GetComponentInChildren<InteractionButton>();
        //Vector3 position = body.transform.position;
        //Vector3 desiredPosition = PositionGoal; //Vector3.Lerp(position, PositionGoal, PositionLerp);
        //Vector3 v = body.velocity;
        //Vector3 desiredV = desiredPosition - position; // currently, the velocity which would be needed for a second of movement
        //if (desiredV.magnitude > maxV)
        //{
        //    desiredV = desiredV.normalized * maxV;
        //}
        //if (position + desiredV.magnitude*Time. )
        //Vector3 linearAcceleration = (desiredV - v);
        //if (linearAcceleration.magnitude > maxA)
        //{
        //    linearAcceleration = linearAcceleration.normalized * maxA;
        //}
        //linearAcceleration *= Time.fixedDeltaTime; // scale with respect to update length

        //Debug.Log("Add Acceleration " + linearAcceleration);
        //Debug.Log("Desired V " + desiredV);
        //body.velocity += linearAcceleration;
        //button.AddLinearAcceleration(linearAcceleration);


        //TODO: rotation
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
