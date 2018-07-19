using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrcode.vr.keyboard;

public class ButtonArrangerDemo : MonoBehaviour
{

	[SerializeField] private Transform buttonPrefab;
	[SerializeField] private string[] strings;
	[SerializeField] private float charSize;

	void Start()
	{
		ButtonArranger ba = GetComponent<ButtonArranger>();

		foreach (string s in strings)
		{
			//GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			//go.transform.localScale = Vector3.one * .1f;

			GameObject button = Instantiate(buttonPrefab).gameObject;

			KeyUtils key = button.GetComponent<KeyUtils>();
			key.SetSpecialKeyInfo(s, s);
			key.SetSize(charSize * s.Length, charSize, charSize);
			key.SetTravel(.1f * charSize);
			
			ba.AddButton(button);
		}
	}
}
