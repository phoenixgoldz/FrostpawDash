using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;
    public float minLoadTime = 0.5f;

    private float progress = 0f;
    private int totalAssets = 0;
    private int loadedAssets = 0;

    void Start()
    {
        Debug.Log("✅ [LoadingManager] Start() called.");
        LoadPlayerPrefs();

        Debug.Log("📌 Manually calling LoadGameAssets()...");
        StartCoroutine(LoadGameAssets());
    }

    void LoadPlayerPrefs()
    {
        Debug.Log("🔹 Loading PlayerPrefs...");

        float volume = PlayerPrefs.GetFloat("Volume", 1.0f);
        int graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        float sensitivity = PlayerPrefs.GetFloat("ControlSensitivity", 1.0f);
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        AudioListener.volume = volume;
        QualitySettings.SetQualityLevel(graphicsQuality, true);

        Debug.Log("✅ PlayerPrefs Loaded Successfully!");
    }
    IEnumerator CheckForPatch()
    {
        string patchPath = Path.Combine(Application.persistentDataPath, "patch.unity3d");

        if (File.Exists(patchPath))
        {
            loadingText.text = "Applying update...";
            Debug.Log("🛠️ Patch file found. Applying...");

            // TODO: Load or extract contents of patch.unity3d here
            yield return new WaitForSeconds(2f); // Simulate time to apply patch

            // Optional: delete or archive patch file
            File.Delete(patchPath);
            Debug.Log("✅ Patch applied successfully.");
        }
        else
        {
            Debug.Log("📦 No patch file found.");
        }

        yield return null;
    }

    IEnumerator LoadGameAssets()
    {
        loadingText.text = "Checking for updates...";
        yield return StartCoroutine(CheckForPatch());

        yield return new WaitForSeconds(0.5f);
        loadingText.text = "Initializing...";

        yield return StartCoroutine(PreloadAssets());

        loadingText.text = "Loading Main Menu...";
        yield return StartCoroutine(LoadMainMenuScene());
    }
    IEnumerator LoadTexturesAsync()
    {
        yield return new WaitForSeconds(0.1f); // Simulate texture loading
                                               // Use Resources.LoadAsync for actual assets later
        loadedAssets++;
        UpdateProgress();
    }

    IEnumerator LoadAudioAsync()
    {
        yield return new WaitForSeconds(0.1f); // Simulate audio loading
        loadedAssets++;
        UpdateProgress();
    }

    IEnumerator PreloadAssets()
    {
        loadingText.text = "Preloading textures...";
        yield return LoadTexturesAsync();

        loadingText.text = "Loading audio...";
        yield return LoadAudioAsync();

        loadingText.text = "Almost ready...";
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator LoadTextures()
    {
        Debug.Log("📌 Preloading Textures...");
        loadedAssets++;
        yield return null;
    }

    IEnumerator LoadAudioClips()
    {
        Debug.Log("📌 Preloading Audio...");
        loadedAssets++;
        yield return null;
    }
    IEnumerator LoadMainMenuScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu");
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progressValue;
            loadingText.text = $"Loading... {Mathf.FloorToInt(progressValue * 100)}%";

            elapsedTime += Time.deltaTime;

            if (progressValue >= 1f && elapsedTime >= minLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    void UpdateProgress()
    {
        progress = (float)loadedAssets / totalAssets;
        loadingBar.value = progress;
        loadingText.text = $"Loading... {Mathf.FloorToInt(progress * 100)}%";
        Debug.Log($"🔄 Loading Progress: {progress * 100}%");
    }
}
