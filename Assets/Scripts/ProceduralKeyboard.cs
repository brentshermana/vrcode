using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralKeyboard : MonoBehaviour {

    public GameObject KeyPrefab;

    public float KeyDim; // cubed dimension of each key
    public float Travel; // how much is each key allowed to move
    public float KeySpacing; // spacing between each key
    public float Tiering; // reduction in height per row

    // rows go from top to bottom
    public int[] RowLengths; // number of keys in each row
    public float[] RowOffsets; // distance each row should be shifted to the right

    public char[][] Characters = new char[][]
    {
        new char[]{ 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' },
        new char[]{ 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'},
        new char[]{'z', 'x', 'c', 'v', 'b', 'n', 'm'}
    };
    
	// Use this for initialization
	void Start () {
        float increment = KeyDim + KeySpacing;

        Vector3 scale = Vector3.one * KeyDim;

        float z = 0f;
        float y = 0f;
	    for (int row = 0; row < RowLengths.Length; row++, z -= increment, y -= Tiering)
        {
            float x = RowOffsets[row];
            for (int col = 0; col < RowLengths[row]; col++, x += increment)
            {
                GameObject newKey = Instantiate(KeyPrefab);

                newKey.GetComponent<KeyUtils>().SetSize(KeyDim); // size
                newKey.GetComponent<KeyUtils>().SetTravel(Travel);

                Quaternion originalGlobalRotation = newKey.transform.rotation;

                newKey.transform.parent = transform; // parent
                newKey.transform.localPosition = Vector3.right*x + z*Vector3.forward + Vector3.up*y; // position
                newKey.transform.localRotation = originalGlobalRotation; // rotation

                newKey.GetComponent<KeyUtils>().KeyChar = Characters[row][col]; // sets the value and the texture
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
