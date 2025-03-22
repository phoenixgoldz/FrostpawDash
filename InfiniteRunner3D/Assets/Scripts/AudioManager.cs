using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip[] level1MusicTracks;
    public AudioClip buttonClickSFX;

    [Header("EasterLevel")]
    public AudioClip[] easterMusicTracks;

    [Header("UI Elements")]
    public Slider masterVolumeSlider;
    public Toggle musicToggle;
    public Toggle vibrationToggle;

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
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        SetVolume(savedVolume);

        bool isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (musicToggle != null) musicToggle.isOn = isMusicEnabled;
        ToggleMusic(isMusicEnabled);

        bool isVibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        if (vibrationToggle != null) vibrationToggle.isOn = isVibrationEnabled;

        PlayMusicForScene(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;

        ApplyButtonClickSoundToAll();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }


    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🎵 Scene Loaded: {scene.name}");
        PlayMusicForScene(scene.name);
        ApplyButtonClickSoundToAll();
    }
    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PlayMusicForScene(string sceneName)
    {
        StopMusic(); // 🛑 Ensure no overlap

        if (sceneName == "MainMenu")
            PlayMusic(mainMenuMusic);
        else if (sceneName == "Level 1")
            PlayRandomLevel1Music();
        else if (sceneName == "EasterLevel")
            PlayRandomMusic(easterMusicTracks);
    }
    void PlayRandomMusic(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        int index = Random.Range(0, clips.Length);
        PlayMusic(clips[index]);
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

    public void ToggleMusic(bool isEnabled)
    {
        musicSource.mute = !isEnabled;
        PlayerPrefs.SetInt("MusicEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }


    void ApplyButtonClickSoundToAll()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PlayButtonClickSFX());
        }
        Debug.Log($"Applied button click sound to {buttons.Length} buttons in {SceneManager.GetActiveScene().name}");
    }

    void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }
}