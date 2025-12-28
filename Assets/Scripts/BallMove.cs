using UnityEngine;

public class BallDespawn : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pipe"))
        {
            Destroy(gameObject);
        }
    }
}
