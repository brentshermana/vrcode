using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrcode.vr.keyboard
{
    public class ProceduralKeyboard : MonoBehaviour {

        public GameObject KeyPrefab;

        public float KeyDim; // cubed dimension of each key
        public float Travel; // how much is each key allowed to move
        public float KeySpacing; // spacing between each key
        public float Tiering; // reduction in height per row

        // rows go from top to bottom
        public float[] RowOffsets; // NUMBER OF KEYS each row should be shifted to the right

        // the keyboard"s keys
        public string[][] Characters = new string[][]
        {
            new string[]{"#", "=", ".", ","},
            new string[]{"+", "-", "*", "/", "%"},
            new string[]{"'", "\"", "[", "]", "{", "}", "(", ")"},
            new string[]{"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"},
            new string[]{ "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" },
            new string[]{ "a", "s", "d", "f", "g", "h", "j", "k", "l"},
            new string[]{"z", "x", "c", "v", "b", "n", "m"},
            new string[]{ "space", "bsp", "return"}
        };

        // mapping of nonletter keys to their respective keycodes:
        private Dictionary<string, KeyCode> keyCodes = new Dictionary<string, KeyCode>
        {
            {"space", KeyCode.Space},
            {"bsp", KeyCode.Backspace},
            {"return", KeyCode.Return}
        };
        // mapping of keys to nonstandard widths:
        private Dictionary<string, float> keyWidths;
        // mapping of nonletter keys to their respective ascii ints:
        private Dictionary<string, int> keyInts = new Dictionary<string, int>
        {
            {"space", 32}, //http://www.theasciicode.com.ar/ascii-printable-characters/space-ascii-code-32.html
            {"bsp", 8},
            {"return", 13}
        };
    
        // Use this for initialization
        void Start () {
            keyWidths = new Dictionary<string, float>
            {
                {"space", KeyDim*5 + KeySpacing*4},
                {"bsp", KeyDim*3 + KeySpacing*2},
                {"space", KeyDim*5 + KeySpacing*4}
            };
            
            float increment = KeyDim + KeySpacing;

            Vector3 scale = Vector3.one * KeyDim;

            float z = 0f;
            float y = 0f;
            for (int row = 0; row < Characters.Length; row++, z -= increment, y -= Tiering)
            {
                float x = RowOffsets[row] * KeyDim;
                for (int col = 0; col < Characters[row].Length; col++, x += KeySpacing)
                {
                    GameObject newKey = Instantiate(KeyPrefab);
                
                    newKey.GetComponent<KeyUtils>().SetTravel(Travel);

                    // set the key's value and texture
                    string KeyFaceStr = Characters[row][col];
                    if (KeyFaceStr.Length == 1)
                        newKey.GetComponent<KeyUtils>().KeyChar = KeyFaceStr[0];
                    else
                        newKey.GetComponent<KeyUtils>().SetSpecialKeyInfo(KeyFaceStr, keyCodes[KeyFaceStr], keyInts[KeyFaceStr]);

                    // set the key's size
                    float width;
                    if (KeyFaceStr.Length == 1)
                    {
                        newKey.GetComponent<KeyUtils>().SetSize(KeyDim); // size
                        width = KeyDim;
                    }
                    else
                    {
                        newKey.GetComponent<KeyUtils>().SetSize(keyWidths[KeyFaceStr], KeyDim, KeyDim);
                        width = keyWidths[KeyFaceStr];
                    }



                    Quaternion originalGlobalRotation = newKey.transform.rotation;

                    newKey.transform.parent = transform; // parent
                    newKey.transform.localPosition = Vector3.right * (x + .5f*width) + z * Vector3.forward + Vector3.up * y; // position
                    newKey.transform.localRotation = originalGlobalRotation; // rotation

                    x += width;
                }
            }
        }
	
        // Update is called once per frame
        void Update () {
		
        }
    }
}