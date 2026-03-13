// =============================================
// Script: MagnetizableObject.cs
// Purpose: Marks an object as affected by magnets. Has a public method to a apply magnetic force to its Rigidbody.
//
// Communicates with:
//   - MagnetController: Receives force through ApplyMagneticForce().
//
// Usage: Attached to objects that should be pulled by magnets. (ball)
// =============================================
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

