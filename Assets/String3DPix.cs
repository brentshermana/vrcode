/*

WARNING - BAD CODE! copied from String3D, then modified. Should reconcile later.

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class String3DPix : MonoBehaviour {

    public Material material;

	private static Dictionary<char, GameObject> characters;
	private static HashSet<char> unavailable_chars;

    private static Dictionary<char, string> special_character_names;
		//TODO: mapping from char to prefab name should be a separate class/object?



	private List<char> chars;
	private List<LetterScope> letters;
	private float x_offset;

	public bool capture_keyboard;
	public string resource_subdir; //System.IO.Path.PathSeparator + "Characters" + System.IO.Path.PathSeparator + "_";


    private float miny;
    private float maxy;
    public float MinY
    {
        get { setMinY(); return miny; }
    }
    public float MaxY
    {
        get { setMaxY(); return maxy; }
    }
    private float minx;
    private float maxx;
    public float MinX
    {
        get { setMinX(); return minx; }
    }
    public float MaxX
    {
        get { setMaxX(); return maxx; }
    }

    public void setMaxX()
    {
        maxx = letters[letters.Count - 1].MaxX;
    }

    public void setMinX()
    {
        minx = letters[0].MinX;
    }

    public void setMinY()
    {
        float min = float.MaxValue;
        foreach (LetterScope letter in letters)
        {
            if (letter.MinY < min)
            {
                min = letter.MinY;
            }
        }
        miny = min;
    }

    public void setMaxY()
    {
        float max = float.MinValue;
        foreach (LetterScope letter in letters)
        {
            if (letter.MaxY > max)
            {
                max = letter.MaxY;
            }
        }
        maxy = max;
    }

	// Use this for initialization
	void Awake () {
		x_offset = 0;

        special_character_names = new Dictionary<char, string>();
        special_character_names.Add('&', "ampersand");
        special_character_names.Add('@', "at");
        special_character_names.Add('*', "asterisk");
        special_character_names.Add('\\', "back_slash");
        special_character_names.Add('`', "backtick");
				special_character_names.Add('^', "caret");
				special_character_names.Add(':', "colon");
				special_character_names.Add('$', "dollar");
				special_character_names.Add('"', "double_quote");
				special_character_names.Add('8', "eight");
				special_character_names.Add('=', "equals");
				special_character_names.Add('!', "exclaimation");
				special_character_names.Add('/', "forward_slash");
				special_character_names.Add('4', "four");

				special_character_names.Add('>', "greater_than");
				special_character_names.Add('#', "hashtag");
				special_character_names.Add('{', "left_curly_bracket");
				special_character_names.Add('(', "left_parenthesis");
				special_character_names.Add('[', "self_square_bracket");
				special_character_names.Add('<', "less_than");
				special_character_names.Add('-', "minus");
				special_character_names.Add('9', "nine");
				special_character_names.Add('1', "one");
				special_character_names.Add('%', "percent");
				special_character_names.Add('+', "plus");
				special_character_names.Add('}', "right_curly_bracket");

				special_character_names.Add(']', "right_square_bracket");
				special_character_names.Add(';', "semicolon");
				special_character_names.Add('7', "seven");
				special_character_names.Add('\'', "single_quote");
				special_character_names.Add('6', "six");
				special_character_names.Add('3', "three");
				special_character_names.Add('~', "tilda");
				special_character_names.Add('2', "two");
				special_character_names.Add('_', "underscore");

				special_character_names.Add('|', "vertical_bar");
				special_character_names.Add('0', "zero");
				//was just doing slash stuff

        characters = new Dictionary<char, GameObject> ();
		unavailable_chars = new HashSet<char> ();
		chars = new List<char> ();
		letters = new List<LetterScope> ();

		//Debug.Log (resource_subdir);
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
				else
                {
                    bool success = AddChar(c);
                    if (success)
                    {
                        //?
                    }
                }
			}
		}
	}

    public bool AddChar(char c)
    {
        if (unavailable_chars.Contains(c))
        {
            Debug.Log(c.ToString() + " Is known to be invalid");
            return false;
        }
        else
        {
            //attempt to load prefab in 'resource_subdir' by this name
            string path = resource_subdir;
            if (char.IsLetter(c))
            { //because unity's naming within editor isn't case sensitive
                path += c.ToString() + (char.IsLower(c) ? "_lower" : "_upper");
            }
            else if (special_character_names.ContainsKey(c))
            {
                //case where unity editor may disallow us from using the character itself as part of path
                path += special_character_names[c];
            }

            GameObject charobj = Resources.Load(path) as GameObject;

            if (charobj != null)
            {
                //Debug.Log("Loaded Resource : " + c.ToString());

                GameObject instance = Instantiate(charobj, transform.position, transform.rotation) as GameObject;
                instance.transform.rotation = transform.rotation;

                chars.Add(c);
                LetterScope letter = instance.GetComponent<LetterScope>();

                //Debug.Log("String3D reads width " + letter.Width);
                x_offset += .5f * letter.Width;
                //Vector3 center = transform.position + -transform.right * x_offset; //left because Unity's coordinate system is odd
                //instance.transform.position = center;
                instance.transform.position = instance.transform.position + -transform.right*x_offset;
                x_offset += .5f * letter.Width;

                //Debug.Log("Offset " + x_offset + " right: " + transform.right);

                if (this.material != null)
                {
                    letter.SetMaterial(this.material);
                }

                letters.Add(letter);
                return true;
            }
            else
            {
                Debug.Log("Failed to Load Resource : " + c.ToString());
                unavailable_chars.Add(c);
                return false;
            }
        }
    }


    public void SetMaterial(Material mat)
    {
        foreach (LetterScope letter in letters)
        {
            letter.SetMaterial(mat);
        }
        material = mat;
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
