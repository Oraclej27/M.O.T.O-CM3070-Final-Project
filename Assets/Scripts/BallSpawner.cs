// =============================================
// Script: BallSpawner.cs
// Purpose: Spawns balls and detects win/lose conditions through tube tags.
//
// Communicates with:
//   - BallController: Sets spawner reference so ball can inform despawn.
//   - WinStateUI: Fires OnWinCondition event when ball enters win tube.
//
// Usage: Attached to an empty GameObject at the spawn tube exit.
// =============================================
using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public float launchForce = 6f;
    public float respawnDelay = 3f;

    [Header("Tubes")]
    public Transform spawnTube;
    public string winTubeTag = "WinTube";
    public string loseTubeTag = "LoseTube";

    [Header("Level Complete")]
    public string nextLevelName;   

    public event System.Action OnWinCondition;

    private GameObject currentBall;
    private bool isGameComplete = false;
    private Coroutine respawnCoroutine;

    void Start()
    {
        SpawnBall();
    }

    void Update()
    {
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
            isGameComplete = true;
            currentBall = null;
            Debug.Log(" YOU WIN! Level Complete!");

            // fired event 
            OnWinCondition?.Invoke();
        }
        else if (tubeTag == loseTubeTag)
        {
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