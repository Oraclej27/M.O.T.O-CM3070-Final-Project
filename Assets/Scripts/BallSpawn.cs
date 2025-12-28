using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public float launchForce = 6f;

    private GameObject currentBall;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        if (currentBall != null)
            Destroy(currentBall);

        currentBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
    }
}

