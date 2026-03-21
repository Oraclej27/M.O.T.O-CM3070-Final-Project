// 3rd party but reffactored
using UnityEngine;

public class Rob13ColorManager : MonoBehaviour
{
    [Header("Color Presets")]
    [SerializeField] private Color[] predefinedColors = new Color[10];

    [Header("Renderers")]
    [SerializeField] private Renderer[] bodyRenderers;
    [SerializeField] private Renderer eyesRenderer;
    [SerializeField] private Renderer mouthRenderer;
    [SerializeField] private Renderer mouthSpeechRenderer;

    [Header("Settings")]
    [SerializeField][Range(0, 10)] private float emissionIntensity = 1f;

    private bool isRainbowCycles = false;
    private bool isBattle = false;
    private int colorIndex = 0;        
    private int eyesColorIndex = 1;
    private int mouthColorIndex = 2;
    private Color rainbowColor = Color.red;

    // Public properties for external access
    public bool IsRainbowCycles
    {
        get => isRainbowCycles;
        set
        {
            isRainbowCycles = value;
            // Optional: trigger immediate update if needed
        }
    }

    public bool IsBattle
    {
        get => isBattle;
        set => isBattle = value;
    }

    public float EmissionIntensity
    {
        get => emissionIntensity;
        set
        {
            emissionIntensity = Mathf.Clamp(value, 0f, 10f);
            UpdateColors();
        }
    }

    void Start()
    {
        UpdateColors();
    }

    private void Update()
    {
        if (isRainbowCycles)
        {
            float hue = Mathf.Repeat(Time.time / 2, 1f);
            rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            foreach (var renderer in bodyRenderers)
            {
                if (renderer != null)
                    renderer.material.SetColor("_EmissionColor", rainbowColor * emissionIntensity);
            }

            if (mouthRenderer != null)
                mouthRenderer.material.SetColor("_EmissionColor", rainbowColor * emissionIntensity);
            if (eyesRenderer != null)
                eyesRenderer.material.SetColor("_EmissionColor", rainbowColor * emissionIntensity);
        }
    }

    private void UpdateColors()
    {
        ApplyColor(colorIndex);
    }

    private void ApplyColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < predefinedColors.Length)
        {
            Color colorToApply = predefinedColors[colorIndex];
            ApplyEmissionColor(colorToApply);
        }
    }

    private void ApplyEmissionColor(Color emissionColor)
    {
        foreach (var renderer in bodyRenderers)
        {
            if (renderer != null)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
        }

        if (eyesRenderer != null)
        {
            eyesRenderer.material.EnableKeyword("_EMISSION");
            eyesRenderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }

        if (mouthRenderer != null)
        {
            mouthRenderer.material.EnableKeyword("_EMISSION");
            mouthRenderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }

        if (mouthSpeechRenderer != null)
        {
            mouthSpeechRenderer.material.EnableKeyword("_EMISSION");
            mouthSpeechRenderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }
    }

    public void ChangeBodyColor(int newColorIndex)
    {
        if (newColorIndex >= 0 && newColorIndex < predefinedColors.Length)
        {
            colorIndex = newColorIndex;
            ApplyColor(colorIndex);
        }
    }

    // Optional: methods to change eye/mouth color separately if needed
    public void SetEyesColorIndex(int index)
    {
        eyesColorIndex = index;
        // Apply eye color logic if you have separate control
    }

    public void SetMouthColorIndex(int index)
    {
        mouthColorIndex = index;
        // Apply mouth color logic if needed
    }
}
