using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public Text tutorialText;
    public Button skipButton;

    [Header("Settings")]
    public float defaultDisplayTime = 4f;
    public KeyCode skipKey = KeyCode.Tab;

    [Header("Tutorial Steps")]
    public bool showMovementTutorial = true;
    public bool showPickupTutorial = true;
    public bool showStateTutorial = true;
    public bool showMagnetTutorial = true;

    private Coroutine currentMessage;
    private bool tutorialActive = true;

    void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(skipKey))
            SkipTutorial();
    }

    public void ShowMessage(string message, float duration, KeyCode keyToPress = KeyCode.None)
    {
        if (!tutorialActive) return;

        if (keyToPress != KeyCode.None)
            message += $"\nPress {keyToPress} to continue";

        if (currentMessage != null)
            StopCoroutine(currentMessage);

        currentMessage = StartCoroutine(DisplayMessage(message, duration));
    }

    IEnumerator DisplayMessage(string message, float duration)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = message;

        yield return new WaitForSeconds(duration);

        tutorialPanel.SetActive(false);
        currentMessage = null;
    }

    public void SkipTutorial()
    {
        tutorialActive = false;
        tutorialPanel.SetActive(false);

        if (currentMessage != null)
            StopCoroutine(currentMessage);
    }
}