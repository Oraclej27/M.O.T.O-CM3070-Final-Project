//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;

//public class MainMenuController : MonoBehaviour
//{
//    [Header("Panels")]
//    public GameObject mainPanel;
//    public GameObject controlsPanel;
//    public GameObject howToPlayPanel;

//    [Header("First Level")]
//    public string level1SceneName = "Level1";

//    [Header("Button Navigation")]
//    public Button[] mainMenuButtons; // 

//    [Header("Sounds")]
//    public AudioSource audioSource;
//    public AudioClip hoverSound;
//    public AudioClip clickSound;
//    public AudioClip switchPanelSound;

//    private int currentButtonIndex = 0;
//    private GameObject currentPanel;
//    private Button[] currentButtons;

//    void Start()
//    {
//        // Show main panel, hide others
//        ShowMainMenu();

//        // Setup audio
//        if (audioSource == null)
//            audioSource = gameObject.AddComponent<AudioSource>();

//        // Add hover sounds to all buttons
//        AddHoverSoundsToAllButtons();
//    }

//    //void Update()
//    //{
//    //    // Handle keyboard navigation
//    //    HandleKeyboardInput();
//    //}

//    //void HandleKeyboardInput()
//    //{
//    //    if (currentPanel == null) return;

//    //    // Get current buttons based on active panel
//    //    if (currentPanel == mainPanel)
//    //        currentButtons = mainMenuButtons;
//    //    else if (currentPanel == controlsPanel || currentPanel == howToPlayPanel)
//    //    {
//    //        // For sub-panels, just have Back button
//    //        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//    //        {
//    //            PlayClickSound();
//    //            ShowMainMenu();
//    //        }
//    //        return;
//    //    }

//    //    if (currentButtons == null || currentButtons.Length == 0) return;

//    //    // Arrow key navigation
//    //    if (Input.GetKeyDown(KeyCode.DownArrow))
//    //    {
//    //        currentButtonIndex = (currentButtonIndex + 1) % currentButtons.Length;
//    //        UpdateButtonSelection();
//    //    }
//    //    else if (Input.GetKeyDown(KeyCode.UpArrow))
//    //    {
//    //        currentButtonIndex--;
//    //        if (currentButtonIndex < 0)
//    //            currentButtonIndex = currentButtons.Length - 1;
//    //        UpdateButtonSelection();
//    //    }

//    //    // Enter/Space to select
//    //    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//    //    {
//    //        if (currentButtons[currentButtonIndex] != null)
//    //        {
//    //            PlayClickSound();
//    //            currentButtons[currentButtonIndex].onClick.Invoke();
//    //        }
//    //    }
//    //}

//    //void UpdateButtonSelection()
//    //{
//    //    // Remove selection from all buttons
//    //    foreach (Button btn in currentButtons)
//    //    {
//    //        btn.OnDeselect(null);
//    //    }

//    //    // Select current button
//    //    currentButtons[currentButtonIndex].Select();
//    //    PlayHoverSound();
//    //}

//    void AddHoverSoundsToAllButtons()
//    {
//        // Add to main menu buttons
//        foreach (Button btn in mainMenuButtons)
//        {
//            AddHoverSound(btn);
//        }

//        // Find and add to back buttons in panels using new syntax
//        Button[] backButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
//        foreach (Button btn in backButtons)
//        {
//            if (btn.name.Contains("Back") || btn.name.Contains("back"))
//            {
//                AddHoverSound(btn);
//            }
//        }
//    }

//    void AddHoverSound(Button button)
//    {
//        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
//        if (trigger == null)
//            trigger = button.gameObject.AddComponent<EventTrigger>();

//        // Create hover enter event
//        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
//        hoverEnter.eventID = EventTriggerType.PointerEnter;
//        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
//        trigger.triggers.Add(hoverEnter);

//        // Create click event
//        EventTrigger.Entry click = new EventTrigger.Entry();
//        click.eventID = EventTriggerType.PointerClick;
//        click.callback.AddListener((data) => { PlayClickSound(); });
//        trigger.triggers.Add(click);
//    }

//    public void ShowMainMenu()
//    {
//        mainPanel.SetActive(true);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(false);
//        currentPanel = mainPanel;
//        currentButtonIndex = 0;

//        if (mainMenuButtons.Length > 0)
//            mainMenuButtons[0].Select();

//        PlaySwitchPanelSound();
//    }

//    public void ShowControls()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(true);
//        howToPlayPanel.SetActive(false);
//        currentPanel = controlsPanel;

//        PlaySwitchPanelSound();
//    }

