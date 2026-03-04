using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public string tutorialMessage = "Press SPACE to pick up blocks";
    public float displayTime = 5f;
    public bool showOnce = true;
    public KeyCode optionalKeyToPress = KeyCode.None; // If they need to press a key

    [Header("UI Reference")]
    public Text tutorialText; // Assign in Inspector or it will find it

    private bool hasBeenShown = false;

    void Start()
    {
        // Find tutorial text if not assigned - UPDATED SYNTAX
        if (tutorialText == null)
        {
            TutorialManager tm = FindFirstObjectByType<TutorialManager>();
            if (tm != null)
                tutorialText = tm.tutorialText;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
        {
            if (showOnce && hasBeenShown) return;

            // UPDATED SYNTAX HERE TOO
            TutorialManager manager = FindFirstObjectByType<TutorialManager>();
            if (manager != null)
            {
                manager.ShowMessage(tutorialMessage, displayTime, optionalKeyToPress);
                hasBeenShown = true;
            }
        }
    }
}