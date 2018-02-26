using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity.Interaction;
using Leap.Unity;
using vrcode.browser;
using vrcode.input.keyboard;
using ZenFulcrum.EmbeddedBrowser;

namespace vrcode.browser
{
    public class InteractionBrowser : InteractionBehaviour, IBrowserUI
    {
        #region Monobehaviour

        // Use this for initialization
        void Start()
        {
            // Set InteractionBehaviour Events

            state = new WaitingState();

            OnContactBegin = onContactBegin;
            OnContactEnd = onContactEnd;

            // tell the browser that this object controls the UI
            browser = GetComponent<Browser>();
            if (browser == null)
            {
                browser = GetComponentInChildren<Browser>();
            }

            browser.UIHandler = this;

            inputSettings = new BrowserInputSettings(); //TODO:  set special options?
            inputSettings.scrollSpeed = 1;
            browserCursor = new BrowserCursor(); // do we care about anything special?
            browserCursor.SetActiveCursor(BrowserNative.CursorType.Pointer);
            keyEvents = new List<Event>(); // TODO: POPULATE?
        }

        #endregion

        #region InteractionBehaviour

        //public HandPool handPool;
        public bool isTouching;

        public float maxraycast = 1f;

        //public MagnifyingGlass magnifyingGlass;
        public GameObject screen;


        void onContactBegin()
        {
            isTouching = true;
        }

        void onContactEnd()
        {
            isTouching = false;
        }

        public Ray GetRayForFinger(Finger finger)
        {
            Vector3 FingerDirection = UnityVectorExtension.ToVector3(primaryHoveringFinger.Direction);
            // we subtract the finger direction because we want the raycast to hit the screen even 
            //  if the finger is slightly penetrating the screen
            Vector3 FingerSource =
                UnityVectorExtension.ToVector3(finger.StabilizedTipPosition) - (FingerDirection * 0.2f);
            Ray ray = new Ray(FingerSource, FingerDirection);
            return ray;
        }

        public bool CursorRaycast(Ray ray, out RaycastHit hit)
        {
            // Not a good idea to use a mask because leap plays around with layers at runtime
            RaycastHit[] hits = Physics.RaycastAll(ray, maxraycast);
            foreach (RaycastHit h in hits)
            {
                if (h.collider.gameObject.Equals(screen))
                {
                    hit = h;
                    return true;
                }
            }

            hit = new RaycastHit(); // dummy value for failure to hit
            return false;
        }

        public Vector2 CalculateMousePosition(RaycastHit hit)
        {
            //Manually calculate the UV Coordinates:
            Vector3 localHitPoint = screen.transform.InverseTransformPoint(hit.point);
            // The next line assumes that the collider is a cube or quad, and that
            //  The front (or rear) face is the one being hit
            // Local Coordinates will range from -.5 to .5, but we need 0 to 1
            // Additionally, the x axis must be reversed because Unity's positive x is left
            return new Vector2(-1f * localHitPoint.x + .5f, localHitPoint.y + .5f);
        }

        #endregion

        #region IBrowserUI

        private IBrowserControllerState state;



        public float InertialScrollingDeceleration; // deceration of inertial speed in pixels/s/s

        public Transform Cursor;

        private Browser browser;

        private CursorInput CurrentInput;
        public CursorInput NextInput = new CursorInput();

        /** Called once per frame by the browser before fetching properties. */
        public void InputUpdate()
        {
            // transition to the appropriate current state
            state = state.Transition(this);
            // the next input values are altered according to the state
            state.InputUpdate(this);

            // make the next input available for reading
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
                //if (CurrentInput.MouseHasFocus) Debug.Log("Mouse Has Focus!");
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
                //Debug.Log(CurrentInput.MousePosition);
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

        public Vector2 MouseScroll
        {
            get { return CurrentInput.MouseScroll; }
        }

        /**
     * Returns true when the browser will receive keyboard events.
     * 
     * In the simplest case, return the same value as MouseHasFocus, but you can track focus yourself if desired.
     * 
     * If this is false, the Key* properties will be ignored.
     */
        public bool KeyboardHasFocus
        {
            get { return true; }
        }

        /**
     * List of key up/down events that have happened since the last InputUpdate() call.
     * 
     * The returned list is not to be altered or retained.
     */
        //TODO: set
        private List<Event> keyEvents;

        public List<Event> KeyEvents
        {
            get { return KeyboardInputManager.FlushEvents(); }
        }

        /**
     * Returns a BrowserCursor instance. The Browser will update the current cursor to reflect the
     * mouse's position on the page.
     * 
     * The IBrowserUI is responsible for changing the actual cursor, be it the mouse cursor or some in-game display.
     */
        private BrowserCursor browserCursor;

        public BrowserCursor BrowserCursor
        {
            get { return browserCursor; }
        }

        /**
     * These settings are used to interpret the input data.
     */
        private BrowserInputSettings inputSettings;

        public BrowserInputSettings InputSettings
        {
            get { return inputSettings; }
        }

        #endregion
    }
}

#region browser_states



interface IBrowserControllerState
{
    // called once per frame after transitioning
    void InputUpdate(InteractionBrowser ibrowser);

