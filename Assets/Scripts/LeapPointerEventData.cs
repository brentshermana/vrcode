using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Leap;
using Leap.Unity;

public struct LeapPointerStatus
{
    public bool Set;
    public bool Contact;
    public bool Scrolling;
    public bool Clicking;
    public bool Hovering;
    public GameObject TheObject;
}

public class LeapPointerEventData : PointerEventData
{
    private float Pullback;
    private float MaxRaycast;

    public Hand Hand;

    private LeapPointerStatus status;
    public LeapPointerStatus Status
    {
        get
        {
            if (!Status.Set)
            {
                bool touching = Touching();
                bool pointPose = PointPoseDetected();

                status.Contact = touching;
                status.Scrolling = !pointPose && touching;
                status.Clicking = pointPose && touching;
                status.Set = true;
                status.Hovering = !touching && Hovering();
            }
            return status;
        }
    }

    public LeapPointerEventData(EventSystem es, float pullback, float maxRaycast) : base(es) {
        pullback = Mathf.Abs(pullback);
        MaxRaycast = maxRaycast;
    }

    // for now we just treat the index finger as the ray source
    // TODO: check if index finger is in a plausible "pointing"
    //      pose?
    public Ray GetRayForHand()
    {
        Finger indexFinger = Hand.GetIndex();
        Vector3 direction = indexFinger.Direction.ToVector3();
        Vector3 origin = indexFinger.StabilizedTipPosition.ToVector3() - direction.normalized * Pullback;
        Ray ray = new Ray(
            origin,
            direction
        );
        return ray;
    }

    private bool Touching()
    {
        return pointerCurrentRaycast.isValid &&
            pointerCurrentRaycast.distance < Pullback;
    }

    private bool Hovering()
    {
        return pointerCurrentRaycast.isValid &&
            pointerCurrentRaycast.distance < Pullback+MaxRaycast;
    }

    // TODO: we may want a utilities class for detecting
    // various hand positions
    private bool PointPoseDetected()
    {
        /*
         * We consider a hand to be 'pointing'
         * if there is a high angle between two
         * non-thumb fingers
         */
        Vector3 pointerFingerDirection = Hand.GetIndex().Direction.ToVector3();
        foreach (Finger f in Hand.Fingers)
        {
            if (
                f == null ||
                f.Type == Finger.FingerType.TYPE_THUMB
            ) continue;

            float angle = Vector3.Angle(pointerFingerDirection, f.Direction.ToVector3());
            if (angle > 30f)
            {
                return true;
            }
        }
        return false;
    }
}
