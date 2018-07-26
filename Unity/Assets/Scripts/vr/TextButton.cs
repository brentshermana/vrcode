using System;
using UnityEngine;
using vrcode.vr.keyboard;

namespace vrcode.vr
{
    public class TextButton : ButtonUtils
    {
        [SerializeField] public float letter_dimension;
        [SerializeField] public float travel;
        [SerializeField] public float width_factor; // text looks weird when each letter is one unit wide and high, so we need a ratio
        [SerializeField] public string button_face;
        public Action OnPressAction;

        public void OnPress()
        {
            Debug.Log("OnPress " + button_face);
            OnPressAction.Invoke();
        }

        public void Start()
        {
            if (button_face != null && button_face != "")
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            base.SetTravel(travel);

            base.SetSize( button_face.Length * width_factor * letter_dimension, letter_dimension, letter_dimension);
            
            // load the textures for each character
            Texture2D[] charTextures = new Texture2D[button_face.Length];
            for (int i = 0; i < charTextures.Length; i++)
            {
                charTextures[i] = LetterTextureLoader.LoadLetter(
                    "Textures\\Letters\\",
                    button_face[i]
                ) as Texture2D;
            }
            // stitch together horizontally
            base.SetTexture(TextureStitch(charTextures));
        }
        
        // TODO: this should be in some other class/library
        public Texture TextureStitch(Texture2D[] textures) // in order from left to right
        {
            int wSum = textures[0].width;
            int h = textures[0].height;
            for (int i = 1; i < textures.Length; i++)
            {
                if (textures[i] == null)
                {
                    Debug.LogError("texture for character at index " + i + " is null");
                }
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
    }
}