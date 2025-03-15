using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject leaderboardPanel;
    public GameObject creditsPanel; // ✅ Added Credits Panel reference

    [Header("UI Elements")]
    public Button leaderboardButton;
    public Button optionsButton;
    public Button quitButton;
    public Button creditsButton; // ✅ Added Credits Button reference

    public Slider volumeSlider;
    public TMP_Dropdown graphicsDropdown;
    public Slider sensitivitySlider;
    public Toggle musicToggle;
    public Toggle vibrationToggle;

    [Header("Saving UI")]
    public GameObject savingText;
    public GameObject savingIcon;

    [Header("Misc UI Elements")]
    public TMP_Text versionText;

    private bool isSaving = false;
    private LeaderboardUI leaderboardUI;

    void Start()
    {
        DisplayGameVersion();

        if (graphicsDropdown != null)
        {
            SetupGraphicsDropdown();
        }

        LoadSettings();

        // ✅ Assign button listeners
        leaderboardButton?.onClick.AddListener(ToggleLeaderboard);
        optionsButton?.onClick.AddListener(OpenOptions);
        quitButton?.onClick.AddListener(QuitGame);
        creditsButton?.onClick.AddListener(ShowCredits); // ✅ Assign Credits Button function

        // ✅ Ensure panels are hidden at start
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        leaderboardUI = FindFirstObjectByType<LeaderboardUI>();
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
        if (leaderboardUI == null) return;

        bool isActive = leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(!isActive);

        if (!isActive)
        {
            leaderboardUI.ShowLeaderboard(); // Calls LeaderboardUI's method to update UI
        }
    }

    public void ShowCredits()
    {
        if (creditsPanel == null || menuPanel == null) return;

        menuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void OpenOptions()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        versionText?.gameObject.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
        versionText?.gameObject.SetActive(true);
    }

    public void ApplySettings()
    {
        if (isSaving) return;

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

        ApplyLoadedSettings();
        StartCoroutine(HideSavingUI());
    }

    void LoadSettings()
    {
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
    }

    void ChangeGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();
    }

    int DetectBestQualityLevel()
    {
        int memory = SystemInfo.systemMemorySize;
        int processorCores = SystemInfo.processorCount;
        return (memory > 6000 && processorCores >= 6) ? 2 : (memory > 3000 && processorCores >= 4) ? 1 : 0;
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
