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

    public TMP_Text levelNameText;
    public Image backgroundImage;

    [Header("Loading Backgrounds")]
    public Sprite crystalCavernsBG;
    public Sprite frozenTundraBG;
    public Sprite easterLevelBG;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
    void Start()
    {
        Debug.Log("✅ [LoadingManager] Start() called.");
        LoadPlayerPrefs();

        if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad != "MainMenu")
        {
            Debug.Log($"🎮 Loading gameplay scene: {sceneToLoad}");
            SetLevelVisuals(sceneToLoad); 
            StartCoroutine(LoadTargetScene());
        }

        else
        {
            bool alreadyCached = PlayerPrefs.GetInt("AssetsPreloaded", 0) == 1;
            if (alreadyCached)
            {
                Debug.Log("⚡ Skipping Preload (Cached).");
                StartCoroutine(LoadMainMenuScene());
            }
            else
            {
                Debug.Log("📌 First load - Preloading assets...");
                StartCoroutine(LoadGameAssets());
            }
        }

    }
    IEnumerator LoadTargetScene()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progressValue;
            loadingText.text = $"Loading {sceneToLoad}... {Mathf.FloorToInt(progressValue * 100)}%";

            elapsedTime += Time.deltaTime;

            if (progressValue >= 1f && elapsedTime >= minLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
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
    void SetLevelVisuals(string levelName)
    {
        if (levelNameText != null)
        {
            switch (levelName)
            {
                case "Level 1":
                    levelNameText.text = "❄️ Entering Crystal Caverns...";
                    backgroundImage.sprite = crystalCavernsBG;
                    break;

                case "Level 3":
                    levelNameText.text = "🌌 Entering Frozen Twilight Tundra...";
                    backgroundImage.sprite = frozenTundraBG;
                    break;

                case "EasterLevel":
                    levelNameText.text = "🐣 Entering Easter Grove...";
                    backgroundImage.sprite = easterLevelBG;
                    break;

                default:
                    levelNameText.text = $"Loading {levelName}...";
                    break;
            }
        }
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
        loadingText.text = "Loading audio...";

        string[] musicPaths = new string[]
        {
        "Music/EasterMusic/easter-bunny-290978",
        "Music/EasterMusic/easter-is-coming-146181",
        "Music/EasterMusic/spring-adventure-198193",
        "Music/EasterMusic/the-first-days-of-spring-188392",
        "Music/Crystal Caves/cold-crystal-caverns-252784",
        "Music/Crystal Caves/fantasy-267185",
        "Music/Crystal Caves/magical-fantasy-world-version-1-199339",
        "Music/Aequinoctium-JuliusH",
        "Music/Epic - SigmaMusicArt",
        "Music/Fantasy Arcadium MonumentMusic",
        "Music/party-game-bgm-vol2-242467"
        };

        foreach (string path in musicPaths)
        {
            ResourceRequest request = Resources.LoadAsync<AudioClip>(path);
            yield return request;

            if (request.asset != null)
            {
                Debug.Log($"🎵 Loaded audio: {path}");
            }
            else
            {
                Debug.LogWarning($"❌ Failed to load audio at: {path}");
            }

            loadedAssets++;
            UpdateProgress();
        }

        loadingText.text = "Audio ready!";
    }
    IEnumerator PreloadAssets()
    {
        loadingText.text = "Preloading models and prefabs...";

        string[] foldersToPreload = new string[]
        {
        "Models/Collectibles/Gem",
        "Models/Crystal Caverns/BlueCrystals",
        "Models/Crystal Caverns/BlueIceFloor",
        "Models/Crystal Caverns/CavernWall",
        "Models/Crystal Caverns/CavernWallModel",
        "Models/Crystal Caverns/Crystal Caverns Floor",
        "Models/Crystal Caverns/Crystal Wall",
        "Models/Crystal Caverns/FlatIcePath",
        "Models/Crystal Caverns/Floating Ice Block",
        "Models/Crystal Caverns/Ice Archway Model",
        "Models/Crystal Caverns/Ice Bridge",
        "Models/Crystal Caverns/IceCrystalWall",
        "Models/Crystal Caverns/StartingLocationCrystals",
        "Models/EasterMap/Environment",
        "Models/EasterMap/EasterArchway",
        "Models/EasterMap/EasterGems",
        "Models/EasterMap/stylized-floor-seamless-texture-freeeee",
        "Models/EasterMap/Walls",
        "Models/Interactables",
        "Models/RainbowTiger"
        };

        totalAssets = foldersToPreload.Length;

        foreach (string folder in foldersToPreload)
        {
            Object[] loaded = Resources.LoadAll(folder);
            Debug.Log($"📦 Preloaded {loaded.Length} assets from {folder}");
            loadedAssets++;
            UpdateProgress();
            yield return null;
        }

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
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null;

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
    public static string sceneToLoad;


    public static void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        SceneManager.LoadScene("LoadingScene"); // this scene shows loading UI
    }
    void UpdateProgress()
    {
        progress = (float)loadedAssets / totalAssets;
        loadingBar.value = progress;
        loadingText.text = $"Loading... {Mathf.FloorToInt(progress * 100)}%";
        Debug.Log($"🔄 Loading Progress: {progress * 100}%");
    }
}
