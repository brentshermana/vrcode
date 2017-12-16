using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCursor : MonoBehaviour {

    /*
     * 1) keeps the object to which this is attached at a relative offset 
     *      and rotation from the controller given by the controller index
     * 2) maintains a 'CollidingObject' variable, which is set to the value of
     *      any object in 'CursorTarget' layer which collides with the pointer
     */


    //public SteamVR_ControllerManager manager;
    public Vector3 offset_position;
    public Vector3 offset_rotation;
    //public Transform controllerTransform;

    //public SteamVR_Controller.Device device;

    //public GameObject CollidingObject;

    public string controllerObjectName;
    public int index;
    public int controller_manager_index;

    private SteamVR_ControllerManager manager;
    private SteamVR_TrackedObject to;
    private SteamVR_Controller.Device controller;

	// Use this for initialization
	void Start () {
        //to = GameObject.Find(controllerObjectName).GetComponent<SteamVR_TrackedObject>();
        //index = (int)to.index;
        //controller = SteamVR_Controller.Input(index);

        Init();
    }
    
	
	// Update is called once per frame
	void LateUpdate () {


        //bool problem = false;
        //if (ensureManager())
        //{
        //    if (!ensureDevice())
        //    {
        //        Debug.LogError("Can't Find Device!");
        //        problem = true;
        //    }
        //    if (!ensureControllerTransform())
        //    {
        //        Debug.Log("Can't Find Transform");
        //        problem = true;
        //    }
        //}
        //else
        //{
        //    problem = true;
        //}

        //if (!problem)
        //{
        //    transform.position = controllerTransform.position + controllerTransform.rotation * offset_position;
        //    transform.rotation = controllerTransform.rotation * Quaternion.Euler(offset_rotation.x, offset_rotation.y, offset_rotation.z);
        //}

        Init();

        //var ct = controller.transform;
        //transform.position = ct.pos + ct.rot * offset_position;
        //transform.rotation = ct.rot * Quaternion.Euler(offset_rotation.x, offset_rotation.y, offset_rotation.z);
	}

    private void Init()
    {
        if (manager == null)
        {
            manager = GameObject.Find("[CameraRig]").GetComponent<SteamVR_ControllerManager>();
        }
        if (to == null)
        {
            to = manager.objects[controller_manager_index].GetComponent<SteamVR_TrackedObject>();// controller_go.GetComponent<SteamVR_TrackedObject>();
        }
        index = (int)to.index;
        controller = SteamVR_Controller.Input(index);
        transform.parent = to.gameObject.transform;
        transform.localPosition = offset_position;
        transform.localRotation = Quaternion.Euler(offset_rotation.x, offset_rotation.y, offset_rotation.z);
    }

    //TODO: I'm currently assuming the cursor will never intersect two cursor targets at the same time, which I know isn't true

    // handle collisions with objects in layer 'CursorTarget'
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer == LayerMask.NameToLayer("CursorTarget") && CollidingObject == null)
    //    {
    //        CollidingObject = other.gameObject;
    //    }
    //    Debug.Log("Collision Enter");
    //}
    //void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.GetInstanceID() == CollidingObject.GetInstanceID())
    //    {
    //        CollidingObject = null;
    //    }
    //    Debug.Log("Collision Exit");
    //}
}
