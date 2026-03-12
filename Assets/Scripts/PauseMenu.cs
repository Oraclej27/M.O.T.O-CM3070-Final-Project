using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel; 
    public GameObject controlsPanel;
    public GameObject howToPlayPanel; 

    [Header("First Selected Buttons")]
    public Button firstPauseButton; 
    public Button firstControlsButton;
    public Button firstHowToButton;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI References")]
    public GameObject tutorialCanvas; 
    public GameObject pauseButton;

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
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (pauseButton != null)
        {
            Button btn = pauseButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(PauseGame);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlayClickSound);
        }
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
                if (currentPanel == controlsPanel || currentPanel == howToPlayPanel)
                {
                    HideSubPanels();
                }
                else
                {
                    ResumeGame();
                }
            }
        }

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

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(false);

        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
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
        Time.timeScale = 1f; 

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(true);

        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        PlayClickSound();

        if (SoundController.Instance != null)
            SoundController.Instance.ResumeMusic();
    }

    public void ShowControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        currentPanel = controlsPanel;

        if (firstControlsButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstControlsButton.gameObject);

        PlaySwitchPanelSound();
    }

    public void ShowHowToPlay()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        currentPanel = howToPlayPanel;

        if (firstHowToButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstHowToButton.gameObject);

        PlaySwitchPanelSound();
    }

    public void HideSubPanels()
    {
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        pausePanel.SetActive(true);
        currentPanel = pausePanel;

        if (firstPauseButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);

        PlaySwitchPanelSound();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        PlayClickSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        PlayClickSound();
        SceneManager.LoadScene(mainMenuSceneName);
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
        if (audioSource != null && switchPanelSound != null)
            audioSource.PlayOneShot(switchPanelSound);
    }
}
