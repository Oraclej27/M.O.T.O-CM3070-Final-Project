using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public string tutorialMessage = "Press SPACE to pick up blocks";
    public float displayTime = 5f;
    public bool showOnce = true;
    public KeyCode optionalKeyToPress = KeyCode.None;

    [Header("UI Reference")]
    public Text tutorialText;

    private bool hasBeenShown = false;

    void Start()
    {
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

            TutorialManager manager = FindFirstObjectByType<TutorialManager>();
            if (manager != null)
            {
                manager.ShowMessage(tutorialMessage, displayTime, optionalKeyToPress);
                hasBeenShown = true;
            }
        }
    }
}