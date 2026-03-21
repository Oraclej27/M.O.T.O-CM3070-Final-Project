// 3rd party but reffactored
using UnityEngine;

public class EmotionChanger : MonoBehaviour
{
    [Header("Emotion Settings")]
    [SerializeField] private int totalEmotions = 10;

    [Header("Renderers")]
    [SerializeField] private Renderer objectRendererEyes;
    [SerializeField] private Renderer objectRendererMouth;

    // Private backing fields for current indices
    private int currentEmotionEyesIndex = 0;
    private int currentEmotionMouthIndex = 0;

    // Public read-only properties (if needed externally)
    public int CurrentEmotionEyesIndex => currentEmotionEyesIndex;
    public int CurrentEmotionMouthIndex => currentEmotionMouthIndex;

    void Start()
    {
        UpdateEmotion();
    }

    public void SetEmotionEyes(int emotionIndex)
    {
        if (emotionIndex >= 0 && emotionIndex < totalEmotions)
        {
            currentEmotionEyesIndex = emotionIndex;
            UpdateEmotion();
        }
        else
        {
            Debug.LogWarning("Emotion index out of range!");
        }
    }

    public void SetEmotionMouth(int emotionIndex)
    {
        if (emotionIndex >= 0 && emotionIndex < totalEmotions)
        {
            currentEmotionMouthIndex = emotionIndex;
            UpdateEmotion();
        }
        else
        {
            Debug.LogWarning("Emotion index out of range!");
        }
    }

    private void UpdateEmotion()
    {
        if (objectRendererEyes != null && objectRendererMouth != null)
        {
            float offsetXEyes = (float)currentEmotionEyesIndex / totalEmotions;
            objectRendererEyes.material.SetTextureOffset("_MainTex", new Vector2(offsetXEyes, 0));

            float offsetXMouth = (float)currentEmotionMouthIndex / totalEmotions;
            objectRendererMouth.material.SetTextureOffset("_MainTex", new Vector2(offsetXMouth, 0));
        }
        else
        {
            Debug.LogError("Object Renderer is not assigned!");
        }
    }
}