//    public void ShowHowToPlay()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(true);
//        currentPanel = howToPlayPanel;

//        PlaySwitchPanelSound();
//    }

//    public void PlayGame()
//    {
//        SceneManager.LoadScene(level1SceneName);
//    }

//    public void ExitGame()
//    {
//        PlayClickSound();
//        Application.Quit();
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
//    }

//    // Sound methods
//    public void PlayHoverSound()
//    {
//        if (audioSource != null && hoverSound != null)
//            audioSource.PlayOneShot(hoverSound);
//    }

//    public void PlayClickSound()
//    {
//        if (audioSource != null && clickSound != null)
//            audioSource.PlayOneShot(clickSound);
//    }

//    public void PlaySwitchPanelSound()
//    {
//        if (audioSource != null && switchPanelSound != null)
//            audioSource.PlayOneShot(switchPanelSound);
//    }
//}
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;

//public class MainMenuController : MonoBehaviour
//{
//    [Header("Panels")]
//    public GameObject mainPanel;
//    public GameObject controlsPanel;
//    public GameObject howToPlayPanel;

//    [Header("First Level")]
//    public string level1SceneName = "Level1";

//    [Header("Button Navigation")]
//    public Button[] mainMenuButtons;

//    [Header("Sounds")]
//    public AudioSource audioSource;
//    public AudioClip hoverSound;
//    public AudioClip clickSound;
//    public AudioClip switchPanelSound;

//    private int currentButtonIndex = 0;
//    private GameObject currentPanel;
//    private EventSystem eventSystem;

//    void Start()
//    {
//        ShowMainMenu();

//        if (audioSource == null)
//            audioSource = gameObject.AddComponent<AudioSource>();

//        AddHoverSoundsToAllButtons();

//        // EventSystem.current should always exist if you have one in the scene
//        eventSystem = EventSystem.current;

//        if (eventSystem == null)
//            Debug.LogWarning("No EventSystem found in scene! Please add one (UI  EventSystem)");
//    }

//    void Update()
//    {
//        HandleKeyboardInput();
//    }

//    void HandleKeyboardInput()
//    {
//        if (currentPanel == mainPanel)
//        {
//            // Arrow key navigation
//            if (Input.GetKeyDown(KeyCode.DownArrow))
//            {
//                currentButtonIndex = (currentButtonIndex + 1) % mainMenuButtons.Length;
//                // This tells Unity which button is selected - triggers visual state
//                eventSystem.SetSelectedGameObject(mainMenuButtons[currentButtonIndex].gameObject);
//                PlayHoverSound();
//            }
//            else if (Input.GetKeyDown(KeyCode.UpArrow))
//            {
//                currentButtonIndex--;
//                if (currentButtonIndex < 0)
//                    currentButtonIndex = mainMenuButtons.Length - 1;
//                eventSystem.SetSelectedGameObject(mainMenuButtons[currentButtonIndex].gameObject);
//                PlayHoverSound();
//            }

//            // Enter/Space to select
//            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//            {
//                if (mainMenuButtons[currentButtonIndex] != null)
//                {
//                    PlayClickSound();
//                    mainMenuButtons[currentButtonIndex].onClick.Invoke();
//                }
//            }
//        }
//        else if (currentPanel == controlsPanel || currentPanel == howToPlayPanel)
//        {
//            // Back button handling
//            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
//            {
//                PlayClickSound();
//                ShowMainMenu();
//            }
//        }
//    }

//    void AddHoverSoundsToAllButtons()
//    {
//        // Add to main menu buttons
//        foreach (Button btn in mainMenuButtons)
//        {
//            AddHoverSound(btn);
//        }

//        // Find and add to back buttons
//        Button[] backButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
//        foreach (Button btn in backButtons)
//        {
//            if (btn.name.Contains("Back") || btn.name.Contains("back"))
//            {
//                AddHoverSound(btn);
//            }
//        }
//    }

//    void AddHoverSound(Button button)
//    {
//        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
//        if (trigger == null)
//            trigger = button.gameObject.AddComponent<EventTrigger>();

//        // Hover enter event - also updates selection for keyboard/mouse sync
//        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
//        hoverEnter.eventID = EventTriggerType.PointerEnter;
//        hoverEnter.callback.AddListener((data) => {
//            // Update the current index when mouse hovers
//            for (int i = 0; i < mainMenuButtons.Length; i++)
//            {
//                if (mainMenuButtons[i] == button)
//                {
//                    currentButtonIndex = i;
//                    break;
//                }
//            }
//            PlayHoverSound();
//        });
//        trigger.triggers.Add(hoverEnter);

