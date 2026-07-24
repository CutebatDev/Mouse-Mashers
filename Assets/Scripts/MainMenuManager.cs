using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    
    [Header("Values")]
    [SerializeField] private string mainGameSceneName;
    [SerializeField] private AudioClip mainMenuMusic;
    
    [Header("References")]
    [SerializeField] private GameObject defaultMenuGroup;
    [SerializeField] private GameObject settingsMenuGroup;
    [SerializeField] private GameObject title;
    [SerializeField] private Toggle coolModeToggle;


    private void Start()
    {
        AudioManager.Instance.PlayMusic(mainMenuMusic, 0.25f);
        
        coolModeToggle.isOn = SettingsManager.Instance.isCoolModeEnabled;
    }


    public void OnCoolModeToggle(bool isToggled)
    {
        SettingsManager.Instance.isCoolModeEnabled = isToggled;
        SettingsManager.Instance.SaveSettingsToPrefs();
    }
    

    public void OnPlayPressed()
    {
        // AudioManager.Instance.StopMusic();
        SceneManager.LoadScene(mainGameSceneName);
    }


    public void OnSettingsPressed()
    {
        defaultMenuGroup.SetActive(false);
        settingsMenuGroup.SetActive(true);
        title.SetActive(false);
    }


    public void OnBackToMenuPressed()
    {
        defaultMenuGroup.SetActive(true);
        settingsMenuGroup.SetActive(false);
        title.SetActive(true);
    }
    
    
    public void OnQuitPressed()
    {
        Application.Quit();
    }


    public void OnAudioSliderChanged(float value)
    {
        AudioManager.Instance.SetMixerVolume(value);
    }
}
