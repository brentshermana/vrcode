using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyUtils : ButtonUtils {
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

        Texture letterTex = LetterResourceLoader.LoadLetter(
                        "Textures\\Letters\\",
                        KeyChar
                    ) as Texture;

        base.SetTexture(letterTex);
    }

    public void PressKey()
    {
        KeyboardInputManager.AddCharPress(KeyChar);
    }
}
