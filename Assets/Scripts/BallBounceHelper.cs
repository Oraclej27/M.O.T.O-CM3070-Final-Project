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