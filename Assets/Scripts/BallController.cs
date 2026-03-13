// =============================================
// Script: BallController.cs
// Purpose: Maintains constant speed of the ball, adds slight randomness on wall bounces. 
//
// Communicates with:
//   - BallSpawner: Calls OnBallDespawned() when ball enters a tube (WinTube/LoseTube/Pipe).
//
// Usage: Attached to the ball prefab.
// =============================================
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Movement")]
    public float constantSpeed = 6f; 
    public float speedRecoveryRate = 10f; 

    [Header("References")]
    public BallSpawner spawner;

    private Rigidbody rb;
    private Vector3 lastDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            lastDirection = rb.linearVelocity.normalized;
            rb.linearVelocity = lastDirection * constantSpeed;
        }
        else if (lastDirection != Vector3.zero)
        {
            rb.linearVelocity = lastDirection * constantSpeed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 randomDir = Random.insideUnitSphere * 0.1f;
            rb.linearVelocity = (rb.linearVelocity.normalized + randomDir).normalized * constantSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pipe") || other.CompareTag("WinTube") || other.CompareTag("LoseTube"))
        {
            string tubeTag = other.tag;

            if (spawner != null)
            {
                spawner.OnBallDespawned(tubeTag);
            }

            Destroy(gameObject);
        }
    }
}
