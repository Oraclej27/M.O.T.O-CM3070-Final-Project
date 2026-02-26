using UnityEngine;

public class CameraAnimationEvents : MonoBehaviour
{
    public BoxContainer boxContainer; // Drag your box here in Inspector

    public void OnCameraAnimationComplete()
    {
        if (boxContainer != null)
            boxContainer.OnCameraAnimationComplete();
    }
}