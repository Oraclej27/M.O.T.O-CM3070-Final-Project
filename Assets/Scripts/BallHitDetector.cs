using UnityEngine;

public class BallHitDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            RobotController robot =
                collision.collider.GetComponent<RobotController>();

            if (robot != null)
            {
                Debug.Log("Ball hit robot (physics-based)");
                robot.RegisterBallHit(transform.position);
            }
        }
    }
}
