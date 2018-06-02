using UnityEngine;
using System;
using System.Collections.Generic;
using vrcode.input.keyboard;


namespace CurvedVRKeyboard {

    [SelectionBase]
    public class KeyboardStatus: KeyboardComponent {

        public List<Event> eventQueue = new List<Event>();

        void Update()
        {
            // unload the events one at a time so keyup and keydown aren't
            // processed the same frame (key doesn't get registered)
            //TODO: ^^^ I FOUND OUT THAT THIS ISN'T NECESSARY, CAN BE REMOVED
            if (eventQueue.Count >= 1)
            {
                Event e = eventQueue[0];
                KeyboardInputManager.AddEvent(e);
                eventQueue.RemoveAt(0);
            }
        }

        public string output;
        //public List<Event> keyevents = new List<Event>();
        
        //----CurrentKeysStatus----
        [SerializeField]
        public Component typeHolder;
        [SerializeField]
        public bool isReflectionPossible;
        private KeyboardItem[] keys;
        private bool areLettersActive = true;
        private bool isLowercase = true;
        private const char BLANKSPACE = ' ';
        private const string TEXT = "text";
        private Component textComponent;


        /// <summary>
        /// Handles click on keyboarditem
        /// </summary>
        /// <param name="clicked">keyboard item clicked</param>
        public void HandleClick ( KeyboardItem clicked ) {
            string value = clicked.GetValue();
            if(value.Equals(QEH) || value.Equals(ABC)) { // special signs pressed
                ChangeSpecialLetters();
            } else if(value.Equals(UP) || value.Equals(LOW)) { // upper/lower case pressed
                LowerUpperKeys();
            } else if(value.Equals(SPACE)) {
                TypeKey(BLANKSPACE);
            } else if(value.Equals(BACK)) {
                BackspaceKey();
            } else {// Normal letter
                TypeKey(value[0]);
            }
        }

        /// <summary>
        /// Displays special signs
        /// </summary>
        private void ChangeSpecialLetters () {
            KeyLetterEnum ToDisplay = areLettersActive ? KeyLetterEnum.NonLetters : KeyLetterEnum.LowerCase;
            areLettersActive =!areLettersActive;
            isLowercase = true;
            for(int i = 0;i < keys.Length;i++) {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        /// <summary>
        /// Changes between lower and upper keys
        /// </summary>
        private void LowerUpperKeys () {
            KeyLetterEnum ToDisplay = isLowercase ? KeyLetterEnum.UpperCase : KeyLetterEnum.LowerCase;
            isLowercase = !isLowercase;
            for(int i = 0;i < keys.Length;i++) {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        private void BackspaceKey () {
            output = output.Substring(0, output.Length-1);
            eventQueue.AddRange(KeyCharMapping.KeyEvents('\b'));
        }

        private void TypeKey ( char key ) {
            Debug.Log("Keyboard got char " + key);
            output = output + key.ToString();
            eventQueue.AddRange(KeyCharMapping.KeyEvents(key));
        }

        public void SetKeys ( KeyboardItem[] keys ) {
            this.keys = keys;
        }

        public void setOutput (ref string stringRef) {
            output = stringRef;
        }
    }
}
