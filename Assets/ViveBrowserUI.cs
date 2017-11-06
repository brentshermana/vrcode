using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using ZenFulcrum.EmbeddedBrowser;

class ViveBrowserUI : MonoBehaviour, IBrowserUI
{
    /** Called once per frame by the browser before fetching properties. */
    public void InputUpdate()
    {
        //TODO: update properties
    }

    /**
	 * Returns true if the browser will be getting mouse events. Typically this is true when the mouse if over the browser.
	 * 
	 * If this is false, the Mouse* properties will be ignored.
	 */
     //TODO: SET
    private bool mouseHasFocus;
    public bool MouseHasFocus { get
        {
            return mouseHasFocus;
        }
    }

    /**
	 * Current mouse position.
	 * 
	 * Returns the current position of the mouse with (0, 0) in the bottom-left corner and (1, 1) in the 
	 * top-right corner.
	 */
    // TODO: SEt
    private Vector2 mousePosition;
    public Vector2 MousePosition { get
        {
            return mousePosition;
        }
    }

    /** Bitmask of currently depressed mouse buttons */
    //TODO: set
    private MouseButton mouseButtons;
    public MouseButton MouseButtons { get { return mouseButtons; } }

    /**
	 * Delta X and Y scroll values since the last time InputUpdate() was called.
	 * 
	 * Return 1 for every "click" of the scroll wheel.
	 * 
	 * Return only integers.
	 */
     //TODO: set
    private Vector2 mouseScroll;
    public Vector2 MouseScroll { get { return mouseScroll; } }

    /**
	 * Returns true when the browser will receive keyboard events.
	 * 
	 * In the simplest case, return the same value as MouseHasFocus, but you can track focus yourself if desired.
	 * 
	 * If this is false, the Key* properties will be ignored.
	 */
     //TODO: set
    private bool keyboardHasFocus;
    public bool KeyboardHasFocus { get { return keyboardHasFocus; } }

    /**
	 * List of key up/down events that have happened since the last InputUpdate() call.
	 * 
	 * The returned list is not to be altered or retained.
	 */
     //TODO: set
    private List<Event> keyEvents;
    public List<Event> KeyEvents { get { return keyEvents; } }

    /**
	 * Returns a BrowserCursor instance. The Browser will update the current cursor to reflect the
	 * mouse's position on the page.
	 * 
	 * The IBrowserUI is responsible for changing the actual cursor, be it the mouse cursor or some in-game display.
	 */
     //TODO: set
    private BrowserCursor browserCursor;
    public BrowserCursor BrowserCursor { get { return browserCursor; } }

    /**
	 * These settings are used to interpret the input data.
	 */
    //TODO: set
    private BrowserInputSettings inputSettings;
    public BrowserInputSettings InputSettings { get { return inputSettings; } }


}
