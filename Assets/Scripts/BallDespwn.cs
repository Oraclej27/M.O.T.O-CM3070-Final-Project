using UnityEngine;

public class BallDespawn : MonoBehaviour
{
    // only in case I need additional despawn triggers 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log($"Ball entered {gameObject.tag} tube");
        }
    }
}