using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Singleton Sound Manager - Persists across all scenes
/// Manages all game audio including music and sound effects
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton instance
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundManager>();
                
                if (instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    instance = go.AddComponent<SoundManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource menuMusicSource;
    
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip menuMusic;

    [Header("Game Start/End Sounds")]
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private AudioClip gameWinSound;
    [SerializeField] private AudioClip gameLoseSound;
    [SerializeField] private AudioClip gameDrawSound;
    
    [Header("Round Sounds")]
    [SerializeField] private AudioClip roundStartSound;
    [SerializeField] private AudioClip roundWinSound;
    [SerializeField] private AudioClip roundLoseSound;
    [SerializeField] private AudioClip roundDrawSound;
    
    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip potSelectSound;
    [SerializeField] private AudioClip stonePickupSound;
    [SerializeField] private AudioClip stoneDropSound;
    [SerializeField] private AudioClip stoneDropEmptySound;
    [SerializeField] private AudioClip stoneCaptureSound;
    [SerializeField] private AudioClip compPointScored;
    [SerializeField] private AudioClip pointScoredSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
    
    private void Awake()
    {
        // Implement Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            InitializeAudioSources();
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }
    }

 
    private void Start()
    {
        LoadVolumeSettings();
        // PlayBackgroundMusic();
        Debug.Log("SoundManager initialized and playing music");
    }
    
    private void InitializeAudioSources()
    {
        // Create music audio source if not assigned
        if (backgroundMusicSource == null)
        {
            Debug.Log("Creating Music Audio Source");
            GameObject backgroundMusicObj = new GameObject("BackgroundMusicSource");
            backgroundMusicObj.transform.SetParent(transform);
            backgroundMusicSource = backgroundMusicObj.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.playOnAwake = false;
        }

        if (menuMusicSource == null)
        {
            Debug.Log("Creating Menu Music Audio Source");
            GameObject menuMusicObj = new GameObject("MenuMusicSource");
            menuMusicObj.transform.SetParent(transform);
            menuMusicSource = menuMusicObj.AddComponent<AudioSource>();
            menuMusicSource.loop = true;
            menuMusicSource.playOnAwake = false;
        }
        
        // Create SFX audio source if not assigned
        if (sfxSource == null)
        {
            Debug.Log("Creating SFX Audio Source");
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }
    
    // ========== MUSIC CONTROLS ==========
    
    public void PlayBackgroundMusic()
    {
       
        Scene scene = SceneManager.GetActiveScene();
         Debug.Log($"Playing background music: {scene.name}");
        if (backgroundMusic != null && backgroundMusicSource != null && scene.name == "GameScene")
        {
            StopBackgroundMusic();
            StopMenuMusic();
            Debug.Log("About to see if backgroundMusic is playing");
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.volume = musicVolume * masterVolume;
            backgroundMusicSource.Play();
        }
    }

    public void PlayMenuMusic()
    {
        
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log($"Playing Menu background music: {scene.name}");
        if (menuMusic != null && menuMusicSource != null && scene.name == "MenuScene")
        {
            Debug.Log("Playing Menu background music");
            StopBackgroundMusic();
            // StopMenuMusic();
            Debug.Log("Palying menu music");
            menuMusicSource.clip = menuMusic;
            menuMusicSource.volume = musicVolume * masterVolume;
            menuMusicSource.Play();
        }
    }

    public void StopMenuMusic()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (menuMusicSource != null  && scene.name == "MenuScene")
        {
            
            menuMusicSource.Stop();
        }
    }
    
    public void StopBackgroundMusic()
    {
        // Scene scene = SceneManager.GetActiveScene();
            if(backgroundMusicSource != null)
        {
             Debug.Log("Stopping background music");
            backgroundMusicSource.Stop();
        }
           
  
    }


    
    public void PauseBackgroundMusic()
    {
         Scene scene = SceneManager.GetActiveScene();
        if (backgroundMusicSource.isPlaying && scene.name == "gameScene")
        {
            backgroundMusicSource.Pause();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (backgroundMusicSource != null && scene.name == "gameScene")
        {
            backgroundMusicSource.UnPause();
        }
    }
    
    // ========== SOUND EFFECTS ==========
    
    private void PlaySFX(AudioClip clip, float volumeMultiplier = 1f, bool randomizePitch = false)
    {
        // if (clip != null && sfxSource != null)
        // {
        //     sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeMultiplier);
        // }
         if (clip != null && sfxSource != null)
    {
        if (randomizePitch)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
        }
        else
        {
            sfxSource.pitch = 1f;
        }
        
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeMultiplier);
    }
    }
    
    // Game Start/End
    // public void PlayGameStart()
    // {
    //     PlaySFX(gameStartSound);
    // }
    
    public void PlayGameWin()
    {
        PlaySFX(gameWinSound);
    }
    
    public void PlayGameLose()
    {
        PlaySFX(gameLoseSound);
    }
    
    public void PlayGameDraw()
    {
        PlaySFX(gameDrawSound);
    }
    
    // Round Sounds
    public void PlayRoundStart()
    {
        PlaySFX(roundStartSound);
    }
    
    public void PlayRoundWin()
    {
        PlaySFX(roundWinSound);
    }
    
    public void PlayRoundLose()
    {
        PlaySFX(roundLoseSound);
    }
    
    public void PlayRoundDraw()
    {
        PlaySFX(roundDrawSound);
    }
    
    // Gameplay Sounds
    public void PlayPotSelect()
    {
        PlaySFX(potSelectSound);
    }
    
    public void PlayStonePickup()
    {
        PlaySFX(stonePickupSound);
    }
    
    public void PlayStoneDrop(bool potWasEmpty)
    {
        if (potWasEmpty && stoneDropEmptySound != null)
        {
            PlaySFX(stoneDropEmptySound, 0.8f, true);
        }
        else
        {
            PlaySFX(stoneDropSound, 0.8f, true);
        }
    }
    
    public void PlayStoneCapture()
    {
        PlaySFX(stoneCaptureSound, 0.8f, true);
    }
    
    public void PlayPointScored()
    {
        PlaySFX(pointScoredSound);
    }

    public void PlayComputerPointScored()
    {
        PlaySFX(compPointScored);
    }
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }
    
    // ========== VOLUME CONTROLS ==========
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
    }
    
    private void UpdateVolumes()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = musicVolume * masterVolume;
        }
        if (menuMusicSource != null)
        {
            menuMusicSource.volume = musicVolume * masterVolume;
        }
    }
    
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    
    // ========== SETTINGS PERSISTENCE ==========
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
        
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }
        
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
        }
        
        UpdateVolumes();
    }
    
    // ========== UTILITY ==========
    
    public void MuteAll()
    {
        SetMasterVolume(0f);
    }
    
    public void UnmuteAll()
    {
        SetMasterVolume(1f);
    }
    
    public void ToggleMute()
    {
        if (masterVolume > 0f)
        {
            SetMasterVolume(0f);
        }
        else
        {
            SetMasterVolume(1f);
        }
    }
}