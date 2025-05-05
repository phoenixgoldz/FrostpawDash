using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

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
    [SerializeField] private Toggle vibrationToggle;

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
        LeaderboardViewer viewer = Object.FindFirstObjectByType<LeaderboardViewer>();
        if (viewer != null)
        {
            viewer.ForceRefreshLeaderboard(); // ✅ Just refresh data without showing
        }

        // Ensure LeaderboardUI is found safely
        LoadSettings();

        // Assign button listeners
        leaderboardButton?.onClick.AddListener(ToggleLeaderboard);
        optionsButton?.onClick.AddListener(OpenOptions);
        quitButton?.onClick.AddListener(QuitGame);
        creditsButton?.onClick.AddListener(ShowCredits);

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
        Debug.Log($"🛠️ Vibration Toggle Changed: {isEnabled} (PRE-check)");

        // Delay 1 frame to get accurate toggle state
        StartCoroutine(DelayedToggleWrite());
    }

    private IEnumerator DelayedToggleWrite()
    {
        yield return null; // Wait 1 frame

        bool actualToggleState = vibrationToggle.isOn; // Get the true current state

        Debug.Log($"📍 Accurate isOn value AFTER frame delay: {actualToggleState}");

        PlayerPrefs.SetInt("VibrationEnabled", actualToggleState ? 1 : 0);
        PlayerPrefs.Save();

        if (actualToggleState)
        {
            VibrationUtility.VibrateShort();
            Debug.Log("✅ Vibration toggled ON");
        }
        else
        {
            Debug.Log("🔇 Vibration toggled OFF");
        }
    }

    void DisplayGameVersion()
    {
        if (versionText != null)
        {
            versionText.text = $"Ver. {Application.version}";
        }
        else
        {
            Debug.LogError("❌ VersionText is not assigned in MainMenuUI!");
        }
    }
   public void PlayGame()
{
    string[] availableLevels = { "Level 1", "Level 3", "EasterLevel" };

    // Select one at random
    int index = Random.Range(0, availableLevels.Length);
    string chosenLevel = availableLevels[index];

    Debug.Log("🧊 Loading through LoadingManager: " + chosenLevel);

    // Trigger async loading through LoadingManager
    LoadingManager.LoadScene(chosenLevel);
}


    IEnumerator LoadSceneAsync(string sceneName)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 1.0f;

        while (!asyncLoad.isDone)
        {
            timer += Time.unscaledDeltaTime;

            if (asyncLoad.progress >= 0.9f && timer >= minLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public void ToggleLeaderboard()
    {
        Debug.Log("📊 Loading LeaderboardScreen...");
        SceneManager.LoadScene("LeaderboardScreen");
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
        // Before assigning value, remove listeners temporarily
        vibrationToggle.onValueChanged.RemoveAllListeners();

        bool vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        vibrationToggle.isOn = vibrationEnabled; // Will no longer trigger ToggleVibration prematurely

        // Re-add the listener AFTER setting value
        vibrationToggle.onValueChanged.AddListener(ToggleVibration);


        ApplyLoadedSettings();
    }

    void ApplyLoadedSettings()
    {
        AudioListener.volume = volumeSlider.value;
        QualitySettings.SetQualityLevel(graphicsDropdown.value, true);
    }

    void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions(); // ✅ wipe previous items
        graphicsDropdown.options = new List<TMP_Dropdown.OptionData>();

        string[] qualityLevels = QualitySettings.names;

        foreach (string level in qualityLevels)
        {
            graphicsDropdown.options.Add(new TMP_Dropdown.OptionData(level));
        }

        int savedIndex = PlayerPrefs.GetInt("GraphicsQuality", DetectBestQualityLevel());
        graphicsDropdown.value = savedIndex;
        graphicsDropdown.RefreshShownValue();

        graphicsDropdown.onValueChanged.RemoveAllListeners(); // clean up
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

        // Tiered detection for Android
        return (memory > 6000 && processorCores >= 6) ? 2 :
               (memory > 3000 && processorCores >= 4) ? 1 :
               0;
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
