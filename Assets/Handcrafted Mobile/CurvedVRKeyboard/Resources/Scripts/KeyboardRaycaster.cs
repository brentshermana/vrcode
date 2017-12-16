using UnityEngine;
using System;
namespace CurvedVRKeyboard {

    public class KeyboardRaycaster: KeyboardComponent {

        //------Raycasting----
        //[SerializeField, HideInInspector]
        private Transform[] raycastingSources;
        

        private float rayLength;
        private Ray ray;
        private RaycastHit hit;
        private LayerMask layer;
        private float minRaylengthMultipler = 1.5f;
        //---interactedKeys---
        private KeyboardStatus keyboardStatus;
        private KeyboardItem[] keyItemCurrent;
        

        void Start () {
            keyboardStatus = gameObject.GetComponent<KeyboardStatus>();
            int layerNumber = gameObject.layer;
            layer = 1 << layerNumber;
        }

        void Update () {
            //reset local variables:
            raycastingSources = ControllerInputManager.controllers;
            if (keyItemCurrent == null)
            {
                keyItemCurrent = new KeyboardItem[raycastingSources.Length];
            }
            else if (keyItemCurrent.Length != raycastingSources.Length)
            {
                KeyboardItem[] old = keyItemCurrent;
                keyItemCurrent = new KeyboardItem[raycastingSources.Length];
                for (int i = 0; i < Math.Min(old.Length, keyItemCurrent.Length); i++)
                {
                    keyItemCurrent[i] = old[i];
                }
            }
            // do raycasts and check for clicks
            for (int i = 0; i < raycastingSources.Length; i++)
            {
                if (raycastingSources[i] != null && ControllerInputManager.initialized[i])
                {
                    RayCastKeyboard(raycastingSources[i], i);
                }
            }
            
        }

        /// <summary>
        /// Check if camera is pointing at any key. 
        /// If it does changes state of key
        /// </summary>
        private void RayCastKeyboard (Transform source, int source_index) {
            ray = new Ray(source.position, source.forward);
            if(Physics.Raycast(ray, out hit, float.PositiveInfinity, layer)) { // If any key was hit
                KeyboardItem focusedKeyItem = hit.transform.gameObject.GetComponent<KeyboardItem>();
                if (focusedKeyItem != null) { // Hit may occur on item without script
                    // change value of current target if needed
                    reselect(source_index, focusedKeyItem);
                    // if the controller is clicking, handle event
                    if (ControllerInputManager.triggerPress[source_index])
                    {
                        keyItemCurrent[source_index].Click();
                        keyboardStatus.HandleClick(keyItemCurrent[source_index]); 
                    }
                }
            } else {
                reselect(source_index, null);
            }
        }

        private void reselect(int index, KeyboardItem new_item)
        {
            if (keyItemCurrent[index] != null)
            {
                keyItemCurrent[index].StopHovering();
            }
            if (new_item != null)
            {
                new_item.Hovering();
            }
            keyItemCurrent[index] = new_item;
        }
        

        

        //public void SetRaycastingTransforms ( Transform[] raycastingSources ) {
        //    this.raycastingSources = raycastingSources;
        //}

        //public void SetClickButton ( string clickInputName ) {
        //    this.clickInputName = clickInputName;
        //}
        
    }
}