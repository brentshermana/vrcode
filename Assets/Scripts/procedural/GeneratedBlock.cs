using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

public class GeneratedBlock : MonoBehaviour, GeneratableObject {

    private float maxDim = .15f;

    private static float MassOfUnitCube = 20f;

    public Material generatedMaterial;

    //private Quaternion offsetRotation = Quaternion.EulerAngles(new Vector3(45f, 45f));

    public void AlignAxis(Vector3 axis)
    {
        transform.LookAt(transform.position + axis);
        //transform.rotation *= offsetRotation;
    }
    public void Resize(float dim)
    {
        float adj_dim = dim / 1.414f;
        adj_dim = Mathf.Min(adj_dim, maxDim);
        transform.localScale = Vector3.one * adj_dim; // divide by square root of two because we want a corner of the cube at each fingertip
    }
    public void Generate()
    {
        // change appearance
        GetComponent<Renderer>().material = generatedMaterial;
        // allow object to interact with others
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = MassOfUnitCube * Mathf.Pow(transform.localScale.x, 3f); // ensure proportional mass
        gameObject.AddComponent<InteractionBehaviour>();
        gameObject.AddComponent<BoxCollider>();
    }
    public void Reposition(Vector3 center)
    {
        transform.position = center;
    }
}
