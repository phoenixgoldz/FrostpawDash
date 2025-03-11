using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;  // Main music player
    public AudioSource sfxSource;    // Sound Effects (SFX) player

    [Header("Audio Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip[] level1MusicTracks; // Array for Level 1 music
    public AudioClip buttonClickSFX; // Button click sound effect

    [Header("UI Elements")]
    public Slider masterVolumeSlider; // Reference to the volume slider
    public Toggle musicToggle; // Toggle for enabling/disabling music
    public Toggle vibrationToggle; // Toggle for enabling/disabling vibration

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f); // Default to 50% if not set
        SetVolume(savedVolume);

        bool isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        musicToggle.isOn = isMusicEnabled;
        ToggleMusic(isMusicEnabled);

        bool isVibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        vibrationToggle.isOn = isVibrationEnabled;

        PlayMusicForScene(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;

        ApplyButtonClickSound();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(ToggleVibration);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🎵 Scene Loaded: {scene.name}");
        PlayMusicForScene(scene.name);
        ApplyButtonClickSound();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetVolume);
            masterVolumeSlider.value = GetVolume();
        }
        if (musicSource.clip == null)
        {
            Debug.LogError("⚠️ AudioManager: Music Source has no clip assigned!");
        }
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (sceneName == "Level 1")
        {
            PlayRandomLevel1Music();
        }
        Debug.Log($"🎵 Playing Music for Scene: {sceneName}");

        if (sceneName == "MainMenu")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (sceneName == "Level 1")
        {
            PlayRandomLevel1Music();
        }

        // Ensure music is playing
        if (!musicSource.isPlaying)
        {
            Debug.LogWarning("⚠️ Music source is not playing, forcing play.");
            musicSource.Play();
        }
    }

    void PlayRandomLevel1Music()
    {
        if (level1MusicTracks.Length == 0)
        {
            Debug.LogWarning("No Level 1 music tracks assigned!");
            return;
        }

        int randomIndex = Random.Range(0, level1MusicTracks.Length);
        PlayMusic(level1MusicTracks[randomIndex]);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
        sfxSource.volume = volume;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    }

    public void ToggleMusic(bool isEnabled)
    {
        musicSource.mute = !isEnabled;
        PlayerPrefs.SetInt("MusicEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyButtonClickSound()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(PlayButtonClickSFX);
            button.onClick.AddListener(PlayButtonClickSFX);
        }

        Debug.Log($"Applied button click sound to {buttons.Length} buttons in {SceneManager.GetActiveScene().name}");
    }

    void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }
   

}
