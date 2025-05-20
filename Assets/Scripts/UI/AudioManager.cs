using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    
    // Mixer parameter names
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    
    private void Start()
    {
        // Load saved volume settings
        LoadVolumeSettings();
    }
    
    public void SetMusicVolume(float sliderValue)
    {
        // Convert slider value (0 to 1) to logarithmic scale for better volume control
        float dbValue = ConvertToDecibel(sliderValue);
        masterMixer.SetFloat(MUSIC_VOLUME, dbValue);
        
        // Save the setting
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float sliderValue)
    {
        // Convert slider value (0 to 1) to logarithmic scale for better volume control
        float dbValue = ConvertToDecibel(sliderValue);
        masterMixer.SetFloat(SFX_VOLUME, dbValue);
        
        // Save the setting
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        // Get saved volume settings (default 0.75 = 75%)
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Apply volume settings to audio mixer
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    // Convert linear slider value (0 to 1) to logarithmic decibel scale (-80dB to 0dB)
    private float ConvertToDecibel(float linearValue)
    {
        // Avoid -infinity when value is 0
        if (linearValue <= 0.001f)
            return -80f;
            
        // Convert to logarithmic scale
        return Mathf.Log10(linearValue) * 20f;
    }
} 