using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;

public class LeapMotionGestures : MonoBehaviour {

    private Controller controller;

	// Use this for initialization
	void Start () {
        controller = new Controller();
        controller.FrameReady += MyOnFrame;
	}
	
	// Update is called once per frame
	void Update () {
	}

    static void MyOnFrame(object sender, FrameEventArgs args)
    {
        Frame frame = args.frame;
        foreach (Hand hand in frame.Hands)
            HandleHand(hand);
    }
    static void HandleHand(Hand hand)
    {
        
    }
}
