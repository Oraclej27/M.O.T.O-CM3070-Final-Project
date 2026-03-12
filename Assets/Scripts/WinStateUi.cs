using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WinStateUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel; // The win screen panel

    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;

    [Header("Scene Management")]
    public string currentLevel; // Set in Inspector: "Level1" or "Level2"
    public string nextLevelName; // "Level2" for Level1, "MainMenu" for Level2
    public bool isLastLevel = false; // Check if this is the final level

    [Header("UI References")]
    public GameObject tutorialCanvas; // Drag your tutorial canvas here

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip winSound; // Special sound when winning

    private GameObject lastSelected;

    void Start()
    {
        // Ensure win panel is hidden at start
        if (winPanel != null)
            winPanel.SetActive(false);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Set up button text based on level
        SetupButtonLabels();

        // Add hover sounds to all buttons
        AddHoverSoundsToAllButtons();
    }

    void SetupButtonLabels()
    {
        // Change button text based on whether it's the last level
        if (isLastLevel)
        {
            // For Level 2 completion
            if (nextLevelButton != null)
            {
                Text buttonText = nextLevelButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "Main Menu";
            }
        }
        else
        {
            // For Level 1 completion
            if (nextLevelButton != null)
            {
                Text buttonText = nextLevelButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "Next Level";
            }
        }

        // Restart button always says "Restart"
        if (restartButton != null)
        {
            Text restartText = restartButton.GetComponentInChildren<Text>();
            if (restartText != null)
                restartText.text = "Restart Level";
        }
    }

    void Update()
    {
        // Play hover sound when selection changes (keyboard/controller)
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if (selected != null && selected != lastSelected)
            {
                if (selected.GetComponent<Button>() != null)
                {
                    PlayHoverSound();
                    lastSelected = selected;
                }
            }
        }
    }

    void AddHoverSoundsToAllButtons()
    {
        // Find all buttons in this panel
        Button[] buttons = winPanel.GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            AddHoverSound(btn);
        }
    }

    void AddHoverSound(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // Clear existing triggers
        trigger.triggers.Clear();

        // Mouse hover
        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(hoverEnter);

        // Keyboard selection
        EventTrigger.Entry selectEvent = new EventTrigger.Entry();
        selectEvent.eventID = EventTriggerType.Select;
        selectEvent.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(selectEvent);

        // Click event (sound only - action handled separately)
        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((data) => { PlayClickSound(); });
        trigger.triggers.Add(click);
    }

    // Call this from BallSpawner when ball enters win tube
    public void ShowWinScreen()
    {
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);
        // Pause the game
        Time.timeScale = 0f;

        // Show win panel
        winPanel.SetActive(true);

        // Set first button selected
        if (nextLevelButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(nextLevelButton.gameObject);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Play win sound
        if (winSound != null && audioSource != null)
            audioSource.PlayOneShot(winSound);
        else
            PlaySwitchPanelSound();

        if (SoundController.Instance != null)
            SoundController.Instance.PauseMusic(); // or StopMusic()
    }

    // Button methods
    public void OnNextLevel()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Resume time before loading

        if (isLastLevel)
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(nextLevelName);
    }

    public void OnRestartLevel()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Resume time before loading
        SceneManager.LoadScene(currentLevel);
    }

    // Sound methods
    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void PlaySwitchPanelSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}