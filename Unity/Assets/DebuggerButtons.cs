using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vrcode.ide.debugger.frontend;
using vrcode.vr;

[RequireComponent(typeof(ButtonArranger))]
public class DebuggerButtons : MonoBehaviour
{
	[SerializeField] private float letter_dimension;
	[SerializeField] private float travel;
	[SerializeField] private float width_factor;

	[SerializeField] private DBFrontend debugger;
	
	[SerializeField] private Transform textButtonPrefab;

	private ButtonArranger buttonArranger;

	// Use this for initialization
	void Start ()
	{
		buttonArranger = GetComponent<ButtonArranger>();
		
		CreateButton("Continue", () =>
		{
			debugger.ContinueExecution();
		});
		
		CreateButton("Next", () =>
		{
			debugger.Next();
		});
		
		CreateButton("Step", () =>
		{
			debugger.Step();
		});
		
		CreateButton("Run", () =>
		{
			TheEnvironment.Rewrite();
			debugger.StartDebugging(TheEnvironment.GetSourceFilePath());
		});

	}

	void CreateButton(string face, Action whenPressed)
	{
		Transform button = Instantiate(textButtonPrefab);

		TextButton tb = button.GetComponent<TextButton>();
		
		tb.letter_dimension = letter_dimension;
		tb.travel = travel;
		tb.width_factor = width_factor;
		tb.button_face = face;

		tb.OnPressAction = whenPressed;
		
		tb.Initialize();
		
		buttonArranger.AddButton(button.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
