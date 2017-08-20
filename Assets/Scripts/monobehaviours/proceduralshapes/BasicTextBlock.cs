using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class BasicTextBlock : MonoBehaviour {

	public float zDim;

	protected TextMeshPro textPro;
	protected ProceduralRect2 rect;

	// Use this for initialization
	public virtual void Start () {
		rect = GetComponent<ProceduralRect2> ();
		//get textPro from a child
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			TextMeshPro tmp = child.gameObject.GetComponent<TextMeshPro> ();
			if (tmp) {
				textPro = tmp;
				i = transform.childCount; //break
			}
		}
	}

	public virtual void Update() {
		Vector2 td = GetTextDim (textPro);
		Vector3 dimensions = new Vector3 (td.x, td.y, zDim);
		rect.SetDimensions (dimensions);
	}

	protected Vector2 GetTextDim(TextMeshPro tmp)
	{
		Vector2 ret = new Vector2(tmp.preferredWidth, tmp.preferredHeight);
		Vector2 scale2D = new Vector2 (tmp.rectTransform.lossyScale.x, tmp.rectTransform.lossyScale.y);
		ret.Scale (scale2D);
		return ret;
	}

	public void SetText(string text) {
		textPro.SetText (text);
	}
}
