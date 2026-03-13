// Not Being used.  For testing only 
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 offset;
    public GameObject player;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    void Update()
    {
        transform.position = player.transform.position + offset;
    }
}
