using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    
    [Header("Scene Settings")]
    [SerializeField] private string mainGameSceneName;
    
    [Header("References")]
    [SerializeField] private GameObject defaultMenuGroup;
    [SerializeField] private GameObject settingsMenuGroup;
    [SerializeField] private GameObject title;
    
    
    public void OnPlayPressed()
    {
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
        Debug.Log("Audio Slider: " + value);
    }
}
