using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrcode.input.keyboard;

namespace vrcode.vr.keyboard
{
    public class KeyUtils : ButtonUtils {
        /* normal keys have a particular char value */
        private bool keyCharSet;
        protected char keyChar;
        public char KeyChar {
            get { return keyChar; }
            set {
                SetKey(value);
            }
        }
        private void SetKey(char key)
        {
            keyChar = key;
            keyCharSet = true;
            specialCharSet = false; // just in case

            Texture letterTex = LetterTextureLoader.LoadLetter(
                "Textures\\Letters\\",
                KeyChar
            ) as Texture;

            base.SetTexture(letterTex);
        }

        /*
     * Some special keys have a special text on their face,
     * and don't correspond to a character (eg shift),
     * so we need to set their face and keyevent
     */
        private bool specialCharSet;
        protected KeyCode keyCode;
        protected int asciiCode; // for help finding these -> http://www.theasciicode.com.ar/ascii-printable-characters/space-ascii-code-32.html
        public void SetSpecialKeyInfo(string keyFace, KeyCode code, int ascii_code)
        {
            asciiCode = ascii_code;
            keyCode = code;

            specialCharSet = true;
            keyCharSet = false; // just in case that was set previously

            // load the textures for each character
            Texture2D[] charTextures = new Texture2D[keyFace.Length];
            for (int i = 0; i < charTextures.Length; i++)
            {
                charTextures[i] = LetterTextureLoader.LoadLetter(
                    "Textures\\Letters\\",
                    keyFace[i]
                ) as Texture2D;
            }
            // stitch together horizontally
            base.SetTexture(TextureStitch(charTextures));
        }

        public Texture TextureStitch(Texture2D[] textures) // in order from left to right
        {
            int wSum = textures[0].width;
            int h = textures[0].height;
            for (int i = 1; i < textures.Length; i++)
            {
                if (textures[i].height != h)
                    Debug.LogError("Textures don't have same height");
                wSum += textures[i].width;
            }

            Texture2D newTexture = new Texture2D(wSum, h);

            Color32[] newPixels = new Color32[newTexture.width * newTexture.height];
            Color32[][] oldPixels = new Color32[textures.Length][]; // [texture_i][pixel]
            int[] oldTexIndices = new int[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                oldTexIndices[i] = 0; //just in case
                oldPixels[i] = textures[i].GetPixels32();
            }

            int ci = 0;
            for (int y = 0; y < newTexture.height; y++)
            {
                // for each texture...
                for (int tex = 0; tex < oldPixels.Length; tex++)
                {
                    // ... copy over its y'th row
                    for (int x = 0; x < textures[tex].width; x++, ci++, oldTexIndices[tex]++)
                    {
                        newPixels[ci] = oldPixels[tex][oldTexIndices[tex]];
                    }
                }
            }

            newTexture.SetPixels32(newPixels);
            newTexture.Apply();

            return newTexture;
        }
    
        public void PressKey()
        {
            if (keyCharSet)
            {
                KeyboardInputManager.AddCharPress(KeyChar);
            }
            else if (specialCharSet)
            {
                // here we assume that holding down keys is not necessary behavior
                KeyboardInputManager.AddCharPress((char)asciiCode);
//                KeyboardInputManager.AddEvent(new Event() { type = EventType.KeyDown, keyCode = this.keyCode });
//                KeyboardInputManager.AddEvent(new Event() { type = EventType.KeyDown, keyCode = KeyCode.None, character = (char)asciiCode}); // don't know why we need the middle one
//                KeyboardInputManager.AddEvent(new Event() { type = EventType.KeyUp, keyCode = this.keyCode });
            }
            else
            {
                Debug.LogError("Key Values not set!");
            }
        }
    }
}