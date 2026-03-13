// =============================================
// Script: CameraAnimationEvents.cs
// Purpose: Bridge for animation events on the drop camera.
//
// Communicates with:
//   - BoxContainer: Invokes OnCameraAnimationComplete() when the camera animation ends.
//
// Usage: Attached to the drop camera. 
// =============================================
using UnityEngine;

public class CameraAnimationEvents : MonoBehaviour
{
    public BoxContainer boxContainer; 

    public void OnCameraAnimationComplete()
    {
        if (boxContainer != null)
            boxContainer.OnCameraAnimationComplete();
    }
}