using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class Test : MonoBehaviour {

    private int i;
    private int j;

    public InputField inputField;
    public TMP_InputField tmField;
    public EventSystem es;

    public StandaloneInputModuleCustom inputModule;

    public GraphicRaycaster raycaster;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        //tmField.textComponent.textInfo.characterInfo[0].y
        if (tmField.isFocused)
        {
            i = tmField.selectionFocusPosition;
            j = tmField.selectionStringFocusPosition;
            Debug.Log("Selection Focus: " + i + " String Focus: " + j);
        }

        Vector3 mouseOrigin = Input.mousePosition;
        mouseOrigin.z = 0.0f;
        mouseOrigin = Camera.main.ScreenToWorldPoint(mouseOrigin);

        int layer = 1 << LayerMask.NameToLayer("UI");

        //Ray ray = new Ray(mouseOrigin, Camera.main.transform.forward);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit))
        //{
        //    Debug.Log("Hit at " + hit.point);
        //}

        Vector3 mp = Input.mousePosition;

        

        

    }

    void OnGUI()
    {
        List < RaycastResult > resultList = new List<RaycastResult>();
        PointerEventData pd = inputModule.GetLastPointerEventDataPublic(-1);
        if (pd != null) {
            raycaster.Raycast(pd, resultList);
            foreach (RaycastResult r in resultList)
                Debug.Log(r.worldPosition);
        }
        
        //Debug.Log("WP: " + pd.pointerCurrentRaycast.worldPosition);
    }

    //void OnGUI()
    //{
    //    Event e = Event.current;
    //    Debug.Log(e.mousePosition);
    //}

    //public void OnGUI()
    //{
    //    //Debug.Log("OnGUI!");
    //    //var ev = Event.current;
    //    //if (ev.type != EventType.KeyDown && ev.type != EventType.KeyUp) return;

    //    ////		if (ev.character != 0) Debug.Log("ev >>> " + ev.character);
    //    ////		else if (ev.type == EventType.KeyUp) Debug.Log("ev ^^^ " + ev.keyCode);
    //    ////		else if (ev.type == EventType.KeyDown) Debug.Log("ev vvv " + ev.keyCode);

    //    //keyEvents.Add(new Event(ev));
    //}
}
