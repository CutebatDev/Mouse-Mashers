using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private AudioMixerGroup mainMixerGroup;
    private AudioSource _musicSource;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }


    public void SetMixerVolume(float linearVolume)
    {
        mainMixerGroup.audioMixer.SetFloat("MainVolume", Mathf.Log10(Mathf.Clamp(linearVolume, 0.0001f, 1f)) * 20f);
    }
    
    
    public void PlaySfx2D(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        GameObject audioSourceObject = Instantiate(Instance.audioSourcePrefab);
        AudioSource audioSourceComponent = audioSourceObject.GetComponent<AudioSource>();

        audioSourceComponent.volume = volume;
        audioSourceComponent.pitch = pitch;

        audioSourceComponent.PlayOneShot(clip, volume);
        Destroy(audioSourceObject, clip.length);
    }
    

    public void PlayMusic(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        GameObject audioSourceObject = Instantiate(Instance.audioSourcePrefab);
        AudioSource audioSourceComponent = audioSourceObject.GetComponent<AudioSource>();
        
        audioSourceComponent.clip = clip;
        audioSourceComponent.loop = true;
        audioSourceComponent.volume = volume;
        audioSourceComponent.pitch = pitch;

        audioSourceComponent.Play();
        _musicSource = audioSourceComponent;
        DontDestroyOnLoad(audioSourceObject);
    }

    public void StopMusic()
    {
        if (_musicSource == null)
            return;

        _musicSource.Stop();
        Destroy(_musicSource.gameObject);
        _musicSource = null;
    }
}
