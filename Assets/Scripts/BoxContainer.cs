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
        // Lid starts locked
        if (lidRigidbody != null)
            lidRigidbody.isKinematic = true;

        // Start with main camera
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);
        if (dropCamera != null)
            dropCamera.gameObject.SetActive(false);
    }

    // Called by lever's onLeverPulled event
    public void OpenContainer()
    {
        if (isOpened) return;
        isOpened = true;

        Debug.Log("Opening container!");
        StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence()
    {
        // 1. Switch to drop camera
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);
        if (dropCamera != null)
            dropCamera.gameObject.SetActive(true);

        // 2. Play camera animation
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger(cameraAnimationTrigger);
        }

        // 3. Open the lid (enable gravity)
        if (lidRigidbody != null)
        {
            lidRigidbody.isKinematic = false;
        }

        // Don't wait here - the animation event will handle camera switch
        yield break;
    }

    //float GetCameraAnimationLength()
    //{
    //    if (cameraAnimator != null && cameraAnimator.runtimeAnimatorController != null)
    //    {
    //        // Get the animation clip length
    //        AnimationClip[] clips = cameraAnimator.runtimeAnimatorController.animationClips;
    //        foreach (AnimationClip clip in clips)
    //        {
    //            if (clip.name.Contains("Drop"))
    //                return clip.length;
    //        }
    //    }
    //    return 3f; // Default fallback
    //}

    public void OnCameraAnimationComplete()
    {
        Debug.Log("Camera animation complete - switching back");

        // Return to main camera
        if (dropCamera != null)
            dropCamera.gameObject.SetActive(false);
        Debug.Log(" Drop camera disabled");
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);
        Debug.Log("Main camera enabled");
    }
}