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
    public AudioClip rollingSound; // Single looping sound
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
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup audio sources
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();

        // Configure music source
        musicSource.loop = true;
        musicSource.volume = musicVolume; // Use the slider value
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
        // When a new scene loads, look for LevelMusic in that scene
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

    // Remove the old Start() method that was doing this
    //void Start()
    //{
    //    // Try to find a LevelMusic component in the current scene
    //    LevelMusic levelMusic = FindFirstObjectByType<LevelMusic>();
    //    if (levelMusic != null && levelMusic.levelMusic != null)
    //    {
    //        ChangeBackgroundMusic(levelMusic.levelMusic);
    //    }
    //}

    void Update()
    {
        // Rolling sound control
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



    // Call this from RobotController movement
    // Call this from RobotController movement
    public void SetMoving(bool moving, float currentSpeed = 0, float maxSpeed = 1)
    {
        isMoving = moving;
        this.currentSpeed = currentSpeed;
        this.maxSpeed = maxSpeed;
    }


    // Sound methods to call from other scripts
    //public void PlayDanceSound()
    //{
    //    if (danceSounds.Length > 0)
    //    {
    //        AudioClip clip = danceSounds[Random.Range(0, danceSounds.Length)];
    //        voiceSource.PlayOneShot(clip);
    //    }
    //}

    //public void PlayCrySound()
    //{
    //    if (crySound != null)
    //        voiceSource.PlayOneShot(crySound);
    //}

    //public void PlayFallSound()
    //{
    //    if (fallSound != null)
    //        voiceSource.PlayOneShot(fallSound);
    //}

    //public void PlayAngrySound()
    //{
    //    if (angrySound != null)
    //        voiceSource.PlayOneShot(angrySound);
    //}

    //public void PlayBumpSound()
    //{
    //    if (bumpSound != null)
    //        sfxSource.PlayOneShot(bumpSound, 0.5f);
    //}

    //public void PlayBallHitSound()
    //{
    //    if (ballHitSound != null)
    //        sfxSource.PlayOneShot(ballHitSound);
    //}

    //public void PlayPickupSound()
    //{
    //    if (pickupSound != null)
    //        sfxSource.PlayOneShot(pickupSound);
    //}

    //public void PlayPlaceSound()
    //{
    //    if (placeSound != null)
    //        sfxSource.PlayOneShot(placeSound);
    //}

    //public void PlayToggleStateSound()
    //{
    //    if (toggleStateSound != null)
    //        sfxSource.PlayOneShot(toggleStateSound);
    //}

    //public void PlayMagnetToggleSound()
    //{
    //    if (magnetToggleSound != null)
    //        sfxSource.PlayOneShot(magnetToggleSound);
    //}

    //public void PlayLeverPullSound()
    //{
    //    if (leverPullSound != null)
    //        sfxSource.PlayOneShot(leverPullSound);
    //}
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
            musicSource.volume = musicVolume * 0.3f; // Lower volume during pause
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume; // Restore normal volume
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

        //if (volume >= 0)
        //    musicSource.volume = volume;
        musicSource.volume = volume >= 0 ? volume : musicVolume;

        musicSource.Play();
    }
}