// =============================================
// Script: BoxContainer.cs
// Purpose: Controls a container that drops blocks when opened. Manages lid physics, camera switch, and camera animation.
//
// Communicates with:
//   - Lever: TriggerBoxOpen() is called from Lever to start the sequence.
//   - CameraAnimationEvents: OnCameraAnimationComplete() is called when camera animation finishes to switch back to main camera.
//
// Usage: Attached to the container root GameObject. Uses lid Rigidbody, hinge joint, and camera references.
// =============================================
using UnityEngine;
using System.Collections;

public class BoxContainer : MonoBehaviour
{
    [Header("Lid Settings")]
    public Rigidbody lidRigidbody;
    public HingeJoint lidHinge;

    [Header("Camera Settings")]
    public Camera dropCamera;
    public Camera mainCamera;
    public Animator cameraAnimator;
    public string cameraAnimationTrigger = "DropSequence";

    private bool isOpened = false;

    void Start()
    {
        if (lidRigidbody != null)
            lidRigidbody.isKinematic = true;

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        if (dropCamera != null)
            dropCamera.gameObject.SetActive(false);
    }

    public void OpenContainer()
    {
        if (isOpened) return;
        isOpened = true;

        StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence()
    {
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);
        if (dropCamera != null)
            dropCamera.gameObject.SetActive(true);

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger(cameraAnimationTrigger);
        }

        if (lidRigidbody != null)
        {
            lidRigidbody.isKinematic = false;
        }

        yield break;
    }

    public void OnCameraAnimationComplete()
    {
        if (dropCamera != null)
            dropCamera.gameObject.SetActive(false);
        
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);
    }
}