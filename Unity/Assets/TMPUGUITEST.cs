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
	}
	
	// Update is called once per frame
	void Update ()
	{
		TMP_InputField field;
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
