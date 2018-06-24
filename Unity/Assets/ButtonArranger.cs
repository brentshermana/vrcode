using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonArranger : MonoBehaviour
{
	// offset between each button
	[SerializeField] private Vector3 incrementalOffset;
	// rotation of each button relative to this object
	[SerializeField] private Vector3 rotationalOffset;
	// position of the first button relative to this object
	[SerializeField] private Vector3 originOffset;
	
	// state variables:
	private List<GameObject> buttons = new List<GameObject>();
	private Vector3 nextButtonOffset;

	// Use this for initialization
	void Start ()
	{
		nextButtonOffset = originOffset;
	}

	public void Clear()
	{
		foreach (GameObject button in buttons)
		{
			Destroy(button);
		}

		buttons = new List<GameObject>();

		nextButtonOffset = originOffset;
	}

	public void AddButton(GameObject newButton)
	{
		newButton.transform.parent = transform;
		newButton.transform.localPosition = nextButtonOffset;
		newButton.transform.localRotation = Quaternion.Euler(rotationalOffset);

		nextButtonOffset += incrementalOffset;
		buttons.Add(newButton);
	}
}
