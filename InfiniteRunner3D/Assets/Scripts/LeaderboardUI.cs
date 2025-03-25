using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public GameObject playerUI;
    public TMP_Text leaderboardText;
    public TMP_InputField initialsInput;
    public GameObject enterInitialsPanel;
    public Button submitButton;
    public Button tryAgainButton;
    public Button mainMenuButton;
    private const int MaxLeaderboardEntries = 15;

    // Added Saving UI elements
    public GameObject savingText;
    public GameObject savingIcon;

    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
    private int playerLastScore;
    private float playerLastDistance;

    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public float distance;
        public int gems; // ✅ NEW

        public LeaderboardEntry(string name, int score, float distance, int gems)
        {
            this.name = name;
            this.score = score;
            this.distance = distance;
            this.gems = gems;
        }
    }
    void Awake()
    {
        if (playerUI == null)
            playerUI = GameObject.Find("PlayerUI Canvas"); // or whatever its actual name is

        if (initialsInput == null)
            initialsInput = Object.FindFirstObjectByType<TMP_InputField>();

        if (leaderboardText == null)
            leaderboardText = Object.FindFirstObjectByType<TMP_Text>();

        if (leaderboardPanel == null)
            leaderboardPanel = this.gameObject;  // if the script is on the panel

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
       // DontDestroyOnLoad(this.gameObject);
        // Add others as needed
    }

    void Start()
    {

        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitScore);

        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (initialsInput != null)
        {
            initialsInput.characterLimit = 3;
            initialsInput.onSubmit.AddListener(delegate { SubmitScore(); });
            initialsInput.onEndEdit.AddListener(delegate { SubmitScore(); }); // ✅ New
            initialsInput.onValueChanged.AddListener(ValidateInput);
            initialsInput.onSelect.AddListener(delegate { OpenKeyboard(); });
        }
        else
        {
            Debug.LogError("❌ LeaderboardPanel is missing! Assign it in the Inspector.");
        }

        EnsureLeaderboardDefaults();
        LoadLeaderboard();
    }
    public void OpenKeyboard()
    {
#if UNITY_ANDROID
        Debug.Log("📱 Forcing Android Keyboard Open...");
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
#endif
    }


    public void ValidateInput(string input)
    {
        initialsInput.text = input.ToUpper(); // Convert input to uppercase
        Debug.Log("✍️ Player is typing: " + initialsInput.text); // Debug Log to confirm input
    }

    public void ShowLeaderboard()
    {
        if (playerUI == null || initialsInput == null)
        {
            Debug.LogWarning("🔁 Reassigning lost references...");

            playerUI = GameObject.Find("PlayerUI Canvas"); // or appropriate name
            initialsInput = Object.FindFirstObjectByType<TMP_InputField>();

            if (playerUI == null || initialsInput == null)
            {
                Debug.LogError("❌ Critical references still null! Leaderboard won't show correctly.");
                return;
            }
        }
        LoadLeaderboard();
        StartCoroutine(EnableInput());
        StartCoroutine(DelaySelectInitialsInput());

        IEnumerator DelaySelectInitialsInput()
        {
            yield return new WaitForEndOfFrame(); // Wait one frame to ensure UI is enabled

            if (initialsInput != null)
            {
                EventSystem.current.SetSelectedGameObject(initialsInput.gameObject);
                initialsInput.ActivateInputField(); // Optional: focus keyboard
            }
            else
            {
                Debug.LogError("❌ initialsInput is still null when trying to select it!");
            }
        }

        playerLastScore = PlayerPrefs.GetInt("LastScore", 0);
        playerLastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        playerUI.SetActive(false);

        bool qualifies = CheckIfPlayerBeatsLeaderboard(playerLastScore);
        enterInitialsPanel.SetActive(qualifies);
        submitButton.gameObject.SetActive(qualifies);

        leaderboardPanel.SetActive(true);
        tryAgainButton.interactable = true;
        mainMenuButton.interactable = true;
        submitButton.interactable = CheckIfPlayerBeatsLeaderboard(playerLastScore);

        PauseGameObjects(); // ✅ Stop movement without freezing UI
        DisplayLeaderboard();
        LeaderboardViewer viewer = Object.FindFirstObjectByType<LeaderboardViewer>();

        if (viewer != null)
        {
            viewer.ShowLeaderboard(); // Refresh main menu view too
        }
    }
    void PauseGameObjects()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }

        PathManager pathManager = Object.FindFirstObjectByType<PathManager>();
        if (pathManager != null)
        {
            pathManager.enabled = false;
        }

    }
    IEnumerator EnableInput()
    {
        yield return new WaitForSeconds(0.1f); // Let UI initialize

        if (initialsInput != null)
        {
            initialsInput.interactable = false;
            yield return new WaitForSeconds(0.1f);
            initialsInput.interactable = true;
            initialsInput.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(initialsInput.gameObject);
        }
        else
        {
            Debug.LogError("❌ initialsInput is null in EnableInput()");
        }
    }

    public void SubmitScore()
    {
        string playerInitials = initialsInput.text.ToUpper();
        if (string.IsNullOrEmpty(playerInitials)) playerInitials = "AAA";

        StartCoroutine(SaveScore(playerInitials));
    }
    IEnumerator SaveScore(string playerInitials)
    {
        ShowSavingUI();

        bool scoreUpdated = false;
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            if (leaderboardEntries[i].name == playerInitials)
            {
                leaderboardEntries[i].score = Mathf.Max(leaderboardEntries[i].score, playerLastScore);
                leaderboardEntries[i].distance = Mathf.Max(leaderboardEntries[i].distance, playerLastDistance);
                scoreUpdated = true;
                break;
            }
        }

        if (!scoreUpdated)
        {
            int playerGems = PlayerUIManager.Instance.GetGemCount();
            leaderboardEntries.Add(new LeaderboardEntry(playerInitials, playerLastScore, playerLastDistance, playerGems));
        }

        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
        if (leaderboardEntries.Count > 15) leaderboardEntries.RemoveAt(15);

        // Save to PlayerPrefs
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            PlayerPrefs.SetString($"Leaderboard_Name_{i}", leaderboardEntries[i].name);
            PlayerPrefs.SetInt($"Leaderboard_Score_{i}", leaderboardEntries[i].score);
            PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", leaderboardEntries[i].distance);
            PlayerPrefs.SetInt($"Leaderboard_Gems_{i}", leaderboardEntries[i].gems);
        }
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.1f); // optional delay

        HideSavingUI();

        // ✅ Now hide input field + buttons
        enterInitialsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);

        // ✅ Refresh leaderboard
        LoadLeaderboard();
        DisplayLeaderboard();

        // ✅ Update main menu panel
        LeaderboardViewer viewer = Object.FindFirstObjectByType<LeaderboardViewer>();
        if (viewer != null)
        {
            viewer.ShowLeaderboard();
        }
    }

    public IEnumerator ShowLeaderboardRoutine()
    {
        // Put your coroutine logic here
        yield return new WaitForSeconds(0.5f); // example delay
        Debug.Log("🌀 Coroutine in Leaderboard running!");
        // Reveal input field or animations
    }
    void ShowSavingUI()
    {
        if (savingText != null) savingText.SetActive(true);
        if (savingIcon != null)
        {
            savingIcon.SetActive(true);
            StartCoroutine(RotateSavingIcon());
        }
    }

    void HideSavingUI()
    {
        if (savingText != null) savingText.SetActive(false);
        if (savingIcon != null) savingIcon.SetActive(false);
    }

    IEnumerator RotateSavingIcon()
    {
        while (savingIcon.activeSelf)
        {
            savingIcon.transform.Rotate(0, 0, -200 * Time.deltaTime);
            yield return null;
        }
    }

    void EnsureLeaderboardDefaults()
    {
        if (!PlayerPrefs.HasKey("Leaderboard_Initialized"))
        {
            Debug.Log("🏆 First-time setup: Creating default leaderboard.");
            PlayerPrefs.SetInt("Leaderboard_Initialized", 1);

            for (int i = 0; i < 15; i++)
            {
                PlayerPrefs.SetString($"Leaderboard_Name_{i}", i == 0 ? "TJH" : "AAA");
                float distance = (i == 0) ? 180f : Random.Range(60f, 160f);
                int gems = (i == 0) ? 25 : Random.Range(8, 20);
                int score = Mathf.FloorToInt(distance + (gems * 5));
                PlayerPrefs.SetInt($"Leaderboard_Gems_{i}", gems);

                PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", distance);
                PlayerPrefs.SetInt($"Leaderboard_Score_{i}", score);

            }
            PlayerPrefs.Save();
        }
    }
    void LoadLeaderboard()
    {
        leaderboardEntries.Clear();

        for (int i = 0; i < 15; i++)
        {
            string name = PlayerPrefs.GetString($"Leaderboard_Name_{i}", "---");
            int score = PlayerPrefs.GetInt($"Leaderboard_Score_{i}", 0);
            float distance = PlayerPrefs.GetFloat($"Leaderboard_Distance_{i}", 0);
            int gems = PlayerPrefs.GetInt($"Leaderboard_Gems_{i}", 0);

            leaderboardEntries.Add(new LeaderboardEntry(name, score, distance, gems));
        }
    }

    void DisplayLeaderboard()
    {
        leaderboardText.text = "TOP 15 PLAYERS\n\n";
        leaderboardText.text += string.Format("{0,-3} {1,-4} {2,6} {3,6} {4,6}\n", "#", "NAME", "SCORE", "DIST", "GEMS");

        int maxEntries = Mathf.Min(MaxLeaderboardEntries, leaderboardEntries.Count);
        for (int i = 0; i < maxEntries; i++)
        {
            leaderboardText.text += string.Format("{0,-3} {1,-4} {2,6} {3,6}m {4,6}\n",
                i + 1,
                leaderboardEntries[i].name,
                leaderboardEntries[i].score,
                Mathf.FloorToInt(leaderboardEntries[i].distance),
                leaderboardEntries[i].gems);
        }

    }

    bool CheckIfPlayerBeatsLeaderboard(int newScore)
    {
        if (leaderboardEntries.Count < 15) return true;

        foreach (LeaderboardEntry entry in leaderboardEntries)
        {
            if (newScore > entry.score)
            {
                return true;
            }
        }
        return false;
    }
    public void TryAgain()
    {
        Debug.Log("🔄 Try Again Button Clicked!");
        ResumeGameObjects(); // ✅ Resume movement before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ReturnToMainMenu()
    {
        Debug.Log("🏠 Main Menu Button Clicked!");
        ResumeGameObjects();
        SceneManager.LoadScene("MainMenu");

        // ✅ Add this:
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene("MainMenu");
        }
    }

    void ResumeGameObjects()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = true;
        }

        PathManager pathManager = Object.FindFirstObjectByType<PathManager>();
        if (pathManager != null)
        {
            pathManager.enabled = true;
        }
    }
}
