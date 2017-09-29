/*

WARNING - BAD CODE! copied from String3D, then modified. Should reconcile later.

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class String3DPix : MonoBehaviour {

	public float r; // radius of circle

    public float spacing;

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


    public char[] Chars
    {
        get
        {
            return chars.ToArray();
        }
    }

    public float Width
    {
        get
        {
            return x_offset;
        }
    }

	// Use this for initialization
	void Awake () {
        Debug.Log(arc_position(0));
        Debug.Log(arc_position(r*Mathf.PI/2));
        Debug.Log(arc_position(r*Mathf.PI));


		x_offset = 0;

        special_character_names = new Dictionary<char, string>();
        special_character_names.Add('&', "ampersand");
        special_character_names.Add('@', "at");
        special_character_names.Add('*', "asterisk");
        special_character_names.Add('\\', "backslash");
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

        special_character_names.Add('?', "question");

				special_character_names.Add('>', "greater_than");
				special_character_names.Add('#', "hashtag");
				special_character_names.Add('{', "left_curly_bracket");
				special_character_names.Add('(', "left_parenthesis");
        special_character_names.Add('[', "left_square_bracket");
        special_character_names.Add('.', "period");
        special_character_names.Add(',', "comma");
				special_character_names.Add('<', "less_than");
				special_character_names.Add('-', "minus");
				special_character_names.Add('9', "nine");
                special_character_names.Add('5', "five");
				special_character_names.Add('1', "one");
				special_character_names.Add('%', "percent");
				special_character_names.Add('+', "plus");
				special_character_names.Add('}', "right_curly_bracket");
        special_character_names.Add(')', "right_parenthesis");

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

	private Vector3 getCenter() {
		return transform.position + (transform.forward * r);
	}


	public Vector3 arc_position(float x_offset) {
		float angle_r = x_offset / r;
		Vector3 center = getCenter ();
		return center + (-Mathf.Cos(angle_r) * transform.forward * r) + (-Mathf.Sin(angle_r) * transform.right * r);
	}

    public void SetContent(string content)
    {
        //flush current content
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
        letters.Clear();
        x_offset = 0f;
        //add new content
        foreach (char c in content)
        {
            AddChar(c);
        }
    }

    private static float space_length = .02f * 6;
    private static int spaces_per_tab = 4;
    private bool validWhitespace(char c)
    {
        return c == ' ' || c == '\t';
    }
    private float whitespaceWidth(char c)
    {
        if (!validWhitespace(c))
            throw new System.ArgumentException("'" + c + "' is not valid whitespace!");

        float space_width = space_length;
        if (c == '\t')
        {
            space_width *= spaces_per_tab;
        }
        return space_width;
    }

    public bool AddChar(char c)
    {
        if (unavailable_chars.Contains(c))
        {
            Debug.Log(c.ToString() + " Is known to be invalid");
            return false;
        }
        else if (validWhitespace(c)) // special branch for characters that are valid whitespace
        {
            float space_width = whitespaceWidth(c);
            chars.Add(c);
            x_offset += spacing;
            x_offset += space_width;
            letters.Add(null);
            return true;
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

                GameObject instance = Instantiate(charobj, transform.position, transform.rotation, transform) as GameObject;

                
                

                //instance.transform.SetParent(transform);

                chars.Add(c);
                LetterScope letter = instance.GetComponent<LetterScope>();

                x_offset += spacing;
                x_offset += .5f * letter.Width;
                //instance.transform.position += -transform.right*x_offset;
				instance.transform.position = arc_position(x_offset);
				instance.transform.LookAt(getCenter(), transform.up);
				//just a hacky solution to reverse rotation because all the letters are backwards
				instance.transform.RotateAround(instance.transform.position, transform.up, 180f);
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

    // untested so far
    void RemoveRange(int from, int to) // inclusive bounds
    {
        int n = to - from + 1;
        if (n < 1)
        {
            throw new System.ArgumentException("to < from");
        }
        else if (to >= chars.Count)
        {
            throw new System.ArgumentException(to + " is out of bounds!");
        }

        
        float cumulative_width = n * spacing;
        for (int i = from; i <= to; i++)
        {
            //determine collective width, including spacing
            if (validWhitespace(chars[i]))
                cumulative_width += whitespaceWidth(chars[i]);
            else
            {
                cumulative_width += letters[i].Width;
                Destroy(letters[i].gameObject);
            }
        }

        letters.RemoveRange(from, n);
        chars.RemoveRange(from, n);

        //shift all chars right of the removed range to the left
        Vector3 shift = transform.right * cumulative_width;
        for (int i = from; i < letters.Count; i++)
        {
            if (letters[i] != null)
            {
                letters[i].gameObject.transform.position += shift;
            }
        }
        
    }

	void Backspace() {
		int index = chars.Count - 1;
		if (index < 0)
			return;
        x_offset -= spacing;
        if (validWhitespace(chars[index]))
        {
            x_offset -= whitespaceWidth(chars[index]);
        }
        else
        {
            x_offset -= letters[index].Width;
            Destroy(letters[index].gameObject);
        }
		chars.RemoveAt (index);
		letters.RemoveAt (index);
	}
}
