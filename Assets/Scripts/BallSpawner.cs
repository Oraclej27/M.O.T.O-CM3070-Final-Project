//using UnityEngine;

//public class BallSpawner : MonoBehaviour
//{
//    public GameObject ballPrefab;
//    public float launchForce = 6f;

//    private GameObject currentBall;

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Q))
//        {
//            SpawnBall();
//        }
//    }

//    void SpawnBall()
//    {
//        if (currentBall != null)
//            Destroy(currentBall);

//        currentBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);

//        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
//        rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
//    }
//}

using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public float launchForce = 6f;
    public float respawnDelay = 3f;

    [Header("Tubes")]
    public Transform spawnTube; // The tube where ball spawns
    public string winTubeTag = "WinTube";
    public string loseTubeTag = "LoseTube";

    [Header("Level Complete")]
    public GameObject levelCompleteUI; // Assign your win screen
    public string nextLevelName; // Name of next scene

    private GameObject currentBall;
    private bool isGameComplete = false;
    private Coroutine respawnCoroutine;

    void Start()
    {
        // Spawn first ball immediately
        SpawnBall();
    }

    void Update()
    {
        // Manual spawn for testing (optional)
        if (Input.GetKeyDown(KeyCode.Q) && !isGameComplete)
        {
            if (currentBall != null)
                Destroy(currentBall);
            SpawnBall();
        }
    }

    public void SpawnBall()
    {
        if (isGameComplete) return;

        if (currentBall != null)
            Destroy(currentBall);

        currentBall = Instantiate(ballPrefab, spawnTube.position, Quaternion.identity);

        BallController ballController = currentBall.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.spawner = this;
        }

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
        rb.AddForce(spawnTube.forward * launchForce, ForceMode.Impulse);
    }

    public void OnBallDespawned(string tubeTag)
    {
        if (tubeTag == winTubeTag)
        {
            // Player wins!
            isGameComplete = true;
            currentBall = null;
            Debug.Log("YOU WIN! Level Complete!");

            if (levelCompleteUI != null)
                levelCompleteUI.SetActive(true);

            // Load next level after delay (optional)
            // StartCoroutine(LoadNextLevel());
        }
        else if (tubeTag == loseTubeTag)
        {
            // Normal despawn - respawn after delay
            if (!isGameComplete)
            {
                if (respawnCoroutine != null)
                    StopCoroutine(respawnCoroutine);
                respawnCoroutine = StartCoroutine(RespawnAfterDelay());
            }
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        currentBall = null;
        yield return new WaitForSeconds(respawnDelay);

        if (!isGameComplete)
        {
            SpawnBall();
        }
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
    }
}