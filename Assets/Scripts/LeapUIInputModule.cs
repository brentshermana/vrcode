using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Leap;
using Leap.Unity;
using UnityEngine.EventSystems;

using LeapHandStates;

public class LeapUIInputModule : StandaloneInputModule {

    // distance behind fingertips to launch raycast from,
    // ensuring a correct raycast hit when penetrating a
    // UI panel
    [SerializeField]
    float FingerRayPullback;
    [SerializeField]
    float HoverRaycastDistance;
    [SerializeField]
    float InertialScrollDeceleration;
    [SerializeField]
    LeapProvider leapProvider;
    [SerializeField]
    GameObject CursorPrefab;

    #region Variables

    protected Dictionary<int, LeapHandFSM> HandFSMs;
    protected Dictionary<int, LeapPointerEventData> HandEventData;
    protected Dictionary<int, GameObject> HandCursors;

    protected Dictionary<GameObject, InertialScrollInstance> InertialScrollers;

    protected Frame LatestLeapFrame;
    static CurvedUIInputModule instance;
    GameObject currentDragging;
    GameObject currentPointedAt;

    #endregion

    private void UpdateLeapFrame(Frame f)
    {
        // we copy because frame objects are mutable
        LatestLeapFrame = new Frame().CopyFrom(f);
    }

	// Use this for initialization
	void Start () {
        HandFSMs = new Dictionary<int, LeapHandFSM>();
        HandEventData = new Dictionary<int, LeapPointerEventData>();
        HandCursors = new Dictionary<int, GameObject>();

        InertialScrollers = new Dictionary<GameObject, InertialScrollInstance>();

        LatestLeapFrame = null;
        leapProvider.OnUpdateFrame += UpdateLeapFrame;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // called by UI once every Update() to process events
    public override void Process()
    {
        if (leapProvider == null || LatestLeapFrame == null) return;

        // send update events if there is a selected object - this is important for InputField to receive keyboard events
        SendUpdateEventToSelectedObject();

        foreach (Hand h in LatestLeapFrame.Hands)
            ProcessHand(h);
        
        foreach (GameObject scrolledObject in InertialScrollers.Keys)
        {
            Vector2 scroll = InertialScrollers[scrolledObject].GetScroll(Time.deltaTime);
            Scroll(-1, scrolledObject, scroll);
            if (Mathf.Approximately(scroll.sqrMagnitude, 0f))
                InertialScrollers.Remove(scrolledObject);
        }
    }

    protected void ProcessHand(Hand hand)
    {
        LeapPointerEventData handData = GetHandPointerData(hand);

        currentPointedAt = handData.pointerCurrentRaycast.gameObject;
        
        if (!HandFSMs.ContainsKey(hand.Id))
        {
            HandFSMs.Add(hand.Id, new LeapHandFSM());
        }
        HandFSMs[hand.Id].Update(handData, this);
        
    }

    protected LeapPointerEventData GetHandPointerData(Hand h)
    {
        if (!HandEventData.ContainsKey(h.Id))
        {
            HandEventData[h.Id] = new LeapPointerEventData(eventSystem, FingerRayPullback, HoverRaycastDistance);
        }
        LeapPointerEventData handData = HandEventData[h.Id];

        handData.Reset();
        handData.delta = Vector2.one; // is this necessary?
        handData.Hand = h;

        // Todo: handData.scrollDelta
        // Todo: click events
        RaycastResult raycast = handData.pointerCurrentRaycast;

        // Delegates to LeapRaycaster's Raycast method
        eventSystem.RaycastAll(handData, m_RaycastResultCache);

        handData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();

        return handData;
    }

    #region HandFSMInterface
    public void ClickDown(LeapPointerEventData eventData)
    {
        // TODO: raise click down event
        eventData.eligibleForClick = true;
        eventData.delta = Vector2.zero;
        eventData.dragging = false;
        eventData.useDragThreshold = true;
        eventData.pressPosition = eventData.position;
        eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

        GameObject currentOver = eventData.pointerCurrentRaycast.gameObject;

        DeselectIfSelectionChanged(currentOver, eventData);

        if (eventData.pointerEnter != currentOver)
        {
            HandlePointerExitAndEnter(eventData, currentOver);
            eventData.pointerEnter = currentOver;
        }

        GameObject pressed = ExecuteEvents.ExecuteHierarchy(
            currentOver,
            eventData,
            ExecuteEvents.pointerDownHandler);
        if (pressed == null)
        {
            pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOver);
        }

        eventData.clickCount = 1;
        // ^^^ CurvedUIInputModule checks for double clicks
        // and things like that, but I don't care

        // TODO: in the event we care about drag events, we need more stuff
    }
    public void ClickUp(LeapPointerEventData eventData)
    {
        GameObject currentOver = eventData.pointerCurrentRaycast.gameObject;

        ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

        // see if we mouse up on the same element that we clicked on...
        var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOver);

        // PointerClick and Drop events
        if (eventData.pointerPress == pointerUpHandler && eventData.eligibleForClick)
        {
            ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
            //Debug.Log("click");
        }
        else if (eventData.pointerDrag != null && eventData.dragging)
        {
            ExecuteEvents.ExecuteHierarchy(currentOver, eventData, ExecuteEvents.dropHandler);
            //Debug.Log("drop");
        }

        eventData.eligibleForClick = false;
        eventData.pointerPress = null;
        eventData.rawPointerPress = null;

        if (eventData.pointerDrag != null && eventData.dragging)
        {
            ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
            //Debug.Log("end drag");
        }

        eventData.dragging = false;
        eventData.pointerDrag = null;

        // send exit events as we need to simulate this on touch up on touch device
        ExecuteEvents.ExecuteHierarchy(eventData.pointerEnter, eventData, ExecuteEvents.pointerExitHandler);
        eventData.pointerEnter = null;
    }
    public void SetCursor(int handId, LeapPointerEventData pointerData)
    {
        // TODO: need a mapping of hand ids to cursor instances
        if (!HandCursors.ContainsKey(handId))
        {
            HandCursors.Add(handId, Instantiate(CursorPrefab));
        }
        Transform cursor = HandCursors[handId].transform;

        cursor.position = pointerData.pointerCurrentRaycast.worldPosition;
    }
    public void Contact(GameObject touched, int handId)
    {
        // TODO: remove any inertial scrolling from touched
        if (InertialScrollers.ContainsKey(touched))
        {
            InertialScrollers.Remove(touched);
        }
    }
    public void Scroll(int handId, GameObject scrolledObject, Vector2 scroll)
    {
        // TODO: scroll the specified amount this frame
        PointerEventData fakeData = new PointerEventData(eventSystem);
        fakeData.scrollDelta = scroll;
        GameObject scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(scrolledObject);
        ExecuteEvents.ExecuteHierarchy(scrollHandler, fakeData, ExecuteEvents.scrollHandler);
    }
    public void InertialScroll(int handId, GameObject scrolledObject, Vector2 scrollV)
    {
        // TODO: insert an inertialScroll instance for the given object
        InertialScrollers[scrolledObject] = new InertialScrollInstance(scrollV, InertialScrollDeceleration);
    }
    #endregion
    
}
