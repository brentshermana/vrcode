using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using vrcode.ide.debugger.frontend;
using vrcode.networking.message;
using vrcode.vr;

[RequireComponent(typeof(ButtonArranger))]
public class EnvironmentDisplayer : MonoBehaviour {

	[SerializeField] private float letter_dimension;
	[SerializeField] private float travel;
	[SerializeField] private float width_factor;
	
	[SerializeField] private Transform textButtonPrefab;

	private ButtonArranger buttonArranger;

	// Use this for initialization
	void Start ()
	{
		buttonArranger = GetComponent<ButtonArranger>();
	}

	public void Clear()
	{
		buttonArranger.Clear();
	}

	public void DisplayEnvironment(DBEnvironment env)
	{
		Clear();
		foreach (Variable v in env.locals)
		{
			if (v.name.StartsWith("__")) continue; // ignore hidden variables
			else if (v.name == "imp") continue; // special case for now... imp is used by backend to boot debugger
			
			string buttonstring = v.name + " => " + v.val;
			buttonstring = buttonstring.Replace(' ', '_');
			buttonstring = buttonstring.Replace('\\', '-'); // for some reason loading backslashes doesn't work
			Debug.Log("Displaying Environment Variable:" + v.name + "\n" + buttonstring);
			CreateButton(buttonstring);
		}
	}

	private void CreateButton(string face)
	{
		face = face.Replace(' ', '_'); // necessary for now because there isn't any texture for an empty space
		
		Transform button = Instantiate(textButtonPrefab);

		TextButton tb = button.GetComponent<TextButton>();
		
		tb.letter_dimension = letter_dimension;
		tb.travel = travel;
		tb.width_factor = width_factor;
		tb.button_face = face;

		//tb.OnPressAction = whenPressed;
		
		tb.Initialize();
		
		buttonArranger.AddButton(button.gameObject);
	}
}
