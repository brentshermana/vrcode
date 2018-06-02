using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using vrcode.browser;

/*
 * Cursor Input Recipients ask the inputmanager what's going on
 * 
 * this object also serves as a convenient access point for querying things like
 * controller locations, transforms, etc.
 */
public class ControllerInputManager : MonoBehaviour {
    //public ScreenDot CURSOR;
    //public static ScreenDot screenCursor; //confusingly, the actual controller on the screen

    public static int[] controller_indices;
    public static Transform[] controllers; //an array is convenient because the index is the way
                                    // to access the appropriate controller through SteamVR's API

    public static bool[] triggerPress;
    private static float maxRaycast = 10f;

    public static bool[] initialized;


    private static SteamVR_ControllerManager manager;
    private static SteamVR_TrackedObject[] tracked_objects;
    private static void Init()
    {
        
        try
        {
            manager = GameObject.Find("[CameraRig]").GetComponent<SteamVR_ControllerManager>();
            tracked_objects = new SteamVR_TrackedObject[manager.objects.Length];
            controller_indices = new int[manager.objects.Length];
            controllers = new Transform[manager.objects.Length];
            initialized = new bool[manager.objects.Length];
            for (int i = 0; i < manager.objects.Length; i++)
            {
                if (tracked_objects[i] == null)
                {
                    tracked_objects[i] = manager.objects[i].GetComponent<SteamVR_TrackedObject>();// controller_go.GetComponent<SteamVR_TrackedObject>();
                }
                controller_indices[i] = (int)tracked_objects[i].index;
                if (controller_indices[i] >= 0)
                {
                    initialized[i] = true;
                }
                controllers[i] = tracked_objects[i].gameObject.transform;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e); // might need more detailed printing...
        }
    }

    // Use this for initialization
    void Start () {
        //screenCursor = CURSOR;

        Init();
    }

    void Update()
    {
        Init();
        //update status of controller button inputs
        triggerPress = new bool[controllers.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            if (!ControllerInputManager.initialized[i]) continue;

            try
            {
                var input = SteamVR_Controller.Input(controller_indices[i]);
                triggerPress[i] = input.GetHairTriggerDown();

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log("i " + i + " index " + controller_indices[i]);
            }
        }
    }

    //TODO: it might be useful in the future to cache raycast requests in cases of many requesters
    // but for now, we'll say that's premature optimization...
    public static CursorInput RequestInputStatus(GameObject requester)
    {
       // Debug.Log("Request");


        //Debug.Log("Received Query");
        // request collision status of cursors
        //GameObject leftControllerCollision = leftCursor.CollidingObject;
        //GameObject rightControllerCollision = rightCursor.CollidingObject;

        // perform raycasts
        int layerMask = 1 << LayerMask.NameToLayer("CursorTarget");
        //collision preprocessing: getting 'closest_i' and 'anyHitThis' allows us to avoid tedious if else's with left and right
        bool[] didHitThis = new bool[controllers.Length];
        RaycastHit[] raycastHits = new RaycastHit[controllers.Length];
        bool anyHitThis = false;
        int closest_i = -1; // only matters if anyHitThis
        for (int i = 0; i < controllers.Length; i++)
        {
            if (!initialized[i]) continue;

            Ray r = new Ray(controllers[i].position, controllers[i].forward);
            bool didHit = Physics.Raycast(r, out raycastHits[i], maxRaycast, layerMask);
            didHitThis[i] = didHit && raycastHits[i].transform != null && raycastHits[i].transform.gameObject.GetInstanceID() == requester.GetInstanceID();
            if (didHitThis[i])
            {
                anyHitThis = true;
                if (closest_i == -1 || raycastHits[closest_i].distance > raycastHits[i].distance)
                {
                    closest_i = i;
                }
            }
        }
            

        // now pack this info into an object:
        CursorInput input = new CursorInput();
        input.MouseScroll = Vector2.zero; //TODO:  mouse scroll stuff
        input.MouseHasFocus = anyHitThis;
           
        if (input.MouseHasFocus)
        {
            int closest_controller_id = controller_indices[closest_i];
            RaycastHit closestHit = raycastHits[closest_i];

            //screenCursor.SetActive(true);
            //screenCursor.SetPosition(closestHit.point);
                
            input.MousePosition = closestHit.textureCoord;

            var buttons = SteamVR_Controller.Input(closest_controller_id);
                

            input.LeftClick = buttons.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);//buttons.GetHairTrigger();
            // input.RightClick = buttons.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
            bool touchpadclick = buttons.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
            Vector2 touchpadloc = buttons.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            input.BackPage = touchpadclick && touchpadloc.x < 0f;
            input.ForwardPage = touchpadclick && touchpadloc.x > 0f;

            if (input.LeftClick || input.RightClick)
            {
                Debug.Log("Left Click: " + input.LeftClick + " Right Click " + input.RightClick);
            }
        }
        else
        {
            //screenCursor.SetActive(false);
        }
            

        //if (input.MousePosition != Vector2.zero)
        //{
        //    Debug.Log(input.MousePosition);
        //}

        return input;
    }
}
