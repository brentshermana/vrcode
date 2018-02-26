//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//
//using Leap;
//using Leap.Unity;
//
//using UnityEngine;
//using UnityEngine.EventSystems;
//
//namespace LeapHandStates
//{
//    
//    public class LeapHandFSM
//    {
//        ILeapHandState CurrentState;
//
//        public LeapHandFSM()
//        {
//            CurrentState = new InactiveState();
//        }
//        
//
//        public void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            CurrentState = CurrentState.Transition(pointerData, inputModule);
//            CurrentState.Update(pointerData, inputModule);
//        }
//    }
//
//    #region States
//
//    interface ILeapHandState
//    {
//        ILeapHandState Transition(LeapPointerEventData pointerData, LeapUIInputModule inputModule);
//        void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule);
//    }
//
//    abstract class LeapHandState : ILeapHandState
//    {
//        public virtual void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule) {
//
//        }
//
//        public virtual void UpdateCursor(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            // TODO: implement
//            inputModule.SetCursor(pointerData.Hand.Id, pointerData);
//        }
//
//        // it's important to tell the input system when contact occurs so that inertial scroll can end
//        public virtual void Contact(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            // TODO: implement
//            inputModule.Contact(pointerData.pointerCurrentRaycast.gameObject, pointerData.Hand.Id);
//        }
//        
//
//        // abstracting out the control flow for transitions:
//        public ILeapHandState Transition(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            ILeapHandState ret = TransitionOnAny(pointerData, inputModule);
//            if (ret == null)
//            {
//                if (pointerData.Status.Contact)
//                {
//                    ret = TransitionOnContact(pointerData, inputModule);
//                    if (ret == null)
//                    {
//                        if (pointerData.Status.Clicking)
//                        {
//                            ret = TransitionOnClick(pointerData, inputModule);
//                        }
//                        else if (pointerData.Status.Scrolling)
//                        {
//                            ret = TransitionOnScroll(pointerData, inputModule);
//                        }
//                        else
//                        {
//                            Debug.LogError("Contact, but neither scrolling nor clicking?");
//                        }
//                    }
//                }
//                else if (pointerData.Status.Hovering)
//                {
//                    ret = TransitionOnHover(pointerData, inputModule);
//                }
//                else
//                {
//                    ret = TransitionOnDefault(pointerData, inputModule);
//                }
//            }
//
//            // trigger event for transitioning out of state
//            if (ret != this)
//            {
//                OnTransitionOut(pointerData, inputModule);
//            }
//
//            if (ret == null)
//            {
//                Debug.LogError("One or more of the required transition functions were not implemented!");
//            }
//            return ret;
//        }
//
//        // for when the state is about to transition to another
//        protected virtual void OnTransitionOut(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { }
//        // define transitions
//        protected virtual ILeapHandState TransitionOnAny(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return null; }
//        protected virtual ILeapHandState TransitionOnClick(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return new ClickDownState(); }
//        protected virtual ILeapHandState TransitionOnContact(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return null; }
//        protected virtual ILeapHandState TransitionOnScroll(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return new ScrollState(); }
//        protected virtual ILeapHandState TransitionOnHover(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return new HoverState(); }
//        protected virtual ILeapHandState TransitionOnDefault(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return new InactiveState(); }
//    }
//
//    class InactiveState : LeapHandState
//    {
//        protected override ILeapHandState TransitionOnDefault(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return this; }
//        public override void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            Contact(pointerData, inputModule);
//            UpdateCursor(pointerData, inputModule);
//        }
//    }
//
//    class HoverState : LeapHandState
//    {
//        protected override ILeapHandState TransitionOnHover(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return this; }
//        public override void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            UpdateCursor(pointerData, inputModule);
//        }
//    }
//    
//    class ClickDownState : LeapHandState
//    {
//        protected Vector2 downCoordinate;
//
//        protected override ILeapHandState TransitionOnContact(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return new InactiveContactState(); }
//        public override void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            downCoordinate = pointerData.pressPosition;
//            Contact(pointerData, inputModule);
//            UpdateCursor(pointerData, inputModule);
//            inputModule.ClickDown(pointerData);
//        }
//        protected override void OnTransitionOut(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            inputModule.ClickUp(pointerData);
//        }
//    }
//
//    class InactiveContactState : LeapHandState
//    {
//        protected override ILeapHandState TransitionOnContact(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return this; }
//    }
//
//    class ScrollState : LeapHandState
//    {
//        Vector2 LastPosition;
//        Vector2 LastDelta;
//        GameObject ScrollingObject;
//
//        protected override ILeapHandState TransitionOnContact(LeapPointerEventData pointerData, LeapUIInputModule inputModule) { return this; }
//        public override void Update(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            Contact(pointerData, inputModule);
//            UpdateCursor(pointerData, inputModule);
//
//            GameObject currentObject = pointerData.pointerCurrentRaycast.gameObject;
//            if (currentObject == null)
//                Debug.LogError("We're in the scroll state, but the raycast hasn't hit an object!");
//            bool firstFrameOnObject = currentObject != ScrollingObject;
//
//            if (firstFrameOnObject)
//            {
//                ImpartInertialScroll(inputModule, pointerData.Hand.Id);
//                ScrollingObject = currentObject;
//                LastPosition = pointerData.position;
//            }
//            else
//            {
//                Vector2 delta = LastPosition - pointerData.position;
//                Scroll(inputModule, pointerData.Hand.Id, delta);
//
//                LastPosition = pointerData.position;
//                LastDelta = delta;
//            }
//        }
//
//        protected override void OnTransitionOut(LeapPointerEventData pointerData, LeapUIInputModule inputModule)
//        {
//            ImpartInertialScroll(inputModule, pointerData.Hand.Id);
//        }
//
//        protected void ImpartInertialScroll(LeapUIInputModule inputModule, int handID)
//        {
//            if (ScrollingObject != null)
//            {
//                inputModule.InertialScroll(handID, ScrollingObject, LastDelta/Time.deltaTime);
//            }
//        }
//
//        protected void Scroll(LeapUIInputModule inputModule, int handID, Vector2 delta)
//        {
//            inputModule.Scroll(handID, ScrollingObject, delta);
//        }
//    }
//
//    #endregion
//}
//
