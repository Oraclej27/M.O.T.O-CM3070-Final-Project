// =============================================
// Script: WeightPad.cs
// Purpose: Detects objects on the pad, calculates total weight, and fires events when weight amount is met or lost.
//
// Communicates with:
//   - WeightPadDoorController: Fires OnWeightMet / OnWeightLost events.
//   - Block / RobotController: Reads their weight.
//
// Usage: Attached to the weight pad GameObject with solid and trigger colliders.
// =============================================
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeightPad : MonoBehaviour
{
    [Header("Weight Settings")]
    public float requiredWeight = 3f;
    public float blockWeight = 1f;
    public float robotWeight = 3f;

    [Header("Visual Feedback")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;
    public Renderer padRenderer;

    [Header("UI Display")]
    public TextMeshProUGUI weightDisplay;
    public string displayPrefix = "Weight: ";

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip weightMetSound;
    public AudioClip weightLostSound;

    [Header("Colliders")]
    public Collider solidCollider;
    public Collider detectionTrigger;

    public event System.Action<float> OnWeightChanged; 
    public event System.Action OnWeightMet;
    public event System.Action OnWeightLost;

    private float currentWeight = 0f;
    private bool wasWeightMet = false;
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
        bool weightMetNow = false;

        HashSet<GameObject> countedObjects = new HashSet<GameObject>();

        foreach (GameObject obj in objectsOnPad)
        {
            if (obj == null) continue;
            if (countedObjects.Contains(obj)) continue;

            GameObject bottom = FindRootObjectOnPad(obj);
            if (bottom != null) 
            {
                Block block = obj.GetComponent<Block>();
                RobotController robot = obj.GetComponent<RobotController>();
                if (block != null)
                {
                    totalWeight += blockWeight;
                    countedObjects.Add(obj);
                }
                else if (robot != null)
                {
                    totalWeight += robotWeight;
                    countedObjects.Add(obj);
                }
            }
        }

        weightMetNow = totalWeight >= requiredWeight;

        if (Mathf.Abs(totalWeight - currentWeight) > 0.001f)
        {
            currentWeight = totalWeight;
            UpdateWeightDisplay();
            OnWeightChanged?.Invoke(currentWeight);
        }

        if (weightMetNow && !wasWeightMet)
        {
            wasWeightMet = true;
            OnWeightMet?.Invoke();
            if (padRenderer != null)
                padRenderer.material.color = activeColor;
            PlaySound(weightMetSound);
        }
        else if (!weightMetNow && wasWeightMet)
        {
            wasWeightMet = false;
            OnWeightLost?.Invoke();
            if (padRenderer != null)
                padRenderer.material.color = inactiveColor;
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
                return FindRootObjectOnPad(hitBlock.gameObject);
        }
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == detectionTrigger.gameObject || other == solidCollider)
            return;

        if (!objectsOnPad.Contains(other.gameObject))
            objectsOnPad.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == detectionTrigger.gameObject || other == solidCollider)
            return;

        if (objectsOnPad.Contains(other.gameObject))
            objectsOnPad.Remove(other.gameObject);
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
            audioSource.PlayOneShot(clip);
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