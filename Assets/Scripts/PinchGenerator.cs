using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity.Interaction;
using Leap.Unity;

public class PinchGenerator : MonoBehaviour {

    public GameObject[] prefabs;
    private int Prefab_I;

    public float maxInstantiateDist = .1f;
    

    //public HandModel leftModel;
    //public HandModel rightModel;

    //private PinchDetector leftDetector = new PinchDetector();
    //private PinchDetector rightDetector = new PinchDetector();

    private PinchDetector[] detectors = new PinchDetector[2];
    //private bool[] pinching = new bool[2];

    // state variables
    private bool generating = false;
    private GeneratableObject objToGenerate = null;

	// Use this for initialization
	void Start () {

        detectors = GetComponents<PinchDetector>();
        foreach (PinchDetector pd in detectors)
            pd.Activate();
	}
	
	// Update is called once per frame
	void Update () {
        bool status = PinchStatus();
        if (status && !generating && PinchDist() < maxInstantiateDist)
        {
            // begin generating
            generating = true;
            objToGenerate = Instantiate(prefabs[Prefab_I]).GetComponent<GeneratableObject>();
            UpdateBlock(objToGenerate);
        }
        else if (!status && generating)
        {
            UpdateBlock(objToGenerate);
            generating = false;
            objToGenerate.Generate();
        }
        else if (status && generating)
        {
            UpdateBlock(objToGenerate);
        }
	}

    float PinchDist()
    {
        return (detectors[0].Position - detectors[1].Position).magnitude;
    }

    public void SetPrefabI(int i)
    {
        Prefab_I = i;
    }

    void UpdateBlock(GeneratableObject obj)
    {
        Vector3 center = (detectors[0].Position + detectors[1].Position) / 2f;
        Vector3 axis = detectors[0].Position - detectors[1].Position;

        float size = axis.magnitude;
        obj.Resize(size);
        obj.AlignAxis(axis);
        obj.Reposition(center);
    }

    private bool PinchStatus()
    {
        for (int i = 0; i < detectors.Length; i++)
        {
            if (!detectors[i].IsPinching)
                return false;
        }
        return true;
    }
}
