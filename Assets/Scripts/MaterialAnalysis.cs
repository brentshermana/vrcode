using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAnalysis : MonoBehaviour {
    public int dim; // how wide and how tall the extracted square should be
    // Base: The bottom leftmost pixel of the square to be extracted
    public int xBase;
    public int yBase;
    public MeshRenderer magnifiedRenderer;

	// Use this for initialization
	void Start () {
        Material mat = gameObject.GetComponent<MeshRenderer>().materials[0];
        Texture2D tex = (Texture2D)mat.mainTexture;

        Debug.Log("Original texture is " + tex.width + "x" + tex.height);

        Texture2D newtex = new Texture2D(dim, dim); // width, height

        Color32[] oldColors = tex.GetPixels32();
        Color32[] newColors = new Color32[dim*dim];

        int i = 0;
        
        for (int y = yBase; y < yBase + dim; y++)
        {
            for (int x = xBase; x < xBase + dim; x++)
            {
                newColors[i] = oldColors[x + y*tex.width];
                i++;
            }
        }
        
        newtex.SetPixels32(newColors);
        newtex.Apply();

        // set other object's texture
        magnifiedRenderer.materials[0].mainTexture = newtex;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
