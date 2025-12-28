using UnityEngine;

public class MagnetizableObject : MonoBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyMagneticForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Force);
    }
}

