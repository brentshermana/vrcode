using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity.Interaction;
using Leap.Unity;

using ZenFulcrum.EmbeddedBrowser;

public class InteractionBrowser : InteractionBehaviour, IBrowserUI
{
#region Monobehaviour

    // Use this for initialization
    void Start()
    {
        // Set InteractionBehaviour Events
        OnPrimaryHoverBegin = onHoverBegin;
        OnPrimaryHoverEnd = onHoverEnd;
        OnPrimaryHoverStay = onHoverStay;

        OnContactBegin = onContactBegin;
        OnContactStay = onContactStay;
        OnContactEnd = onContactEnd;

        // tell the browser that this object controls the UI
        browser = GetComponent<Browser>();
        if (browser == null)
        {
            browser = GetComponentInChildren<Browser>();
        }
        browser.UIHandler = this;
        inputSettings = new BrowserInputSettings(); //TODO:  set special options?
        browserCursor = new BrowserCursor(); // do we care about anything special?
        browserCursor.SetActiveCursor(BrowserNative.CursorType.Pointer);
        keyEvents = new List<Event>(); // TODO: POPULATE?
        //magnifyingGlass.SetEnabled(false);
    }

    // Update is called once per frame
    void Update()
    {
        NextInput.MouseHasFocus = hovering || touching;
        if (NextInput.MouseHasFocus)
        {
            // Get Mouse Position
            RaycastHit hit;
            if (CursorRaycast(out hit))
            {
                // NextInput.MousePosition = hit.textureCoord; // TextureCoord doesn't work on non-mesh colliders

                //Manually calculate the UV Coordinates:
                Vector3 screenScale = screen.transform.lossyScale;
                Vector3 localHitPoint = screen.transform.InverseTransformPoint(hit.point);
                // The next line assumes that the collider is a cube or quad, and that
                //  The front (or rear) face is the one being hit
                // Local Coordinates will range from -.5 to .5, but we need 0 to 1
                // Additionally, the x axis must be reversed because Unity's positive x is left
                NextInput.MousePosition = new Vector2(-1f*localHitPoint.x + .5f, localHitPoint.y + .5f);
                
                LastMousePosition = NextInput.MousePosition;
                // While we're here, set the cursor's position
                Cursor.position = hit.point;
            }
            else
            {
                NextInput.MousePosition = LastMousePosition;
            }
            
            // Set Click Status
            NextInput.LeftClick = touching;
            touching = false; //TODO: REMOVE IF YOU DON'T WANT INSTANTANEOUS CLICKS
        }
    }
#endregion

#region InteractionBehaviour

    private bool hovering;
    private bool touching;
    public float maxraycast = 1f;
    //public MagnifyingGlass magnifyingGlass;
    public GameObject screen;

    void onHoverBegin()
    {
        hovering = true;
    }
    void onHoverStay()
    {
        hovering = true;
    }
    void onHoverEnd()
    {
        hovering = false;
    }

    void onContactBegin()
    {
        touching = true;
    }
    void onContactStay()
    {
        // touching = true;
    }
    void onContactEnd()
    {
        touching = false;
    }

    private bool CursorRaycast(out RaycastHit hit)
    {
        Vector3 rayDirection = UnityVectorExtension.ToVector3(primaryHoveringFinger.Direction);
        Ray ray = new Ray(UnityVectorExtension.ToVector3(primaryHoveringFinger.StabilizedTipPosition), rayDirection);
        // Not a good idea to use a mask because leap plays around with layers at runtime
        //int mask = 1 << screen.layer; // the goal is to fire a raycast at the screen
        RaycastHit[] hits = Physics.RaycastAll(ray, maxraycast);
        foreach (RaycastHit h in hits)
        {
            if (h.collider.gameObject.Equals(screen))
            {
                //Quaternion rotation = Quaternion.LookRotation(transform.forward, transform.up);
                //magnifyingGlass.SetEnabled(true);
                //magnifyingGlass.Reposition(hit.point, rotation);

                hit = h;
                return true;
            }
        }
        hit = new RaycastHit(); // dummy value
        return false;
    }

    #endregion

    #region IBrowserUI

    public Transform Cursor;
    
    private Vector2 LastMousePosition;

    private Browser browser;

    private CursorInput CurrentInput;
    private CursorInput NextInput = new CursorInput();

    /** Called once per frame by the browser before fetching properties. */
    public void InputUpdate()
    {
        CurrentInput = NextInput;
        NextInput = new CursorInput();
    }

    /**
	 * Returns true if the browser will be getting mouse events. Typically this is true when the mouse if over the browser.
	 * 
	 * If this is false, the Mouse* properties will be ignored.
	 */
    public bool MouseHasFocus
    {
        get
        {
            if (CurrentInput.MouseHasFocus) Debug.Log("Mouse Has Focus!");
            return CurrentInput.MouseHasFocus;
        }
    }

    /**
	 * Current mouse position.
	 * 
	 * Returns the current position of the mouse with (0, 0) in the bottom-left corner and (1, 1) in the 
	 * top-right corner.
	 */
    public Vector2 MousePosition
    {
        get
        {
            Debug.Log(CurrentInput.MousePosition);
            return CurrentInput.MousePosition;
        }
    }

    /** Bitmask of currently depressed mouse buttons */
    public MouseButton MouseButtons
    {
        get
        {
            MouseButton buttons = 0;
            if (CurrentInput.LeftClick)
            {
                buttons = buttons | MouseButton.Left;
            }
            if (CurrentInput.RightClick)
            {
                buttons = buttons | MouseButton.Right;
            }
            return buttons;
        }
    }

    /**
	 * Delta X and Y scroll values since the last time InputUpdate() was called.
	 * 
	 * Return 1 for every "click" of the scroll wheel.
	 * 
	 * Return only integers.
	 */
    //TODO: set
    private Vector2 mouseScroll;
    public Vector2 MouseScroll { get { return CurrentInput.MouseScroll; } }

    /**
	 * Returns true when the browser will receive keyboard events.
	 * 
	 * In the simplest case, return the same value as MouseHasFocus, but you can track focus yourself if desired.
	 * 
	 * If this is false, the Key* properties will be ignored.
	 */
    public bool KeyboardHasFocus { get { return true; } }

    /**
	 * List of key up/down events that have happened since the last InputUpdate() call.
	 * 
	 * The returned list is not to be altered or retained.
	 */
    //TODO: set
    private List<Event> keyEvents;
    public List<Event> KeyEvents { get { return KeyboardInputManager.FlushEvents(); } }

    /**
	 * Returns a BrowserCursor instance. The Browser will update the current cursor to reflect the
	 * mouse's position on the page.
	 * 
	 * The IBrowserUI is responsible for changing the actual cursor, be it the mouse cursor or some in-game display.
	 */
    private BrowserCursor browserCursor;
    public BrowserCursor BrowserCursor { get { return browserCursor; } }

    /**
	 * These settings are used to interpret the input data.
	 */
    private BrowserInputSettings inputSettings;
    public BrowserInputSettings InputSettings { get { return inputSettings; } }

#endregion
}
