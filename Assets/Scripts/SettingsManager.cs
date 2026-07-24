using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public bool isCoolModeEnabled = true;
    private const string CoolModeSettingsName = "CoolMode";
    
    
    private void Awake()
    {
        LoadSettingsFromPrefs();

        if (Instance != null && Instance != this) {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
        
    }


    public void SaveSettingsToPrefs()
    {
        PlayerPrefs.SetInt(CoolModeSettingsName, isCoolModeEnabled ? 1 : 0);
    }


    private void LoadSettingsFromPrefs()
    {
        isCoolModeEnabled = PlayerPrefs.GetInt(CoolModeSettingsName) != 0;
    }
    
}
