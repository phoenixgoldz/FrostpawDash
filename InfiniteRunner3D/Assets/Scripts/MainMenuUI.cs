using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject leaderboardPanel; // ✅ Leaderboard Panel

    [Header("UI Elements")]
    public Button leaderboardButton; // ✅ Reference to Leaderboard Button
    public Button optionsButton; // ✅ Reference to Options Button
    public Button quitButton; // ✅ Reference to Quit Button

    public Slider volumeSlider;
    public TMP_Dropdown graphicsDropdown;
    public Slider sensitivitySlider;
    public Toggle musicToggle;
    public Toggle vibrationToggle;

    [Header("Saving UI")]
    public GameObject savingText;
    public GameObject savingIcon;

    [Header("UI Elements")]
    public TMP_Text versionText;
    public TMP_Text leaderboardScoreText;
    public TMP_Text leaderboardDistanceText;

    private bool isSaving = false;

    void Start()
    {
        DisplayGameVersion();
        SetupGraphicsDropdown();
        LoadSettings();
        Debug.Log("🛠️ MainMenu Loaded - Checking AudioManager...");

        if (AudioManager.instance != null)
        {
            Debug.Log("✅ AudioManager exists, playing Main Menu Music...");
            AudioManager.instance.PlayMusicForScene("MainMenu");
        }
        else
        {
            Debug.LogError("❌ AudioManager NOT FOUND! Ensure it's in the MainMenu scene.");
        }

        // ✅ Assign button listeners
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(ToggleLeaderboard);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // ✅ Ensure leaderboard panel is hidden at start
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    void DisplayGameVersion()
    {
        if (versionText != null)
        {
            versionText.text = $"Version {Application.version}";
        }
        else
        {
            Debug.LogError("❌ VersionText is not assigned in MainMenuUI!");
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void ToggleLeaderboard()
    {
        if (leaderboardPanel == null) return;

        bool isActive = leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(!isActive);

        if (!isActive)
        {
            LoadLeaderboard();
        }
    }

    void LoadLeaderboard()
    {
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        float lastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        if (leaderboardScoreText != null)
            leaderboardScoreText.text = "Score: " + lastScore;

        if (leaderboardDistanceText != null)
            leaderboardDistanceText.text = "Distance: " + Mathf.FloorToInt(lastDistance) + "m";
    }

    public void OpenOptions()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);

        if (versionText != null)
        {
            versionText.gameObject.SetActive(false); // Hide version text
        }
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);

        if (versionText != null)
        {
            versionText.gameObject.SetActive(true); // Unhide version text
        }
    }

    public void ApplySettings()
    {
        if (isSaving) return;

        Debug.Log("Applying Settings...");
        isSaving = true;

        savingText.SetActive(true);
        savingIcon.SetActive(true);
        StartCoroutine(RotateSavingIcon());

        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsDropdown.value);
        PlayerPrefs.SetFloat("ControlSensitivity", sensitivitySlider.value);
        PlayerPrefs.SetInt("MusicEnabled", musicToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("VibrationEnabled", vibrationToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("✅ Settings Saved!");

        ApplyLoadedSettings();
        StartCoroutine(HideSavingUI());
    }

    void LoadSettings()
    {
        Debug.Log("📥 Loading Settings...");

        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", DetectBestQualityLevel());
        sensitivitySlider.value = PlayerPrefs.GetFloat("ControlSensitivity", 1.0f);
        musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;

        ApplyLoadedSettings();
    }

    void ApplyLoadedSettings()
    {
        AudioListener.volume = volumeSlider.value;
        QualitySettings.SetQualityLevel(graphicsDropdown.value, true);
        Debug.Log($"✅ Graphics Quality Set: {graphicsDropdown.value}");
    }

    void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();
        string[] qualityLevels = QualitySettings.names;
        int bestQualityLevel = DetectBestQualityLevel();

        foreach (string level in qualityLevels)
        {
            graphicsDropdown.options.Add(new TMP_Dropdown.OptionData(level));
        }

        graphicsDropdown.value = bestQualityLevel;
        graphicsDropdown.RefreshShownValue();
        graphicsDropdown.onValueChanged.AddListener(ChangeGraphicsQuality);

        Debug.Log($"📌 Graphics dropdown initialized. Best quality detected: {bestQualityLevel}");
    }

    void ChangeGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
        Debug.Log($"📢 Graphics Quality Changed to: {index}");
    }

    int DetectBestQualityLevel()
    {
        int memory = SystemInfo.systemMemorySize;
        int processorCores = SystemInfo.processorCount;
        int gpuPerformance = (SystemInfo.graphicsShaderLevel >= 45) ? 2 : (SystemInfo.graphicsShaderLevel >= 30) ? 1 : 0;

        int detectedLevel = 0;

        if (memory > 6000 && processorCores >= 6) detectedLevel = 2;
        else if (memory > 3000 && processorCores >= 4) detectedLevel = 1;
        else detectedLevel = 0;

        Debug.Log($"🖥️ System Specs - RAM: {memory}MB, Cores: {processorCores}, GPU Level: {gpuPerformance}");
        Debug.Log($"🔍 Auto-detected best quality: {detectedLevel}");
        
        return detectedLevel;
    }

    IEnumerator RotateSavingIcon()
    {
        while (savingIcon.activeSelf)
        {
            savingIcon.transform.Rotate(0, 0, -200 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator HideSavingUI()
    {
        yield return new WaitForSeconds(1.5f);
        savingText.SetActive(false);
        savingIcon.SetActive(false);
        isSaving = false;
        CloseOptions();
    }

    public void QuitGame()
    {
        Debug.Log("🚪 Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
