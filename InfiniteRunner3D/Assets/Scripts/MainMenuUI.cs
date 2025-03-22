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
    private LeaderboardUI leaderboardUI;

    public GameObject creditsPanel;

    [Header("UI Elements")]
    public Button leaderboardButton;
    public Button optionsButton;
    public Button quitButton;
    public Button creditsButton;

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

    [Header("Fading Animation")]
    public CanvasGroup menuCanvasGroup;
    public float fadeDuration = 1.5f; // Adjust fade speed

    void Start()
    {
        DisplayGameVersion();

        if (graphicsDropdown != null)
        {
            SetupGraphicsDropdown();
        }

        // Ensure LeaderboardUI is found safely
        leaderboardUI = Object.FindFirstObjectByType<LeaderboardUI>();
        LoadSettings();

        // Assign button listeners
        leaderboardButton?.onClick.AddListener(ToggleLeaderboard);
        optionsButton?.onClick.AddListener(OpenOptions);
        quitButton?.onClick.AddListener(QuitGame);
        creditsButton?.onClick.AddListener(ShowCredits);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Start the fade-in effect when the scene loads
        StartCoroutine(FadeInMenu());
    }
    IEnumerator FadeInMenu()
    {
        menuCanvasGroup.alpha = 0;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            menuCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        menuCanvasGroup.alpha = 1;
    }
    public void ToggleVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (isEnabled)
        {
            VibrationUtility.VibrateShort(); // Now it will work
        }
        else
        {
            Debug.Log("🔇 Vibration OFF");
        }
    }
    public void PlayEasterLevel()
    {
        StartCoroutine(LoadSceneAsync("EasterLevel"));
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
        StartCoroutine(LoadSceneAsync("Level 1"));
    }
    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Optional: Display a loading screen UI here (fade out, spinner, etc.)

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Optionally track asyncLoad.progress (0–0.9 while loading)
            if (asyncLoad.progress >= 0.9f)
            {
                // Optional delay, animations, or transitions before allowing activation
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
    public void ToggleLeaderboard()
    {
        if (leaderboardUI == null)
        {
            Debug.LogError("❌ Cannot open leaderboard: LeaderboardUI reference is missing.");
            return;
        }

        bool isActive = leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(!isActive);

        if (!isActive)
        {
            leaderboardUI.ShowLeaderboard(); // SAFE: Calls LeaderboardUI if it exists
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
