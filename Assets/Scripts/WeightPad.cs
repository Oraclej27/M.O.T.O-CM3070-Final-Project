//using UnityEngine;
//using System.Collections.Generic;
//using TMPro; // Add this for TextMeshPro

//public class WeightPad : MonoBehaviour
//{
//    [Header("Weight Settings")]
//    public float requiredWeight = 3f;
//    public float blockWeight = 1f;
//    public float robotWeight = 3f;

//    [Header("Door")]
//    public Animator doorAnimator;
//    public string doorOpenTrigger = "Open";
//    public string doorCloseTrigger = "Close";

//    [Header("Visual Feedback")]
//    public Color activeColor = Color.green;
//    public Color inactiveColor = Color.red;
//    public Renderer padRenderer;

//    [Header("UI Display")]
//    public TextMeshProUGUI weightDisplay; // Drag your UI Text here
//    public string displayPrefix = "Weight: ";

//    [Header("Sound Effects")]
//    public AudioSource audioSource; // Add an AudioSource component
//    public AudioClip doorOpenSound;
//    public AudioClip doorCloseSound;
//    public AudioClip weightMetSound;
//    public AudioClip weightLostSound;

//    [Header("Particle Effects (Optional)")]
//    public ParticleSystem activeEffect;

//    private float currentWeight = 0f;
//    private bool isDoorOpen = false;
//    private List<GameObject> objectsOnPad = new List<GameObject>();

//    void Start()
//    {
//        if (padRenderer != null)
//            padRenderer.material.color = inactiveColor;

//        // Update UI at start
//        UpdateWeightDisplay();

//        // Make sure we have an AudioSource
//        if (audioSource == null)
//            audioSource = GetComponent<AudioSource>();
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        Debug.Log($"Something entered: {other.gameObject.name}");
//        if (objectsOnPad.Contains(other.gameObject)) return;

//        objectsOnPad.Add(other.gameObject);

//        Block block = other.GetComponent<Block>();
//        RobotController robot = other.GetComponent<RobotController>();

//        Debug.Log($"Has Block: {block != null}, Has Robot: {robot != null}");

//        float previousWeight = currentWeight;
//        bool wasWeightMet = currentWeight >= requiredWeight;

//        if (block != null)
//            currentWeight += blockWeight;
//        else if (robot != null)
//            currentWeight += robotWeight;
//        else
//            return;

//        Debug.Log($"Weight: {currentWeight}/{requiredWeight}");
//        UpdateWeightDisplay();

//        // Check if we just reached required weight
//        if (!wasWeightMet && currentWeight >= requiredWeight)
//        {
//            PlaySound(weightMetSound);
//            if (activeEffect != null) activeEffect.Play();
//        }

//        UpdatePadState();
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (!objectsOnPad.Contains(other.gameObject)) return;

//        objectsOnPad.Remove(other.gameObject);

//        Block block = other.GetComponent<Block>();
//        RobotController robot = other.GetComponent<RobotController>();

//        float previousWeight = currentWeight;
//        bool wasWeightMet = currentWeight >= requiredWeight;

//        if (block != null)
//            currentWeight -= blockWeight;
//        else if (robot != null)
//            currentWeight -= robotWeight;

//        Debug.Log($"Weight: {currentWeight}/{requiredWeight}");
//        UpdateWeightDisplay();

//        // Check if we just lost required weight
//        if (wasWeightMet && currentWeight < requiredWeight)
//        {
//            PlaySound(weightLostSound);
//            if (activeEffect != null) activeEffect.Stop();
//        }

//        UpdatePadState();
//    }

//    void UpdatePadState()
//    {
//        bool weightMet = currentWeight >= requiredWeight;

//        // Visual feedback
//        if (padRenderer != null)
//        {
//            padRenderer.material.color = weightMet ? activeColor : inactiveColor;
//        }

//        // Door control
//        if (weightMet && !isDoorOpen)
//        {
//            OpenDoor();
//        }
//        else if (!weightMet && isDoorOpen)
//        {
//            CloseDoor();
//        }
//    }

//    void OpenDoor()
//    {
//        if (doorAnimator != null)
//            doorAnimator.SetTrigger(doorOpenTrigger);

//        PlaySound(doorOpenSound);
//        isDoorOpen = true;
//        Debug.Log(" Door OPENED");
//    }

//    void CloseDoor()
//    {
//        if (doorAnimator != null)
//            doorAnimator.SetTrigger(doorCloseTrigger);

//        PlaySound(doorCloseSound);
//        isDoorOpen = false;
//        Debug.Log(" Door CLOSED");
//    }

//    void UpdateWeightDisplay()
//    {
//        if (weightDisplay != null)
//        {
//            weightDisplay.text = $"{displayPrefix}{currentWeight}/{requiredWeight}";

