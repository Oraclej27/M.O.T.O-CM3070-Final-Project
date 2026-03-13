// =============================================
// Script: WinStateUI.cs
// Purpose: Displays win panel with buttons, and handles scene loading.
//
// Communicates with:
//   - BallSpawner: Subscribes to OnWinCondition event to show win screen.
//   - SoundController: Calls PauseMusic when win screen appears.
//
// Usage: Attached to the Canvas GameObject containing the win panel.
// =============================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WinStateUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;

    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;

    [Header("Scene Management")]
    public string currentLevel;
    public string nextLevelName;
    public bool isLastLevel = false;

    [Header("UI References")]
    public GameObject tutorialCanvas;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip winSound;

    private GameObject lastSelected;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetupButtonLabels();
        AddHoverSoundsToAllButtons();

        // subscribe 
        BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
        if (spawner != null)
        {
            spawner.OnWinCondition += ShowWinScreen;
        }
    }

    void OnDestroy()
    {
        //  to help avoid memory leaks
        BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
        if (spawner != null)
        {
            spawner.OnWinCondition -= ShowWinScreen;
        }
    }

    void SetupButtonLabels()
    {
        if (isLastLevel)
        {
            if (nextLevelButton != null)
            {
                Text buttonText = nextLevelButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "Main Menu";
            }
        }
        else
        {
            if (nextLevelButton != null)
            {
                Text buttonText = nextLevelButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "Next Level";
            }
        }

        if (restartButton != null)
        {
            Text restartText = restartButton.GetComponentInChildren<Text>();
            if (restartText != null)
                restartText.text = "Restart Level";
        }
    }

    void Update()
    {
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

        trigger.triggers.Clear();

        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(hoverEnter);

        EventTrigger.Entry selectEvent = new EventTrigger.Entry();
        selectEvent.eventID = EventTriggerType.Select;
        selectEvent.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(selectEvent);

        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((data) => { PlayClickSound(); });
        trigger.triggers.Add(click);
    }

    public void ShowWinScreen()
    {
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        Time.timeScale = 0f;
        winPanel.SetActive(true);

        if (nextLevelButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(nextLevelButton.gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winSound != null && audioSource != null)
            audioSource.PlayOneShot(winSound);
        else
            PlaySwitchPanelSound();

        if (SoundController.Instance != null)
            SoundController.Instance.PauseMusic();
    }

    public void OnNextLevel()
    {
        PlayClickSound();
        Time.timeScale = 1f;

        if (isLastLevel)
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(nextLevelName);
    }

    public void OnRestartLevel()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentLevel);
    }

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