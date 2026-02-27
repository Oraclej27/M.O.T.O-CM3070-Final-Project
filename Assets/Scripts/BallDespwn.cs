//using UnityEngine;

//public class BallDespawn : MonoBehaviour
//{
//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Pipe"))
//        {
//            Destroy(gameObject);
//        }
//    }
//}
using UnityEngine;

public class BallDespawn : MonoBehaviour
{
    // This script is now optional - the logic moved to BallController
    // Keep only if you need additional despawn triggers

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            // Optional: Additional effects when ball enters tube
            Debug.Log($"Ball entered {gameObject.tag} tube");
        }
    }
}