//        // Click event
//        EventTrigger.Entry click = new EventTrigger.Entry();
//        click.eventID = EventTriggerType.PointerClick;
//        click.callback.AddListener((data) => { PlayClickSound(); });
//        trigger.triggers.Add(click);
//    }

//    public void ShowMainMenu()
//    {
//        mainPanel.SetActive(true);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(false);
//        currentPanel = mainPanel;
//        currentButtonIndex = 0;

//        // Set the first button as selected
//        if (mainMenuButtons.Length > 0 && eventSystem != null)
//        {
//            eventSystem.SetSelectedGameObject(mainMenuButtons[0].gameObject);
//        }

//        PlaySwitchPanelSound();
//    }

//    public void ShowControls()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(true);
//        howToPlayPanel.SetActive(false);
//        currentPanel = controlsPanel;

//        // Clear selection when switching panels
//        if (eventSystem != null)
//            eventSystem.SetSelectedGameObject(null);

//        PlaySwitchPanelSound();
//    }

//    public void ShowHowToPlay()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(true);
//        currentPanel = howToPlayPanel;

//        // Clear selection when switching panels
//        if (eventSystem != null)
//            eventSystem.SetSelectedGameObject(null);

//        PlaySwitchPanelSound();
//    }

//    public void PlayGame()
//    {
//        SceneManager.LoadScene(level1SceneName);
//    }

//    public void ExitGame()
//    {
//        PlayClickSound();
//        Application.Quit();
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
//    }

//    public void PlayHoverSound()
//    {
//        if (audioSource != null && hoverSound != null)
//            audioSource.PlayOneShot(hoverSound);
//    }

//    public void PlayClickSound()
//    {
//        if (audioSource != null && clickSound != null)
//            audioSource.PlayOneShot(clickSound);
//    }

//    public void PlaySwitchPanelSound()
//    {
//        if (audioSource != null && switchPanelSound != null)
//            audioSource.PlayOneShot(switchPanelSound);
//    }
//}
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;

//public class MainMenuController : MonoBehaviour
//{
//    [Header("Panels")]
//    public GameObject mainPanel;
//    public GameObject controlsPanel;
//    public GameObject howToPlayPanel;

//    [Header("First Level")]
//    public string level1SceneName = "Level1";

//    [Header("Button Navigation")]
//    public Button[] mainMenuButtons;

//    [Header("Back Buttons")]
//    public Button controlsBackButton; // Drag the back button from controls panel
//    public Button howToPlayBackButton; // Drag the back button from how to play panel

//    [Header("Sounds")]
//    public AudioSource audioSource;
//    public AudioClip hoverSound;
//    public AudioClip clickSound;
//    public AudioClip switchPanelSound;

//    private int currentButtonIndex = 0;
//    private GameObject currentPanel;
//    private EventSystem eventSystem;

//    void Start()
//    {
//        ShowMainMenu();

//        if (audioSource == null)
//            audioSource = gameObject.AddComponent<AudioSource>();

//        AddHoverSoundsToAllButtons();

//        eventSystem = EventSystem.current;
//        if (eventSystem == null)
//            Debug.LogWarning("No EventSystem found in scene! Please add one (UI  EventSystem)");
//    }

//    void Update()
//    {
//        HandleKeyboardInput();
//    }

//    void HandleKeyboardInput()
//    {
//        if (currentPanel == mainPanel)
//        {
//            // Arrow key navigation for main menu
//            if (Input.GetKeyDown(KeyCode.DownArrow))
//            {
//                currentButtonIndex = (currentButtonIndex + 1) % mainMenuButtons.Length;
//                eventSystem.SetSelectedGameObject(mainMenuButtons[currentButtonIndex].gameObject);
//                PlayHoverSound();
//            }
//            else if (Input.GetKeyDown(KeyCode.UpArrow))
//            {
//                currentButtonIndex--;
//                if (currentButtonIndex < 0)
//                    currentButtonIndex = mainMenuButtons.Length - 1;
//                eventSystem.SetSelectedGameObject(mainMenuButtons[currentButtonIndex].gameObject);
//                PlayHoverSound();
//            }

