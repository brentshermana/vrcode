using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    public int i;

    public InputField inputField;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log((int)'a');
        Debug.Log((int)'b');
        //var buttons = SteamVR_Controller.Input(i);

        //if ( buttons.GetPress(SteamVR_Controller.ButtonMask.Trigger) )//buttons.GetHairTrigger();
        //{
        //    Debug.Log("Trigger");
        //}

    }
    public void OnGUI()
    {
        //Debug.Log("OnGUI!");
        //var ev = Event.current;
        //if (ev.type != EventType.KeyDown && ev.type != EventType.KeyUp) return;

        ////		if (ev.character != 0) Debug.Log("ev >>> " + ev.character);
        ////		else if (ev.type == EventType.KeyUp) Debug.Log("ev ^^^ " + ev.keyCode);
        ////		else if (ev.type == EventType.KeyDown) Debug.Log("ev vvv " + ev.keyCode);

        //keyEvents.Add(new Event(ev));
    }
}