//            // Optional: Change text color based on weight status
//            if (currentWeight >= requiredWeight)
//                weightDisplay.color = Color.green;
//            else
//                weightDisplay.color = Color.white;
//        }
//    }

//    void PlaySound(AudioClip clip)
//    {
//        if (audioSource != null && clip != null)
//        {
//            audioSource.PlayOneShot(clip);
//        }
//    }

//    // Optional: Reset the pad (call this if you want to clear it)
//    public void ResetPad()
//    {
//        currentWeight = 0f;
//        objectsOnPad.Clear();
//        UpdateWeightDisplay();
//        UpdatePadState();

//        if (padRenderer != null)
//            padRenderer.material.color = inactiveColor;

//        if (activeEffect != null)
//            activeEffect.Stop();
//    }

//    void OnDrawGizmos()
//    {
//        // Visualize weight pad area
//        Gizmos.color = Color.yellow;
//        BoxCollider col = GetComponent<BoxCollider>();
//        if (col != null)
//        {
//            Gizmos.matrix = transform.localToWorldMatrix;
//            Gizmos.DrawWireCube(col.center, col.size);
//        }
//    }
//}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeightPad : MonoBehaviour
{
    [Header("Weight Settings")]
    public float requiredWeight = 3f;
    public float blockWeight = 1f;
    public float robotWeight = 3f;

    [Header("Door")]
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";
    public string doorCloseTrigger = "Close";

    [Header("Visual Feedback")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;
    public Renderer padRenderer;

    [Header("UI Display")]
    public TextMeshProUGUI weightDisplay;
    public string displayPrefix = "Weight: ";

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public AudioClip weightMetSound;
    public AudioClip weightLostSound;

    private float currentWeight = 0f;
    private bool isDoorOpen = false;
    private List<GameObject> objectsOnPad = new List<GameObject>(); // Renamed for clarity

    void Start()
    {
        if (padRenderer != null)
            padRenderer.material.color = inactiveColor;

        UpdateWeightDisplay();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        CalculateWeight();
    }

    void CalculateWeight()
    {
        float totalWeight = 0f;
        bool wasWeightMet = currentWeight >= requiredWeight;

        // Check each object that entered our trigger
        foreach (GameObject obj in objectsOnPad)
        {
            if (obj == null) continue;

            // Check if this object is actually resting ON the pad
            if (IsRestingOnPad(obj))
            {
                Block block = obj.GetComponent<Block>();
                RobotController robot = obj.GetComponent<RobotController>();

                if (block != null)
                    totalWeight += blockWeight;
                else if (robot != null)
                    totalWeight += robotWeight;
            }
        }

        currentWeight = totalWeight;
        UpdateWeightDisplay();

        // Check for state changes
        bool weightMet = currentWeight >= requiredWeight;

        // Visual feedback
        if (padRenderer != null)
        {
            padRenderer.material.color = weightMet ? activeColor : inactiveColor;
        }

        // Door control
        if (weightMet && !isDoorOpen)
        {
            OpenDoor();
            if (!wasWeightMet)
                PlaySound(weightMetSound);
        }
        else if (!weightMet && isDoorOpen)
        {
            CloseDoor();
            if (wasWeightMet)
                PlaySound(weightLostSound);
        }
    }

    bool IsRestingOnPad(GameObject obj)
    {
        // Raycast down from the object to see if it's touching the pad
        RaycastHit hit;
        Vector3 rayStart = obj.transform.position;

        // Slightly adjust ray start to be above the object's bottom
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            rayStart.y = objCollider.bounds.min.y + 0.1f;
        }

        // Cast ray straight down
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 0.5f))
        {
            // Check if we hit this weight pad
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER ENTER: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        if (!objectsOnPad.Contains(other.gameObject))
        {
            objectsOnPad.Add(other.gameObject);
            Debug.Log($" Added to objectsOnPad list. Count: {objectsOnPad.Count}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($" TRIGGER EXIT: {other.gameObject.name}");

        if (objectsOnPad.Contains(other.gameObject))
        {
            objectsOnPad.Remove(other.gameObject);
            Debug.Log($" Removed from objectsOnPad list. Count: {objectsOnPad.Count}");
        }
    }

    void OpenDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        PlaySound(doorOpenSound);
        isDoorOpen = true;
        Debug.Log(" Door OPENED");
    }

    void CloseDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorCloseTrigger);

        PlaySound(doorCloseSound);
        isDoorOpen = false;
        Debug.Log(" Door CLOSED");
    }

    void UpdateWeightDisplay()
    {
        if (weightDisplay != null)
        {
            weightDisplay.text = $"{displayPrefix}{currentWeight}/{requiredWeight}";
            weightDisplay.color = currentWeight >= requiredWeight ? Color.green : Color.white;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmos()
    {
        // Visualize weight pad area
        Gizmos.color = Color.yellow;
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}