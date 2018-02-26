using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrcode.vr.keyboard;

namespace vrcode
{
    public class TextureTest : MonoBehaviour {
        public char a;
        public char b;

        public Texture letterTextureStitch(Texture2D at, Texture2D bt) {

            Texture2D ct = new Texture2D(at.width+bt.width, at.height);
            if (at.height != bt.height)
            {
                Debug.Log("Error: Character textures don't have same height!");
            }

            Color32[] cPixels = new Color32[ct.width * ct.height];
            Color32[] aPixels = at.GetPixels32();
            Color32[] bPixels = bt.GetPixels32();

            int ai = 0, bi = 0, ci = 0;
            for (int y = 0; y < ct.height; y++) // row major order
            {
                for (int x = 0; x < at.width; ai++,ci++,x++) // copy at's row
                {
                    cPixels[ci] = aPixels[ai];
                }
                for (int x = 0; x <bt.width; bi++,ci++,x++)
                {
                    cPixels[ci] = bPixels[bi];
                }
            }

            ct.SetPixels32(cPixels);
            ct.Apply();

            return ct;
        }

        // Use this for initialization
        void Start () {
            Texture2D at = LetterTextureLoader.LoadLetter(
                "Textures\\Letters\\",
                a
            ) as Texture2D;
            Texture2D bt = LetterTextureLoader.LoadLetter(
                "Textures\\Letters\\",
                b
            ) as Texture2D;

            Texture ct = letterTextureStitch(at, bt);

            GetComponent<Renderer>().materials[0].mainTexture = ct;
        }
	
        // Update is called once per frame
        void Update () {
		
        }
    }
}