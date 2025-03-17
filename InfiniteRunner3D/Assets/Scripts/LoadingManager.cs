using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;
    public float minLoadTime = 3f;

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

    IEnumerator LoadGameAssets()
    {
        Debug.Log("📌 LoadGameAssets() started! Running first step...");

        yield return new WaitForSeconds(1f);
        Debug.Log("✅ Step 1: LoadGameAssets() is still running... Proceeding to Preload Assets.");

        yield return StartCoroutine(PreloadAssets());
        Debug.Log("✅ Step 2: Finished Preloading Assets. Moving to scene loading.");

        yield return StartCoroutine(LoadMainMenuScene());
        Debug.Log("✅ Step 3: Scene loading complete!");
    }

    IEnumerator PreloadAssets()
    {
        Debug.Log("🔹 Preloading Game Assets...");
        totalAssets = 3;

        yield return StartCoroutine(LoadTextures());
        UpdateProgress();

        yield return StartCoroutine(LoadAudioClips());
        UpdateProgress();

        Debug.Log("📌 PreloadAssets() started!");
        yield return new WaitForSeconds(1f);

        Debug.Log("✅ All Assets Preloaded!");
        Debug.Log("✅ PreloadAssets() completed.");
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
        Debug.Log("📌 Loading MainMenu Scene...");

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
                Debug.Log("✅ Scene fully loaded. Activating...");
                operation.allowSceneActivation = true;
                yield break;
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
