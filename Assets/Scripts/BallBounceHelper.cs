// =============================================
// Script: BallBounceHelper.cs
// Purpose: Safety script that helps the ball keep moving by adding a small impulse if its velocity drops too low.
//
// Communicates with: None (works on its own Rigidbody).
//
// Usage: Attached to the ball prefab as an optional backup for constant speed.(Used for testing ball movement)
// =============================================
using UnityEngine;

public class BallBounceHelper : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb.linearVelocity.magnitude < 3f)
        {
            rb.AddForce(rb.linearVelocity.normalized * 5f, ForceMode.Impulse);
        }
    }
}