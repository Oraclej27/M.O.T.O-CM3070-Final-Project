// =============================================
// Script: MagnetController.cs
// Purpose: Toggles a magnet on/off with the R key. Applies force to all MagnetizableObject within the range.
// Communicates with:
//   - MagnetizableObject: Finds all instances and applies force.
//   - SoundController: Plays magnet toggle sound via static instance.
//
// Usage: Attached to a magnet GameObject. 
// =============================================
using UnityEngine;

public class MagnetController : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] float strength = 50f;
    [SerializeField] float maxRange = 20f;
    [SerializeField] float minDistance = 1.5f;
    [SerializeField] AnimationCurve falloffCurve;

    [Header("Visuals")]
    [SerializeField] Renderer magnetRenderer;
    [SerializeField] Color offColor = Color.gray;
    [SerializeField] Color onColor = Color.red;

    private SoundController soundController;

    bool magnetActive = false;

    private void Awake()
    {
       soundController = FindFirstObjectByType<SoundController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleMagnet();
        }
    }

    void FixedUpdate()
    {
        if (!magnetActive) return;
        MagnetizableObject[] objects =
     FindObjectsByType<MagnetizableObject>(FindObjectsSortMode.None);

        foreach (MagnetizableObject obj in objects)
        {
            Vector3 direction = transform.position - obj.transform.position;
            float distance = direction.magnitude;

            if (distance > maxRange) continue;
 
            float t = Mathf.InverseLerp(maxRange, minDistance, distance);
            float forceAmount = strength * falloffCurve.Evaluate(t);

            obj.ApplyMagneticForce(direction.normalized * forceAmount);
        }
    }

    void ToggleMagnet()
    {
        magnetActive = !magnetActive;
        magnetRenderer.material.color =
            magnetActive ? onColor : offColor;

        if (SoundController.Instance != null)
            SoundController.Instance.PlayMagnetToggleSound();
    }
}

