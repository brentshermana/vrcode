using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputManager : MonoBehaviour {

    static List<Event> keyboard_events = new List<Event>();

    // events are raised here
    public void OnGUI()
    {
        var ev = Event.current;
        
        if (ev.type != EventType.KeyDown && ev.type != EventType.KeyUp) return;
        Debug.Log("'Official Event' " + ev);
        keyboard_events.Add(new Event(ev));
    }

    private void Update()
    {
        //Unity doesn't include events for some keys, so fake it.
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            keyboard_events.Add(new Event() { type = EventType.KeyDown, keyCode = KeyCode.LeftShift });
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            //Note: doesn't matter if left or right shift, the browser can't tell.
            keyboard_events.Add(new Event() { type = EventType.KeyUp, keyCode = KeyCode.LeftShift });
        }
    }

    public static void AddEvent(Event e)
    {
        keyboard_events.Add(e);
    }
    

    public static List<Event> FlushEvents()
    {
        var old = keyboard_events;
        keyboard_events = new List<Event>();
        Debug.Log("Sending off " + old.Count + " Events to browser ");
        return old;
    }
}
