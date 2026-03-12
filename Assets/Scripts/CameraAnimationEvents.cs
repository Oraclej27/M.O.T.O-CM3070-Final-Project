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