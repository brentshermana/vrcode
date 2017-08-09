using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (MeshFilter))] 
[RequireComponent (typeof (MeshRenderer))]
public class ProceduralRect : MonoBehaviour {

	private ProceduralRectInfo rectInfo;

	private TextMesh textMesh;

	// Use this for initialization
	void Start () {
		rectInfo = new ProceduralRectInfo ();
		rectInfo.Enforce (gameObject);
		textMesh = transform.GetChild(0).gameObject.GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update () {
		float newX = GetTextWidth (textMesh);
		if (newX > .1f) {
			Vector3 newDim = new Vector3 (newX, 1f, 1f);
			rectInfo.Dimensions = newDim;
			rectInfo.Enforce (gameObject);
		}
	}

	private float GetTextWidth(TextMesh mesh)
	{
		float width = 0;
		foreach (char symbol in mesh.text)
		{
			CharacterInfo info;
			if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
			{
				width += info.advance;
			}
		}
		return width * mesh.characterSize * 0.1f;
	}
}
