using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Movement")]
    public float constantSpeed = 6f; // The speed ball will always try to maintain
    public float speedRecoveryRate = 10f; // How quickly it recovers speed

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
        // Always maintain constant speed
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            // Store direction
            lastDirection = rb.linearVelocity.normalized;

            // Maintain speed
            rb.linearVelocity = lastDirection * constantSpeed;
        }
        else if (lastDirection != Vector3.zero)
        {
            // If stopped, give it a push in last direction
            rb.linearVelocity = lastDirection * constantSpeed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Optional: Add slight randomness on bounce to prevent loops
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Small random deflection for natural movement
            Vector3 randomDir = Random.insideUnitSphere * 0.1f;
            rb.linearVelocity = (rb.linearVelocity.normalized + randomDir).normalized * constantSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pipe") || other.CompareTag("WinTube") || other.CompareTag("LoseTube"))
        {
            string tubeTag = other.tag;

            // Notify spawner
            if (spawner != null)
            {
                spawner.OnBallDespawned(tubeTag);
            }

            // Destroy this ball
            Destroy(gameObject);
        }
    }
}
