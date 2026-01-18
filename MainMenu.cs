using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{

[Header("Settings Menu")]
[SerializeField] private SettingsMenu settingsMenu;
[SerializeField] private Button settingsButton;
[SerializeField] private Button quitButton;

[SerializeField] private Button playButton;
    private void Start()
    {
         if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuMusic();
        }
         if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
         if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
    }
  public void PlayGame()
    {
      if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMenuMusic();
            SoundManager.Instance.PlayButtonClick();
        }
     
        
        
        SceneManager.LoadScene(2);
    }

  public void OnSettingsClicked()
    {
        Debug.Log("Settings is Clicked");
        if (settingsMenu != null)
        {
            settingsMenu.OpenSettings();
        }
    }

private void OnQuitClicked()
    {
         Debug.Log("Quit Button is Clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    
}