    // called once per frame, returns new state for current frame
    IBrowserControllerState Transition(InteractionBrowser ibrowser);
}

class WaitingState : IBrowserControllerState
{
    public WaitingState()
    {
    }

    public WaitingState(InertialScroller inertialScroller)
    {
        this.iScroller = inertialScroller;
    }

    public InertialScroller iScroller;

    public IBrowserControllerState Transition(InteractionBrowser ibrowser)
    {
        if (ibrowser.isPrimaryHovered)
            // presumably the browser will be primary hovered before it is touched
            return new PrimaryHoverState(iScroller);
        return this;
    }

    public void InputUpdate(InteractionBrowser ibrowser)
    {
        if (iScroller != null)
            iScroller.InertialScroll(ibrowser);
    }
}

// wraps the repetitive maintanence of cursor values which
// happens in many states (e.g. contact and hover)
abstract class CursorUpdater
{
    protected Finger RaycastFinger;
    protected bool PriorMousePositionSet;
    protected Vector2 LastMousePosition;

    public CursorUpdater(CursorUpdater cu)
    {
        RaycastFinger = cu.RaycastFinger;
        PriorMousePositionSet = cu.PriorMousePositionSet;
        LastMousePosition = cu.LastMousePosition;
    }

    public CursorUpdater(Finger finger, Vector2 lastMousePosition)
    {
        PriorMousePositionSet = true;
        LastMousePosition = lastMousePosition;
        RaycastFinger = finger;
    }

    public CursorUpdater(Finger finger)
    {
        RaycastFinger = finger;
    }

    public CursorUpdater(Vector2 lastMousePosition)
    {
        LastMousePosition = lastMousePosition;
    }

    public CursorUpdater()
    {
    }

    // This is what the class is all about
    protected void UpdateCursor(InteractionBrowser browser, bool updateRaycastFinger)
    {
        if (updateRaycastFinger && browser.primaryHoveringFinger != null)
        {
            RaycastFinger = browser.primaryHoveringFinger;
        }

        Ray ray = browser.GetRayForFinger(GetFinger(browser));
        RaycastHit hit;
        if (browser.CursorRaycast(ray, out hit))
        {
            browser.Cursor.position = hit.point;
            browser.NextInput.MousePosition = LastMousePosition = browser.CalculateMousePosition(hit);
            browser.NextInput.MouseHasFocus = true;
            PriorMousePositionSet = true;
        }
        else if (PriorMousePositionSet)
        {
            browser.NextInput.MousePosition = LastMousePosition;
            browser.NextInput.MouseHasFocus = true;
        }
    }

    protected Finger GetFinger(InteractionBrowser browser)
    {
        if (RaycastFinger != null)
        {
            return RaycastFinger;
        }
        else
        {
            // fall back on the primary finger
            return browser.primaryHoveringFinger;
        }
    }
}

// for any class (hover, waiting) which does not interfere with inertial scrolling
class InertialScroller
{
    public InertialScroller(Vector2 scrollV)
    {
        this.ScrollV = scrollV;
    }

    //private Vector2 carryOver;
    private Vector2 ScrollV;

    public void InertialScroll(InteractionBrowser browser)
    {
        browser.NextInput.MouseHasFocus = true; // otherwise scrolling won't be read
        Vector2 ScrollThisFrame = ScrollV * Time.deltaTime;
        browser.NextInput.MouseScroll = ScrollThisFrame; // scale by time
        // 
        Debug.Log("Inertia " + browser.NextInput.MouseScroll);
        ScrollV = Vector2.MoveTowards(ScrollV, Vector2.zero,
            browser.InertialScrollingDeceleration * Time.deltaTime);
    }
}

class PrimaryHoverState : CursorUpdater, IBrowserControllerState
{
    public PrimaryHoverState() : base()
    {
    }

    public PrimaryHoverState(InertialScroller inertialScroller)
    {
        iScroller = inertialScroller;
    }

    private Finger LastPrimaryHoveringFinger;
    private Hand LastPrimaryHoveringHand;
    private InertialScroller iScroller;

    public IBrowserControllerState Transition(InteractionBrowser ibrowser)
    {
        if (ibrowser.isTouching)
        {
            return new ClickDownState(this, LastPrimaryHoveringHand);
        }
        else if (!ibrowser.isPrimaryHovered)
        {
            return new WaitingState(iScroller);
        }
        else
        {
            return this;
        }
    }

