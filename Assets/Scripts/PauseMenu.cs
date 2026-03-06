using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel; // Main pause panel
    public GameObject controlsPanel; // Controls sub-panel

    [Header("First Selected Buttons")]
    public Button firstPauseButton; // Usually Resume button
    public Button firstControlsButton; // Back button in controls panel

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip switchPanelSound;

    private bool isPaused = false;
    private GameObject currentPanel;
    private GameObject lastSelected;

    void Start()
    {
        // Ensure menus are hidden at start
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlayClickSound);
        }

        // Find all buttons and add hover sounds
        AddHoverSoundsToAllButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                // If we're in controls panel, go back to pause panel
                if (currentPanel == controlsPanel)
                {
                    HideControls();
                }
                else
                {
                    ResumeGame();
                }
            }
        }

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
        // Find all buttons in the scene
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
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

        // Mouse hover (PointerEnter event)
        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(hoverEnter);

        // Keyboard / controller selection
        EventTrigger.Entry selectEvent = new EventTrigger.Entry();
        selectEvent.eventID = EventTriggerType.Select;
        selectEvent.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(selectEvent);

        // Click event
        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((data) => { PlayClickSound(); });
        trigger.triggers.Add(click);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Debug.Log($"Game Paused - Time.timeScale: {Time.timeScale}"); // Should be 0

        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);
        currentPanel = pausePanel;

        if (firstPauseButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlaySwitchPanelSound();

        if (SoundController.Instance != null)
            SoundController.Instance.PauseMusic();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game time

        // Hide all panels
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        // Relock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayClickSound();

        if (SoundController.Instance != null)
            SoundController.Instance.ResumeMusic();
    }

    public void ShowControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
        currentPanel = controlsPanel;

        // Set first selected button in controls panel
        if (firstControlsButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstControlsButton.gameObject);

        PlaySwitchPanelSound();
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
        currentPanel = pausePanel;

        // Restore selection to main pause button
        if (firstPauseButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);

        PlaySwitchPanelSound();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Resume time before loading
        PlayClickSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Resume time before loading
        PlayClickSound();
        SceneManager.LoadScene(mainMenuSceneName);
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
        if (audioSource != null && switchPanelSound != null)
            audioSource.PlayOneShot(switchPanelSound);
    }
}
