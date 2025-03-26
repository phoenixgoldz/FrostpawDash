
// Optimized LeaderboardUI.cs for LeaderboardScreen
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
    public TMP_Text leaderboardText;
    public TMP_InputField initialsInput;
    public GameObject enterInitialsPanel;
    public Button submitButton;
    public Button tryAgainButton;
    public Button mainMenuButton;
    private const int MaxLeaderboardEntries = 15;
    private bool hasSubmittedScore = false;

    public GameObject savingText;
    public GameObject savingIcon;

    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
    private int playerLastScore;
    private float playerLastDistance;
    private int highlightedIndex = -1;

    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public float distance;
        public int gems;
        public string title; // NEW

        public LeaderboardEntry(string name, int score, float distance, int gems, string title = "")
        {
            this.name = name;
            this.score = score;
            this.distance = distance;
            this.gems = gems;
            this.title = title;
        }
    }

    void Awake()
    {
        if (initialsInput == null)
            initialsInput = Object.FindFirstObjectByType<TMP_InputField>();

        if (leaderboardText == null)
            leaderboardText = Object.FindFirstObjectByType<TMP_Text>();

        if (leaderboardPanel == null)
            leaderboardPanel = this.gameObject;

        leaderboardPanel?.SetActive(false);
    }
    void Start()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false); // optional (safe default)

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
            initialsInput.onValueChanged.AddListener(ValidateInput);
            initialsInput.onSelect.AddListener(delegate { OpenKeyboard(); });
        }
        else
        {
            Debug.LogError("❌ LeaderboardPanel is missing! Assign it in the Inspector.");
        }

        EnsureLeaderboardDefaults();
        LoadLeaderboard();

        // Automatically show leaderboard when entering this scene
        ShowLeaderboard();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene("LeaderboardScreen");
        }
    }

    void OnEnable()
    {
        Debug.Log("📊 [LeaderboardUI] OnEnable called!");

        playerLastScore = PlayerPrefs.GetInt("LastScore", 0);
        playerLastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        LoadLeaderboard();
        DisplayLeaderboard();

        bool qualifies = CheckIfPlayerBeatsLeaderboard(playerLastScore);

        enterInitialsPanel.SetActive(qualifies);
        submitButton.gameObject.SetActive(qualifies);

        StartCoroutine(EnableInput());
    }


    public void TryAgain()
    {
        Debug.Log("🔄 Try Again Button Clicked!");

        string lastLevel = PlayerPrefs.GetString("LastPlayedLevel", "Level 1");

        if (string.IsNullOrEmpty(lastLevel))
        {
            Debug.LogWarning("⚠️ No LastPlayedLevel found in PlayerPrefs, loading default Level 1.");
            lastLevel = "Level 1";
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(lastLevel);
    }


    public void OpenKeyboard()
    {
#if UNITY_ANDROID
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
#endif
    }

    public void ValidateInput(string input)
    {
        initialsInput.text = input.ToUpper();
    }
    public void ShowLeaderboard()
    {
        Debug.Log("📊 Showing Leaderboard");

        playerLastScore = PlayerPrefs.GetInt("LastScore", 0);
        playerLastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        leaderboardPanel.SetActive(true);
        LoadLeaderboard();
        DisplayLeaderboard();

        // ✅ Always keep leaderboard visible
        if (leaderboardText != null)
            leaderboardText.gameObject.SetActive(true);

        // ✅ Only show input if they qualify AND haven't submitted
        bool qualifies = CheckIfPlayerBeatsLeaderboard(playerLastScore) && !hasSubmittedScore;

        enterInitialsPanel.SetActive(qualifies);
        submitButton.gameObject.SetActive(qualifies);
        submitButton.interactable = qualifies;

        tryAgainButton.interactable = true;
        mainMenuButton.interactable = true;

        StartCoroutine(DelayedEnableInput());

        LeaderboardViewer viewer = Object.FindFirstObjectByType<LeaderboardViewer>();
        if (viewer != null) viewer.ForceRefreshLeaderboard();
    }

    private IEnumerator DelayedEnableInput()
    {
        yield return new WaitForEndOfFrame();
        StartCoroutine(EnableInput());

        yield return new WaitForSeconds(0.05f);
        if (initialsInput != null)
        {
            EventSystem.current.SetSelectedGameObject(initialsInput.gameObject);
            initialsInput.ActivateInputField();
        }
    }

    IEnumerator EnableInput()
    {
        yield return new WaitForSeconds(0.1f);

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

        hasSubmittedScore = true; // ✅ Mark score as submitted

        enterInitialsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);

        StartCoroutine(SaveScore(playerInitials));
    }

    IEnumerator SaveScore(string playerInitials)
    {
        ShowSavingUI();
        highlightedIndex = leaderboardEntries.FindIndex(e => e.name == playerInitials && e.score == playerLastScore);

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
            int playerGems = PlayerPrefs.GetInt("LastGems", 0);
            leaderboardEntries.Add(new LeaderboardEntry(playerInitials, playerLastScore, playerLastDistance, playerGems));
        }

        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
        if (leaderboardEntries.Count > 15) leaderboardEntries.RemoveAt(15);

        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            PlayerPrefs.SetString($"Leaderboard_Name_{i}", leaderboardEntries[i].name);
            PlayerPrefs.SetInt($"Leaderboard_Score_{i}", leaderboardEntries[i].score);
            PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", leaderboardEntries[i].distance);
            PlayerPrefs.SetInt($"Leaderboard_Gems_{i}", leaderboardEntries[i].gems);
        }
        PlayerPrefs.Save();

        yield return new WaitForSeconds(3f);

        HideSavingUI();

        // Clear the input field and prevent panel from being re-shown
        initialsInput.text = "";

        // Reload leaderboard and show updates
        LoadLeaderboard();
        DisplayLeaderboard();

        // Lock the initials panel OFF after submit
        enterInitialsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);

        // Refresh external viewer if needed
        LeaderboardViewer viewer = Object.FindFirstObjectByType<LeaderboardViewer>();
        if (viewer != null)
            viewer.ForceRefreshLeaderboard();

        StartCoroutine(ClearHighlightAfterDelay(1f)); // fades after 1 sec

    }
    IEnumerator ClearHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        highlightedIndex = -1;
        DisplayLeaderboard(); // redraw without highlight
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
            PlayerPrefs.SetInt("Leaderboard_Initialized", 1);

            string[] defaultInitials = {
            "JEX", "TJK", "RYU", "KAT", "ZOE",
            "LUX", "PIX", "ASH", "BMO", "NIA",
            "REX", "VIK", "SKY", "GUS", "MIM"
        };

            float[] defaultDistances = {
            45f, 50f, 55f, 60f, 68f,
            73f, 78f, 84f, 89f, 94f,
            100f, 110f, 120f, 135f, 150f
        };

            int[] defaultGems = {
            4, 5, 6, 8, 10,
            11, 12, 14, 15, 16,
            18, 19, 21, 22, 25
        };
            string[] rankTitles = {
            "Chilly Cub",       // 15
            "Icy Hopper",       // 14
            "Frosty Sprinter",  // 13
            "Snow Scout",       // 12
            "Frozen Dancer",    // 11
            "Glacier Pouncer",  // 10
            "Winter Charger",   // 9
            "Blizzard Blazer",  // 8
            "Avalanche Ace",    // 7
            "Crystal Dasher",   // 6
            "Frostfang Flyer",  // 5
            "Moonlit Racer",    // 4
            "Aurora Strider",   // 3
            "Mystic Pouncer",   // 2
            "Crystal Champ"     // 1 (Top spot)
        };

            for (int i = 0; i < 15; i++)
            {
                string initials = defaultInitials[i];
                float distance = defaultDistances[i];
                int gems = defaultGems[i];
                int score = Mathf.FloorToInt(distance + (gems * 5));

                PlayerPrefs.SetString($"Leaderboard_Name_{i}", initials);
                PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", distance);
                PlayerPrefs.SetInt($"Leaderboard_Gems_{i}", gems);
                PlayerPrefs.SetInt($"Leaderboard_Score_{i}", score);
            }

            PlayerPrefs.Save();
            Debug.Log("🌟 Default leaderboard created with balanced arcade-style values.");
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

        leaderboardText.text += string.Format(
            "{0,-4} {1,-18} {2,-6} {3,6} {4,8} {5,6}\n",
            "#", "TIER", "NAME", "SCORE", "DIST", "GEMS"
        );
        leaderboardText.text += new string('-', 60) + "\n";

        string[] rankTitles = {
        "Chilly Cub", "Icy Hopper", "Frosty Sprinter", "Snow Scout", "Frozen Dancer",
        "Glacier Pouncer", "Winter Charger", "Blizzard Blazer", "Avalanche Ace",
        "Crystal Dasher", "Frostfang Flyer", "Moonlit Racer", "Aurora Strider",
        "Mystic Pouncer", "Crystal Champ"
    };

        int maxEntries = Mathf.Min(MaxLeaderboardEntries, leaderboardEntries.Count);
        for (int i = 0; i < maxEntries; i++)
        {
            LeaderboardEntry entry = leaderboardEntries[i];
            string tier = rankTitles[14 - i]; // Reverse order: top rank = Crystal Champ

            string line = string.Format(
                "{0,-4} {1,-18} {2,-6} {3,6} {4,8}  {5,5}\n",
                i + 1,
                tier,
                entry.name,
                entry.score,
                Mathf.FloorToInt(entry.distance),
                entry.gems
            );

            // Highlight the newly submitted score
            if (i == highlightedIndex)
                line = $"<color=#BC7AA0>{line}</color>";

            leaderboardText.text += line;
        }

        StartCoroutine(AnimateLeaderboardRefresh());
    }


    IEnumerator AnimateLeaderboardRefresh()
    {
        leaderboardText.alpha = 0;
        yield return new WaitForSeconds(0.1f);
        leaderboardText.alpha = 1;
    }

    bool CheckIfPlayerBeatsLeaderboard(int newScore)
    {
        if (leaderboardEntries.Count < 15) return true;

        foreach (LeaderboardEntry entry in leaderboardEntries)
        {
            if (newScore > entry.score) return true;
        }
        return false;
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("🏠 Main Menu Button Clicked!");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic(); // 👈 stops current music
            AudioManager.instance.PlayMusicForScene("MainMenu"); // 👈 plays MainMenu music if set up
        }
        // Stop the manually placed music
        AudioSource leaderboardMusic = Object.FindFirstObjectByType<AudioSource>();
        if (leaderboardMusic != null && leaderboardMusic.isPlaying)
        {
            leaderboardMusic.Stop();
        }

        SceneManager.LoadScene("MainMenu");

        PlayerPrefs.Save();
    }
}
