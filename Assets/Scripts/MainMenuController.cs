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

    [Header("First Level")]
    public string level1SceneName = "Level1";

    [Header("Button Navigation")]
    public Button[] mainMenuButtons; // Drag buttons in order

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip switchPanelSound;

    private int currentButtonIndex = 0;
    private GameObject currentPanel;
    private Button[] currentButtons;

    void Start()
    {
        // Show main panel, hide others
        ShowMainMenu();

        // Setup audio
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Add hover sounds to all buttons
        AddHoverSoundsToAllButtons();
    }

    void Update()
    {
        // Handle keyboard navigation
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        if (currentPanel == null) return;

        // Get current buttons based on active panel
        if (currentPanel == mainPanel)
            currentButtons = mainMenuButtons;
        else if (currentPanel == controlsPanel || currentPanel == howToPlayPanel)
        {
            // For sub-panels, just have Back button
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                PlayClickSound();
                ShowMainMenu();
            }
            return;
        }

        if (currentButtons == null || currentButtons.Length == 0) return;

        // Arrow key navigation
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentButtonIndex = (currentButtonIndex + 1) % currentButtons.Length;
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentButtonIndex--;
            if (currentButtonIndex < 0)
                currentButtonIndex = currentButtons.Length - 1;
            UpdateButtonSelection();
        }

        // Enter/Space to select
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentButtons[currentButtonIndex] != null)
            {
                PlayClickSound();
                currentButtons[currentButtonIndex].onClick.Invoke();
            }
        }
    }

    void UpdateButtonSelection()
    {
        // Remove selection from all buttons
        foreach (Button btn in currentButtons)
        {
            btn.OnDeselect(null);
        }

        // Select current button
        currentButtons[currentButtonIndex].Select();
        PlayHoverSound();
    }

    void AddHoverSoundsToAllButtons()
    {
        // Add to main menu buttons
        foreach (Button btn in mainMenuButtons)
        {
            AddHoverSound(btn);
        }

        // Find and add to back buttons in panels using new syntax
        Button[] backButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button btn in backButtons)
        {
            if (btn.name.Contains("Back") || btn.name.Contains("back"))
            {
                AddHoverSound(btn);
            }
        }
    }

    void AddHoverSound(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // Create hover enter event
        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(hoverEnter);

        // Create click event
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
        currentButtonIndex = 0;

        if (mainMenuButtons.Length > 0)
            mainMenuButtons[0].Select();

        PlaySwitchPanelSound();
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        currentPanel = controlsPanel;

        PlaySwitchPanelSound();
    }

    public void ShowHowToPlay()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        currentPanel = howToPlayPanel;

        PlaySwitchPanelSound();
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