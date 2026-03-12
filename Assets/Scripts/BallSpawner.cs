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

    [Header("Win State UI")]
    public WinStateUI winStateUI;

    [Header("Level Complete")]
    public string nextLevelName;

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

            if (winStateUI != null)
                winStateUI.ShowWinScreen();
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