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

    [Header("UI Elements")]
    public Slider volumeSlider;
    public TMP_Dropdown graphicsDropdown;
    public Slider sensitivitySlider;
    public Toggle musicToggle;
    public Toggle vibrationToggle;

    [Header("Saving UI")]
    public GameObject savingText;
    public GameObject savingIcon;

    private bool isSaving = false;

    void Start()
    {
        SetupGraphicsDropdown(); // Restored function for setting up graphics options
        LoadSettings(); // Load saved settings on start
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
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void OpenOptions()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
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
        Debug.Log("Settings Saved!");

        ApplyLoadedSettings();
        StartCoroutine(HideSavingUI());
    }

    void LoadSettings()
    {
        Debug.Log("Loading Settings...");

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
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
        Debug.Log("Graphics Quality Set: " + graphicsDropdown.value);
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
        Debug.Log("Graphics Quality Changed to: " + index);
    }

    int DetectBestQualityLevel()
    {
        int memory = SystemInfo.systemMemorySize;
        int processorCores = SystemInfo.processorCount;
        int gpuPerformance = (SystemInfo.graphicsShaderLevel >= 45) ? 2 : (SystemInfo.graphicsShaderLevel >= 30) ? 1 : 0;

        if (memory > 6000 && processorCores >= 6) return 2;
        if (memory > 3000 && processorCores >= 4) return 1;
        return 0;
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
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
