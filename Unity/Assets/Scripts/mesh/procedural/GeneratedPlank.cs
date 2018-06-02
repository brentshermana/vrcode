using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

namespace vrcode.mesh.procedural
{
    public class GeneratedPlank : MonoBehaviour, GeneratableObject
    {
        private static float maxDim = .4f;

        private static float constX = .13f;
        private static float constY = .03f;
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
            transform.localScale = new Vector3(constX, constY, adj_dim);
        }
        public void Generate()
        {
            // change appearance
            GetComponent<Renderer>().material = generatedMaterial;
            // allow object to interact with others
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            Vector3 scale = transform.localScale;
            rb.mass = MassOfUnitCube * scale.x * scale.y * scale.z;
            gameObject.AddComponent<InteractionBehaviour>();
            gameObject.AddComponent<BoxCollider>();
        }
        public void Reposition(Vector3 center)
        {
            transform.position = center;
        }
    }
}