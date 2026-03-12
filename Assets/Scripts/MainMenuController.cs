using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject controlsPanel;
    public GameObject howToPlayPanel;

    public Button firstMainButton;
    public Button firstControlsButton;
    public Button firstHowToButton;

    [Header("Scenes")]
    public string level1SceneName = "Level1";
    public string tutorialSceneName = "TutorialLevel"; // Add this

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip switchPanelSound;

    private GameObject currentPanel;

    void Start()
    {
        ShowMainMenu();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        AddHoverSoundsToAllButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPanel != mainPanel)
            {
                ShowMainMenu();
            }
        }
    }

    // No Update method needed! Unity handles keyboard automatically

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

        // Mouse hover
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


    public void ShowMainMenu()
    {
        mainPanel.SetActive(true);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        currentPanel = mainPanel;
        EventSystem.current.SetSelectedGameObject(firstMainButton.gameObject);
        PlaySwitchPanelSound();
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        currentPanel = controlsPanel;
        EventSystem.current.SetSelectedGameObject(firstControlsButton.gameObject);
        PlaySwitchPanelSound();
    }

    public void ShowHowToPlay()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        currentPanel = howToPlayPanel;
        EventSystem.current.SetSelectedGameObject(firstHowToButton.gameObject);
        PlaySwitchPanelSound();
    }

    public void PlayTutorial() 
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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