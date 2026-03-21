// 3rd party but refactored 
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Scrolling Settings")]
    [SerializeField] private float horizontalSpeed = 0.2f;
    [SerializeField] private int trackCount = 3;

    // Private backing field
    private int currentTrack = 2;
    private Vector2 uvOffset = Vector2.zero;

    // Public property for current track (if external access needed)
    public int CurrentTrack
    {
        get => currentTrack;
        set => currentTrack = value;
    }

    void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("Target Renderer is not assigned!");
        }
        else
        {
            // vertical step per track
            float verticalStep = 1.0f / trackCount;
        }
    }

    void Update()
    {
        if (targetRenderer != null)
        {
            uvOffset.x += horizontalSpeed * Time.deltaTime;

            // currentTrack = (currentTrack + 1) % trackCount;

            uvOffset.y = currentTrack * (1.0f / trackCount);

            targetRenderer.material.SetTextureOffset("_MainTex", uvOffset);
        }
    }
}
