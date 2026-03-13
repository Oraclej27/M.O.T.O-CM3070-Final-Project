// =============================================
// Script: BallHitDetector.cs
// Purpose: Detects collision between the ball and the robot, and notifies the RobotController.
//
// Communicates with:
//   - RobotController: Calls RegisterBallHit() when collision with player occurs.
//
// Usage: Attached to the ball prefab. 
// =============================================
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
                Debug.Log("Ball hit robot");
                robot.RegisterBallHit();
            }
        }
    }
}