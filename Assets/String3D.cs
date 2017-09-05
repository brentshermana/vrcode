using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class String3D : MonoBehaviour {

	private static Dictionary<char, GameObject> characters;
	private static HashSet<char> unavailable_chars;



	private List<char> chars;
	private List<LetterScope> letters;
	private float x_offset;

	public bool capture_keyboard;
	public string resource_subdir; //System.IO.Path.PathSeparator + "Characters" + System.IO.Path.PathSeparator + "_";

	// Use this for initialization
	void Awake () {
		x_offset = 0;
		characters = new Dictionary<char, GameObject> ();
		unavailable_chars = new HashSet<char> ();
		chars = new List<char> ();
		letters = new List<LetterScope> ();

		Debug.Log (resource_subdir);
	}
	
	// Update is called once per frame
	void Update () {
		if (capture_keyboard) {
			foreach (char c in Input.inputString) {
				//check for backspace:
				if (c == '\b') {
					Backspace ();
					continue;
				}
				else if (unavailable_chars.Contains (c)) {
					Debug.Log (c.ToString() + " Is known to be invalid");
					continue;
				}
				else {
					//attempt to load prefab in 'resource_subdir' by this name
					string path = resource_subdir + c.ToString();
					if (char.IsLetter (c) && char.IsLower (c)) { //because unity's naming within editor isn't case sensitive
						path += "l";
					}
					GameObject charobj = Resources.Load(path) as GameObject;

					if (charobj != null) {
						Debug.Log ("Loaded Resource : " + c.ToString());

						GameObject instance = Instantiate (charobj) as GameObject;
						instance.transform.rotation = transform.rotation;

						chars.Add (c);
						LetterScope letter = instance.GetComponent<LetterScope> ();

						Debug.Log ("String3D reads width " + letter.Width);
						x_offset += .5f * letter.Width;
						Vector3 center = transform.position + -transform.right * x_offset; //left because Unity's coordinate system is odd
						instance.transform.position = center;
						x_offset += .5f * letter.Width;

						Debug.Log ("Offset " + x_offset + " right: " + transform.right);

						letters.Add (letter);
					} else {
						Debug.Log ("Failed to Load Resource : " + c.ToString());
						unavailable_chars.Add (c);
					}
				}
			}
		}
	}

	void Backspace() {
		int index = chars.Count - 1;
		if (index < 0)
			return;
		x_offset -= letters [index].Width;
		Destroy (letters [index].gameObject);
		chars.RemoveAt (index);
		letters.RemoveAt (index);
	}
}
