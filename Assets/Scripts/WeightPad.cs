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

    [Header("Colliders")]
    public Collider solidCollider;  
    public Collider detectionTrigger; 

    private float currentWeight = 0f;
    private bool isDoorOpen = false;
    private List<GameObject> objectsOnPad = new List<GameObject>(); 

    void Start()
    {
        if (solidCollider != null)
            solidCollider.isTrigger = false;

        if (detectionTrigger != null)
            detectionTrigger.isTrigger = true;

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

        HashSet<GameObject> countedObjects = new HashSet<GameObject>();

        foreach (GameObject obj in objectsOnPad)
        {
            if (obj == null) continue;
            if (countedObjects.Contains(obj)) continue;

            GameObject rootObject = FindRootObjectOnPad(obj);

            if (rootObject != null && !countedObjects.Contains(rootObject))
            {
                Block block = rootObject.GetComponent<Block>();
                RobotController robot = rootObject.GetComponent<RobotController>();

                if (block != null)
                {
                    totalWeight += blockWeight;
                    countedObjects.Add(rootObject);
                }
                else if (robot != null)
                {
                    totalWeight += robotWeight;
                    countedObjects.Add(rootObject);
                }
            }
        }

        currentWeight = totalWeight;
        UpdateWeightDisplay();

        bool weightMet = currentWeight >= requiredWeight;

        if (padRenderer != null)
        {
            padRenderer.material.color = weightMet ? activeColor : inactiveColor;
        }

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

    GameObject FindRootObjectOnPad(GameObject obj)
    {
        RaycastHit hit;
        Vector3 rayStart = obj.transform.position;

        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            rayStart.y = objCollider.bounds.min.y + 0.05f;
        }

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 3f))
        {
            if (hit.collider == solidCollider)
                return obj;

            Block hitBlock = hit.collider.GetComponent<Block>();
            if (hitBlock != null)
            {
                return FindRootObjectOnPad(hitBlock.gameObject);
            }
        }

        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == detectionTrigger.gameObject) return;

        if (other == solidCollider) return;

        if (!objectsOnPad.Contains(other.gameObject))
        {
            objectsOnPad.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == detectionTrigger.gameObject) return;

        if (other == solidCollider) return;

        if (objectsOnPad.Contains(other.gameObject))
        {
            objectsOnPad.Remove(other.gameObject);
        }
    }

    void OpenDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        PlaySound(doorOpenSound);
        isDoorOpen = true;
    }

    void CloseDoor()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorCloseTrigger);

        PlaySound(doorCloseSound);
        isDoorOpen = false;
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
        if (solidCollider != null)
        {
            Gizmos.color = Color.yellow;
            BoxCollider box = solidCollider as BoxCollider;
            if (box != null)
            {
                Gizmos.matrix = solidCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }

        if (detectionTrigger != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            BoxCollider box = detectionTrigger as BoxCollider;
            if (box != null)
            {
                Gizmos.matrix = detectionTrigger.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
}