//            // Enter/Space to select
//            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
//            {
//                if (mainMenuButtons[currentButtonIndex] != null)
//                {
//                    PlayClickSound();
//                    mainMenuButtons[currentButtonIndex].onClick.Invoke();
//                }
//            }
//        }
//        else if (currentPanel == controlsPanel)
//        {
//            // Handle back button in controls panel
//            if (controlsBackButton != null)
//            {
//                // Single button navigation - no arrows needed
//                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
//                {
//                    PlayClickSound();
//                    ShowMainMenu();
//                }
//            }
//        }
//        else if (currentPanel == howToPlayPanel)
//        {
//            // Handle back button in how to play panel
//            if (howToPlayBackButton != null)
//            {
//                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
//                {
//                    PlayClickSound();
//                    ShowMainMenu();
//                }
//            }
//        }
//    }

//    void AddHoverSoundsToAllButtons()
//    {
//        // Add to main menu buttons
//        foreach (Button btn in mainMenuButtons)
//        {
//            AddHoverSound(btn);
//        }

//        // Add to back buttons if assigned
//        if (controlsBackButton != null)
//            AddHoverSound(controlsBackButton);

//        if (howToPlayBackButton != null)
//            AddHoverSound(howToPlayBackButton);

//        // Find and add to any other back buttons (fallback)
//        Button[] backButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
//        foreach (Button btn in backButtons)
//        {
//            if (btn.name.Contains("Back") || btn.name.Contains("back"))
//            {
//                if (btn != controlsBackButton && btn != howToPlayBackButton)
//                    AddHoverSound(btn);
//            }
//        }
//    }

//    void AddHoverSound(Button button)
//    {
//        if (button == null) return;

//        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
//        if (trigger == null)
//            trigger = button.gameObject.AddComponent<EventTrigger>();

//        // Hover enter event
//        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
//        hoverEnter.eventID = EventTriggerType.PointerEnter;
//        hoverEnter.callback.AddListener((data) => {
//            // Update the current index when mouse hovers over main menu buttons
//            for (int i = 0; i < mainMenuButtons.Length; i++)
//            {
//                if (mainMenuButtons[i] == button)
//                {
//                    currentButtonIndex = i;
//                    break;
//                }
//            }
//            PlayHoverSound();
//        });
//        trigger.triggers.Add(hoverEnter);

//        // Click event
//        EventTrigger.Entry click = new EventTrigger.Entry();
//        click.eventID = EventTriggerType.PointerClick;
//        click.callback.AddListener((data) => { PlayClickSound(); });
//        trigger.triggers.Add(click);
//    }

//    public void ShowMainMenu()
//    {
//        mainPanel.SetActive(true);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(false);
//        currentPanel = mainPanel;
//        currentButtonIndex = 0;

//        // Set the first button as selected
//        if (mainMenuButtons.Length > 0 && eventSystem != null)
//        {
//            eventSystem.SetSelectedGameObject(mainMenuButtons[0].gameObject);
//        }

//        PlaySwitchPanelSound();
//    }

//    public void ShowControls()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(true);
//        howToPlayPanel.SetActive(false);
//        currentPanel = controlsPanel;

//        // Select the back button when entering controls panel
//        if (controlsBackButton != null && eventSystem != null)
//        {
//            eventSystem.SetSelectedGameObject(controlsBackButton.gameObject);
//        }
//        else
//        {
//            // Fallback: clear selection
//            if (eventSystem != null)
//                eventSystem.SetSelectedGameObject(null);
//        }

//        PlaySwitchPanelSound();
//    }

//    public void ShowHowToPlay()
//    {
//        mainPanel.SetActive(false);
//        controlsPanel.SetActive(false);
//        howToPlayPanel.SetActive(true);
//        currentPanel = howToPlayPanel;

//        // Select the back button when entering how to play panel
//        if (howToPlayBackButton != null && eventSystem != null)
//        {
//            eventSystem.SetSelectedGameObject(howToPlayBackButton.gameObject);
//        }
//        else
//        {
//            // Fallback: clear selection
//            if (eventSystem != null)
//                eventSystem.SetSelectedGameObject(null);
//        }

//        PlaySwitchPanelSound();
//    }

//    public void PlayGame()
//    {
//        SceneManager.LoadScene(level1SceneName);
//    }

//    public void ExitGame()
//    {
//        PlayClickSound();
//        Application.Quit();
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
//    }

//    public void PlayHoverSound()
//    {
//        if (audioSource != null && hoverSound != null)
//            audioSource.PlayOneShot(hoverSound);
//    }

//    public void PlayClickSound()
//    {
//        if (audioSource != null && clickSound != null)
//            audioSource.PlayOneShot(clickSound);
//    }

//    public void PlaySwitchPanelSound()
//    {
//        if (audioSource != null && switchPanelSound != null)
//            audioSource.PlayOneShot(switchPanelSound);
//    }
//}
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

        // Hover enter event
        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(hoverEnter);

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