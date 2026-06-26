using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    
    [Header("Values")]
    [SerializeField] private string mainGameSceneName;
    [SerializeField] private AudioClip mainMenuMusic;
    
    [Header("References")]
    [SerializeField] private GameObject defaultMenuGroup;
    [SerializeField] private GameObject settingsMenuGroup;
    [SerializeField] private GameObject title;


    private void Start()
    {
        AudioManager.Instance.PlayMusic(mainMenuMusic);
    }


    public void OnPlayPressed()
    {
        AudioManager.Instance.StopMusic();
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
