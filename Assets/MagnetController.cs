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

    bool magnetActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleMagnet();
        }
    }

    void FixedUpdate()
    {
        // to only apply the force if magniet is active
        if (!magnetActive) return;
        // look for all subscribed objects "magnitized" objects 
        MagnetizableObject[] objects =
     FindObjectsByType<MagnetizableObject>(FindObjectsSortMode.None);

        foreach (MagnetizableObject obj in objects)
        {
            Vector3 direction = transform.position - obj.transform.position;
            float distance = direction.magnitude;

            if (distance > maxRange) continue;
            // normalize distance into 0-1 range 
            float t = Mathf.InverseLerp(maxRange, minDistance, distance);
            // calculate force using falloff 
            float forceAmount = strength * falloffCurve.Evaluate(t);

            obj.ApplyMagneticForce(direction.normalized * forceAmount);
        }
    }

    void ToggleMagnet()
    {
        magnetActive = !magnetActive;
        magnetRenderer.material.color =
            magnetActive ? onColor : offColor;
    }
}

