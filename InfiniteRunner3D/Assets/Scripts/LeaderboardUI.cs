using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public GameObject playerUI;
    public TMP_Text leaderboardText;
    public TMP_InputField initialsInput;
    public GameObject enterInitialsPanel;
    public Button submitButton;

    // ✅ Added Saving UI elements
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

        public LeaderboardEntry(string name, int score, float distance)
        {
            this.name = name;
            this.score = score;
            this.distance = distance;
        }
    }

    void Start()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ LeaderboardPanel is missing! Assign it in the Inspector.");
        }

        EnsureLeaderboardDefaults();
        LoadLeaderboard();
    }

    public void ShowLeaderboard()  // ✅ FIXED: Re-added ShowLeaderboard() function
    {
        Debug.Log("📌 Showing Leaderboard...");
        LoadLeaderboard();  // Ensure the latest data is loaded

        playerLastScore = PlayerPrefs.GetInt("LastScore", 0);
        playerLastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        bool qualifies = CheckIfPlayerBeatsLeaderboard(playerLastScore);

        enterInitialsPanel.SetActive(qualifies);
        submitButton.gameObject.SetActive(qualifies);

        leaderboardPanel.SetActive(true);
        Time.timeScale = 0; // Pause the game
        DisplayLeaderboard();
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
            leaderboardEntries.Add(new LeaderboardEntry(playerInitials, playerLastScore, playerLastDistance));
        }

        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));

        if (leaderboardEntries.Count > 15) leaderboardEntries.RemoveAt(15);

        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            PlayerPrefs.SetString($"Leaderboard_Name_{i}", leaderboardEntries[i].name);
            PlayerPrefs.SetInt($"Leaderboard_Score_{i}", leaderboardEntries[i].score);
            PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", leaderboardEntries[i].distance);
        }
        PlayerPrefs.Save();

        Debug.Log("✅ Score submitted & saved.");

        yield return new WaitForSeconds(1.5f);

        HideSavingUI();

        enterInitialsPanel.SetActive(false);
        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        LoadLeaderboard();
        DisplayLeaderboard();
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
                PlayerPrefs.SetInt($"Leaderboard_Score_{i}", i == 0 ? 500 : Random.Range(200, 480));
                PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", i == 0 ? 150f : Random.Range(50f, 140f));
            }
            PlayerPrefs.Save();
        }
    }

    void LoadLeaderboard()
    {
        leaderboardEntries.Clear();
        for (int i = 0; i < 15; i++)
        {
            leaderboardEntries.Add(new LeaderboardEntry(
                PlayerPrefs.GetString($"Leaderboard_Name_{i}", "---"),
                PlayerPrefs.GetInt($"Leaderboard_Score_{i}", 0),
                PlayerPrefs.GetFloat($"Leaderboard_Distance_{i}", 0)
            ));
        }
    }

    void DisplayLeaderboard()
    {
        leaderboardText.text = "LEADERBOARDS\n";
        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));

        int maxEntries = Mathf.Min(leaderboardEntries.Count, 15);
        for (int i = 0; i < maxEntries; i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardEntries[i].name} - {leaderboardEntries[i].score} pts - {Mathf.FloorToInt(leaderboardEntries[i].distance)} m\n";
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
}
