using System.Xml.XPath;
using Leap.Unity.Interaction;
using UnityEngine;

namespace vrcode.input.keyboard
{
    public class DraggableHandle : MonoBehaviour
    {
        private InteractionBehaviour ib;
        private Rigidbody rb;
        
        void Start()
        {
            ib = GetComponent<InteractionBehaviour>();
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (ib == null)
            {
                ib = GetComponent<InteractionBehaviour>();
                Debug.Log("NULL");
            }
            else if (!ib.isGrasped)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}