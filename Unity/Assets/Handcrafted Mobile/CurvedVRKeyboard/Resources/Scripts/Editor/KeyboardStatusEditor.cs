using CurvedVRKeyboard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CurvedVRKeyboard {

    /// <summary>
    /// Special editor for keyboard status
    /// </summary>
    [CustomEditor(typeof(KeyboardStatus))]
    [CanEditMultipleObjects]
    public class KeyboardStatusEditor: Editor {

        #region GUI_STRINGS
        private static GUIContent OUTPUT = new GUIContent("Gameobject Output", "field receiving input from the keyboard (Text,InputField,TextMeshPro)");
        private static GUIContent OUTPUT_LENGTH = new GUIContent("Output Length", "Maximum output text length");
        private const string OUTPUT_TYPE = "Choose Output Script Type";
        #endregion


        private const string TEXT = "text";

        private KeyboardStatus keyboardStatus;
        private Component[] componentsWithText;
        private string[] scriptsNames;

        private bool notNullTargetAndChanged;
        private int currentSelected = 0;
        private int previousSelected = 0;
       
        private void Awake () {
            keyboardStatus = target as KeyboardStatus;

            
        }
        
        

        private void DrawTargetGameObjectFields () {
            
        }

        private void DrawPopupList () {
            currentSelected = EditorGUILayout.Popup(OUTPUT_TYPE, currentSelected, scriptsNames);

            if(previousSelected != currentSelected) {//if popup value was changed
                notNullTargetAndChanged = true;
            }
            previousSelected = currentSelected;
            

        }

        private void GetTextParameterViaReflection () {
            notNullTargetAndChanged = false;
            keyboardStatus.typeHolder = componentsWithText[currentSelected];
            keyboardStatus.output = (string)componentsWithText[currentSelected]
                .GetType().GetProperty(TEXT).GetValue(componentsWithText[currentSelected], null);
        }

        
        

        private void HandleValuesChanges () {
            if(GUI.changed) {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.SetDirty(keyboardStatus);
            }
        }
    }

}