    public void InputUpdate(InteractionBrowser browser)
    {
        if (iScroller != null)
            iScroller.InertialScroll(browser);
        LastPrimaryHoveringFinger = browser.primaryHoveringFinger;
        if (browser.primaryHoveringHand != null)
            LastPrimaryHoveringHand = browser.primaryHoveringHand;
        UpdateCursor(browser, true);
    }
}

/*
* branches based on whether the click is done with the index finger
* or not
*/
class ClickDownState : CursorUpdater, IBrowserControllerState
{
    protected bool DidClick;
    protected Hand TouchingHand;

    public ClickDownState(CursorUpdater cu, Hand touchingHand) : base(cu)
    {
        TouchingHand = touchingHand;
    }
    //public ClickDownState(Finger clickingFinger) : base(clickingFinger) { }
    //public ClickDownState(Finger clickingFinger, Vector2 lastMousePosition) : base(clickingFinger, lastMousePosition) { }

    public IBrowserControllerState Transition(InteractionBrowser browser)
    {
        if (DidClick)
            return new ClickUpState(this);
        else if (!browser.isTouching)
        {
            if (browser.isPrimaryHovered)
            {
                return new PrimaryHoverState();
            }
            else
            {
                return new WaitingState();
            }
        }
        else
            return new ScrollState(this);

    }

    public void InputUpdate(InteractionBrowser browser)
    {
        UpdateCursor(browser, true);
        List<Finger> smallFingers = TouchingHand.Fingers;
        Vector3 pointerFingerDirection = TouchingHand.GetIndex().Direction.ToVector3();
        foreach (Finger f in smallFingers)
        {
            if (f == null || f.Type == Finger.FingerType.TYPE_THUMB || f.Type == RaycastFinger.Type)
                continue;
            float angle = Vector3.Angle(pointerFingerDirection, f.Direction.ToVector3());
            Debug.Log(angle);
            if (angle > 30f)
            {
                // click
                browser.NextInput.LeftClick = true;
                DidClick = true;
                return;
            }
        }

        // all fingers are pointing in similar directions, so hover
    }
}

class ScrollState : CursorUpdater, IBrowserControllerState
{
    private Vector2 carryover; // sub-integer scroll distance rounded off from prev frame
    private Vector2 currentSpeed; // used for inertialScrolling

    public ScrollState(CursorUpdater cu) : base(cu)
    {
        currentSpeed = Vector2.zero;
    }

    public void InputUpdate(InteractionBrowser browser)
    {
        Vector2 prev_touch = base.LastMousePosition;
        base.UpdateCursor(browser, false);
        Vector2 screenDelta = prev_touch - base.LastMousePosition; // in percent of screen width and height
        screenDelta.Scale(browser.GetComponent<Browser>().Size); // scale to the number of pixels
        screenDelta += carryover;
        Vector2 roundedScreenDelta = new Vector2(Mathf.Round(screenDelta.x), Mathf.Round(screenDelta.y));
        carryover = screenDelta - roundedScreenDelta;
        currentSpeed = (screenDelta / Time.deltaTime + currentSpeed) / 2f; //TODO: some better form of smoothing?
        browser.NextInput.MouseScroll = roundedScreenDelta;

        //Debug.Log("scrolling! " + screenDelta);
    }

    public IBrowserControllerState Transition(InteractionBrowser browser)
    {
        if (browser.isTouching)
        {
            return this;
        }
        else if (browser.isPrimaryHovered)
        {
            return new PrimaryHoverState(new InertialScroller(currentSpeed));
        }
        else
        {
            return new WaitingState(new InertialScroller(currentSpeed));
        }
    }
}

// no clicking occurs in this state. necessary to provide mouse up event
class ClickUpState : CursorUpdater, IBrowserControllerState
{
    public ClickUpState(CursorUpdater cu) : base(cu)
    {
    }

    public IBrowserControllerState Transition(InteractionBrowser browser)
    {
        return new InactiveTouchingState(this);
    }

    public void InputUpdate(InteractionBrowser browser)
    {
        browser.NextInput.MouseHasFocus = true;
        browser.NextInput.MousePosition = LastMousePosition;
    }
}

// no clicking occurs in this state
class InactiveTouchingState : CursorUpdater, IBrowserControllerState
{
    public InactiveTouchingState(CursorUpdater cu) : base(cu)
    {
    }

    public void InputUpdate(InteractionBrowser browser)
    {
        UpdateCursor(browser, true);
    }

    public IBrowserControllerState Transition(InteractionBrowser browser)
    {
        if (browser.isTouching)
        {
            return this;
        }
        else if (browser.isPrimaryHovered)
        {
            return new PrimaryHoverState();
        }
        else
        {
            return new WaitingState();
        }
    }
}

#endregion