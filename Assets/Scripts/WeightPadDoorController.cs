// =============================================
// Script: WeightPadDoorController.cs
// Purpose: Listens to weight pad events and controls the door animation and sounds.
//
// Communicates with:
//   - WeightPad: Subscribes to OnWeightMet / OnWeightLost events.
//
// Usage: Attached to the door GameObject
// =============================================
using UnityEngine;

public class WeightPadDoorController : MonoBehaviour
{
    [Header("Door")]
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";
    public string doorCloseTrigger = "Close";

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    private void Start()
    {
        WeightPad pad = FindFirstObjectByType<WeightPad>();
        if (pad != null)
        {
            pad.OnWeightMet += OpenDoor;
            pad.OnWeightLost += CloseDoor;
        }
    }

    private void OnDestroy()
    {
        WeightPad pad = FindFirstObjectByType<WeightPad>();
        if (pad != null)
        {
            pad.OnWeightMet -= OpenDoor;
            pad.OnWeightLost -= CloseDoor;
        }
    }

    private void OpenDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);
        PlaySound(doorOpenSound);
    }

    private void CloseDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorCloseTrigger);
        PlaySound(doorCloseSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}