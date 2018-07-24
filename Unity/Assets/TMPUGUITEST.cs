using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class TMPUGUITEST : MonoBehaviour
{
	private float start_time = 0f;
	private bool printed = false;

	// Use this for initialization
	void Start ()
	{
//		Debug.Log(tmp.autoSizeTextContainer);
//		tmp.autoSizeTextContainer = true;
		TMP_InputField field = GetComponent<TMP_InputField>();
		field.text = "<color=#333333>hello!</color>";
	}
	
	// Update is called once per frame
	void Update ()
	{
//		TMP_InputField field = GetComponent<TMP_InputField>();
//		Debug.Log(field.text);
//		Debug.Log(field.textComponent.textInfo.characterCount);
//		Debug.Log(field.caretPosition);
		
		
//		TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
//		Debug.Log(tmp.bounds.extents * 2f);
////		start_time += Time.deltaTime;
////		if (start_time > 5f && !printed)
////		{
////			TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
////			Debug.Log(tmp.textInfo.characterInfo[0].vertex_BL.position);
////			Debug.Log(tmp.textInfo.characterInfo[0].vertex_TR.position);
////
////			printed = true;
////		}
	}
}
