using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SettingsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;
    
    [Header("Audio Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    
    [Header("Difficulty Controls")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TextMeshProUGUI difficultyDescriptionText;
    [SerializeField] private AyoGameManager gameManager;
    
    [Header("Other Settings")]
    // [SerializeField] private Toggle vibrationToggle;
    // [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Toggle muteMusicToggle;
    [SerializeField] private Toggle muteSFXToggle;
    
    // private const string MUSIC_VOLUME_KEY = "MusicVolume";
    // private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string DIFFICULTY_KEY = "GameDifficulty";
    // private const string VIBRATION_KEY = "Vibration";
    // private const string TUTORIAL_KEY = "ShowTutorial";
    private bool initializing = false;
    private void Start()
    {
        // Hide settings panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        SetupUI();
        LoadSettings();
    }
    
    private void SetupUI()
    {
        // Close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        // Music volume slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        // SFX volume slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (muteMusicToggle != null)
        {
            muteMusicToggle.onValueChanged.AddListener(OnMuteMusicToggled);
        }
        
        if (muteSFXToggle != null)
        {
            muteSFXToggle.onValueChanged.AddListener(OnMuteSFXToggled);
        }
        
        // Difficulty dropdown
        if (difficultyDropdown != null)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Easy",
                "Medium",
                "Hard"
            });
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }
        
        // Vibration toggle
        // if (vibrationToggle != null)
        // {
        //     vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
        // }
        
        // // Tutorial toggle
        // if (tutorialToggle != null)
        // {
        //     tutorialToggle.onValueChanged.AddListener(OnTutorialToggled);
        // }
        initializing = false;
    }
    
    private void LoadSettings()
    {

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = SoundManager.Instance.GetMasterVolume();
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = SoundManager.Instance.GetMusicVolume();
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = SoundManager.Instance.GetSFXVolume();;
        }
        
        // Load toggle states
        if (muteMusicToggle != null)
        {
            muteMusicToggle.isOn = SoundManager.Instance.GetMusicVolume() == 0f;
        }
        
        if (muteSFXToggle != null)
        {
            muteSFXToggle.isOn = SoundManager.Instance.GetSFXVolume() == 0f;
        }
        // Load difficulty
        int difficulty = PlayerPrefs.GetInt(DIFFICULTY_KEY, 1); // Default: Medium
        if (difficultyDropdown != null)
        {
            difficultyDropdown.value = difficulty;
            UpdateDifficultyDescription((Difficulty)difficulty);
        }
        
        // Load vibration
        // bool vibration = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
        // if (vibrationToggle != null)
        // {
        //     vibrationToggle.isOn = vibration;
        // }
        
        // // Load tutorial
        // bool tutorial = PlayerPrefs.GetInt(TUTORIAL_KEY, 1) == 1;
        // if (tutorialToggle != null)
        // {
        //     tutorialToggle.isOn = tutorial;
        // }
        
        // Apply settings to game
        ApplySettings();
    }
    
    private void ApplySettings()
    {
        // Apply music volume
        if (SoundManager.Instance != null && musicVolumeSlider != null)
        {
            SoundManager.Instance.SetMusicVolume(musicVolumeSlider.value);
        }
        
        // Apply SFX volume
        if (SoundManager.Instance != null && sfxVolumeSlider != null)
        {
            SoundManager.Instance.SetSFXVolume(sfxVolumeSlider.value);
        }
        
        // Apply difficulty
        if (gameManager != null && difficultyDropdown != null)
        {
            gameManager.SetDifficulty((Difficulty)difficultyDropdown.value);
        }
    }
    
    // ========== AUDIO CONTROLS ==========

    private void OnMasterVolumeChanged(float value)
    {
        if (initializing) return;
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);
        }
        
        UpdateVolumeText(masterVolumeText, value);
    }
    private void OnMusicVolumeChanged(float value)
    {
        if (initializing) return;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
        
        UpdateVolumeText(musicVolumeText, value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (initializing) return;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
            SoundManager.Instance.PlayButtonClick(); // Preview sound
        }
    
        UpdateVolumeText(sfxVolumeText, value);
    }


    private void UpdateVolumeText(TextMeshProUGUI textComponent, float value)
    {
        if (textComponent != null)
        {
            textComponent.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    
    // ========== DIFFICULTY CONTROLS ==========
    private void OnDifficultyChanged(int index)
    {
        if (initializing) return;
        Difficulty difficulty = (Difficulty)index;
        
        // Apply to game manager
        if (gameManager != null)
        {
            gameManager.SetDifficulty(difficulty);
        }
        
        // Update description
        UpdateDifficultyDescription(difficulty);
        
        // Save preference
        PlayerPrefs.SetInt(DIFFICULTY_KEY, index);
        PlayerPrefs.Save();
        
        // Play sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
        
        Debug.Log($"Difficulty changed to: {difficulty}");
    }
    
    private void UpdateDifficultyDescription(Difficulty difficulty)
    {
        if (difficultyDescriptionText == null) return;
        
        switch (difficulty)
        {
            case Difficulty.Easy:
                difficultyDescriptionText.text = "Perfect for learning the game. The Keeper makes random moves.";
                break;
            case Difficulty.Medium:
                difficultyDescriptionText.text = "Balanced challenge. The Keeper tries to capture stones when possible.";
                break;
            case Difficulty.Hard:
                difficultyDescriptionText.text = "Expert level. The Keeper plays strategically and plans ahead.";
                break;
        }
    }
    
    // ========== OTHER SETTINGS ==========
    // private void OnVibrationToggled(bool isOn)
    // {
    //     PlayerPrefs.SetInt(VIBRATION_KEY, isOn ? 1 : 0);
    //     PlayerPrefs.Save();
        
    //     if (SoundManager.Instance != null)
    //     {
    //         SoundManager.Instance.PlayButtonClick();
    //     }
    // }
    
    // private void OnTutorialToggled(bool isOn)
    // {
    //     PlayerPrefs.SetInt(TUTORIAL_KEY, isOn ? 1 : 0);
    //     PlayerPrefs.Save();
        
    //     if (SoundManager.Instance != null)
    //     {
    //         SoundManager.Instance.PlayButtonClick();
    //     }
    // }
    
    private void OnMuteMusicToggled(bool isMuted)
    {
        if (initializing) return;
        
        if (SoundManager.Instance != null)
        {
            if (isMuted)
            {
                SoundManager.Instance.SetMusicVolume(0f);
            }
            else
            {
                SoundManager.Instance.SetMusicVolume(0.7f); // Default value
            }
            
            LoadSettings();
        }
    }
    
    private void OnMuteSFXToggled(bool isMuted)
    {
        if (initializing) return;
        
        if (SoundManager.Instance != null)
        {
            if (isMuted)
            {
                SoundManager.Instance.SetSFXVolume(0f);
            }
            else
            {
                SoundManager.Instance.SetSFXVolume(1f); // Default value
            }
            
            LoadSettings();
        }
    }
    // ========== PANEL MANAGEMENT ==========
public void OpenSettings()
{
    Debug.Log("Settings Button Clicked");
    if (settingsPanel != null)
    {
        // Cancel any existing tweens on this panel
        LeanTween.cancel(settingsPanel);
        
        // Activate panel first
        settingsPanel.SetActive(true);
        
        // Reset scale to zero BEFORE animating
        settingsPanel.transform.localScale = Vector3.zero;
        
        // Use Vector3 instead of Vector2 for 3D scale
        LeanTween.scale(settingsPanel, Vector3.one, 0.3f)
            .setEaseOutBack()
            .setIgnoreTimeScale(true); // Important: Works even if game is paused
        
        // Play sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
    }
    else
    {
        Debug.LogError("Settings Panel is null!");
    }
}
    
    public void CloseSettings()
{
    Debug.Log("Closing Settings");
    if (settingsPanel != null)
    {
        // Cancel any existing tweens
        LeanTween.cancel(settingsPanel);
        
        // Animate scale down
        LeanTween.scale(settingsPanel, Vector3.zero, 0.2f)
            .setEaseInBack()
            .setIgnoreTimeScale(true)
            .setOnComplete(() => {
                // Deactivate after animation completes
                if (settingsPanel != null)
                {
                    settingsPanel.SetActive(false);
                }
            });
        
        // Play sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
    }
}
    
    // ========== RESET SETTINGS ==========
    public void ResetToDefault()
    {
        // PlayerPrefs.DeleteKey(MUSIC_VOLUME_KEY);
        // PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
        // PlayerPrefs.DeleteKey(DIFFICULTY_KEY);
        // // PlayerPrefs.DeleteKey(VIBRATION_KEY);
        // // PlayerPrefs.DeleteKey(TUTORIAL_KEY);
        // PlayerPrefs.Save();
        
        
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
            LoadSettings();
        }
        
        Debug.Log("Settings reset to default");
    }
}