using UnityEngine;

using Leap.Unity;
using Leap;

public class VRUI_InputModule : MonoBehaviour
{
    [SerializeField]
    LeapProvider leapProvider;

    private Frame LatestLeapFrame;
        
    void Start()
    {
        leapProvider.OnUpdateFrame += UpdateLeapFrame;
    }
    
    private void UpdateLeapFrame(Frame f)
    {
        // we copy because frame objects are mutable
        LatestLeapFrame = new Frame().CopyFrom(f);
    }

    void Update()
    {
        
    }
}