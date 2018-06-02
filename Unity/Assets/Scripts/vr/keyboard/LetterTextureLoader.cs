using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrcode.vr.keyboard
{
    public class LetterTextureLoader : MonoBehaviour {

        //don't forget to cast after using this method!
        public static Object LoadLetter(string resource_subdir, char c)
        {
            return Resources.Load(resource_subdir + LetterResourceMapping.resource_fname(c));
        }
    }
}