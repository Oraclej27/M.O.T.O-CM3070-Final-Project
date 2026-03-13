// =============================================
// Script: SoundController.cs
// Purpose: Audio manager for the game. Persists across scenes.
//
// Communicates with:
//   - All gameplay scripts: Provides public methods like PlayPickupSound(), PlayBumpSound(), etc.
//   - LevelMusic: Finds LevelMusic component in each scene to change background music.
//   - RobotController: Receives movement state via SetMoving() to control rolling sound.
//
// Usage: Attached to a persistent GameObject.
// =============================================
using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource; 


    [Header("Level Music")]
    public AudioClip defaultMusic;

    [Header("Robot Emotional Sounds")]
    public AudioClip[] danceSounds; 
    public AudioClip crySound;
    public AudioClip fallSound;
    public AudioClip angrySound;
    public AudioClip bumpSound;
    public AudioClip ballHitSound;

    [Header("Block Interaction")]
    public AudioClip pickupSound;
    public AudioClip placeSound;
    public AudioClip toggleStateSound;
    public AudioClip magnetToggleSound;

    [Header("Lever Sounds")]
    public AudioClip leverPullSound;

    [Header("Movement")]
    public AudioClip rollingSound; 
    public float rollingPitchMin = 0.8f;
    public float rollingPitchMax = 1.2f;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 0.7f;

    private static SoundController _instance;
    public static SoundController Instance { get { return _instance; } }

    private bool isMoving = false;
    private bool wasMoving = false;
    private float currentSpeed = 0;
    private float maxSpeed = 1;
    private float originalMusicVolume;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = musicVolume; 
        originalMusicVolume = musicVolume;
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        LevelMusic levelMusic = FindFirstObjectByType<LevelMusic>();
        if (levelMusic != null && levelMusic.levelMusic != null)
        {
            ChangeBackgroundMusic(levelMusic.levelMusic);
        }
        else if (defaultMusic != null)
        {
            ChangeBackgroundMusic(defaultMusic);
        }
    }

    void Update()
    {
        if (isMoving != wasMoving)
        {
            if (isMoving)
            {
                sfxSource.clip = rollingSound;
                sfxSource.loop = true;
                sfxSource.pitch = Random.Range(rollingPitchMin, rollingPitchMax);
                sfxSource.Play();
            }
            else
            {
                sfxSource.Stop();
                sfxSource.loop = false;
            }
            wasMoving = isMoving;
        }
    }

    public void SetMoving(bool moving, float currentSpeed = 0, float maxSpeed = 1)
    {
        isMoving = moving;
        this.currentSpeed = currentSpeed;
        this.maxSpeed = maxSpeed;
    }
    public void PlayDanceSound()
    {
        if (danceSounds.Length > 0)
        {
            AudioClip clip = danceSounds[Random.Range(0, danceSounds.Length)];
            voiceSource.PlayOneShot(clip, voiceVolume);
        }
    }

    public void PlayCrySound()
    {
        if (crySound != null)
            voiceSource.PlayOneShot(crySound, voiceVolume);
    }

    public void PlayFallSound()
    {
        if (fallSound != null)
            voiceSource.PlayOneShot(fallSound, voiceVolume);
    }

    public void PlayAngrySound()
    {
        if (angrySound != null)
            voiceSource.PlayOneShot(angrySound, voiceVolume);
    }

    public void PlayBumpSound()
    {
        if (bumpSound != null)
            sfxSource.PlayOneShot(bumpSound, sfxVolume * 0.5f);
    }

    public void PlayBallHitSound()
    {
        if (ballHitSound != null)
            sfxSource.PlayOneShot(ballHitSound, sfxVolume);
    }

    public void PlayPickupSound()
    {
        if (pickupSound != null)
            sfxSource.PlayOneShot(pickupSound, sfxVolume);
    }

    public void PlayPlaceSound()
    {
        if (placeSound != null)
            sfxSource.PlayOneShot(placeSound, sfxVolume);
    }

    public void PlayToggleStateSound()
    {
        if (toggleStateSound != null)
            sfxSource.PlayOneShot(toggleStateSound, sfxVolume);
    }

    public void PlayMagnetToggleSound()
    {
        if (magnetToggleSound != null)
            sfxSource.PlayOneShot(magnetToggleSound, sfxVolume);
    }

    public void PlayLeverPullSound()
    {
        if (leverPullSound != null)
            sfxSource.PlayOneShot(leverPullSound, sfxVolume);
    }

    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * 0.3f; 
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume; 
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void StartMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void ChangeBackgroundMusic(AudioClip newMusic, float volume = -1)
    {
        if (musicSource == null) return;

        musicSource.Stop();
        musicSource.clip = newMusic;

        musicSource.volume = volume >= 0 ? volume : musicVolume;

        musicSource.Play();
    }
}