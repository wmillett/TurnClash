using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button backButton;
    
    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;
    private CameraMovement cameraMovement;
    
    private void Start()
    {
        // Find camera movement component if exists
        cameraMovement = Camera.main.GetComponent<CameraMovement>();
        
        // Add event listeners
        if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton) optionsButton.onClick.AddListener(ShowOptions);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);
        if (backButton) backButton.onClick.AddListener(CloseOptions);
        
        // Start with menu hidden
        pauseMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
    }
    
    private void Update()
    {
        // Toggle pause when key is pressed
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        pauseMenuPanel.SetActive(isPaused);
        
        if (isPaused)
        {
            // Disable camera movement when paused
            if (cameraMovement) cameraMovement.SetEnabled(false);
            
            // Pause game time (affects animations and physics)
            Time.timeScale = 0f;
        }
        else
        {
            // Close any sub-panels
            if (optionsPanel) optionsPanel.SetActive(false);
            
            // Re-enable camera movement
            if (cameraMovement) cameraMovement.SetEnabled(true);
            
            // Resume normal time scale
            Time.timeScale = 1f;
        }
    }
    
    public void ResumeGame()
    {
        TogglePause();
    }
    
    public void ShowOptions()
    {
        optionsPanel.SetActive(true);
        
        // Update sliders to current values
        if (musicVolumeSlider) musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (sfxVolumeSlider) sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
    }
    
    public void CloseOptions()
    {
        // Save current volume settings
        if (musicVolumeSlider) PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        if (sfxVolumeSlider) PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.Save();
        
        optionsPanel.SetActive(false);
    }
    
    public void ReturnToMainMenu()
    {
        // Resume normal time scale before loading scene
        Time.timeScale = 1f;
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 