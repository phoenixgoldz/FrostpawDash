using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardViewer : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public GameObject mainMenuPanel;
    public TMP_Text leaderboardText;

    private const int MaxLeaderboardEntries = 15;
    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();

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

        EnsureLeaderboardDefaults(); // ✅ Ensure leaderboard initializes properly
        LoadLeaderboardData();
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel == null || mainMenuPanel == null) return;

        leaderboardPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        LoadLeaderboardData();
        DisplayLeaderboard();
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel == null || mainMenuPanel == null) return;

        leaderboardPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    void EnsureLeaderboardDefaults()
    {
        if (!PlayerPrefs.HasKey("Leaderboard_Initialized"))
        {
            Debug.Log("🏆 First-time setup: Creating default leaderboard.");
            PlayerPrefs.SetInt("Leaderboard_Initialized", 1);

            for (int i = 0; i < MaxLeaderboardEntries; i++)
            {
                PlayerPrefs.SetString($"Leaderboard_Name_{i}", i == 0 ? "TJH" : "AAA");
                PlayerPrefs.SetInt($"Leaderboard_Score_{i}", i == 0 ? 500 : Random.Range(200, 480));
                PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", i == 0 ? 150f : Random.Range(50f, 140f));
            }
            PlayerPrefs.Save();
        }
    }

    void LoadLeaderboardData()
    {
        leaderboardEntries.Clear();

        for (int i = 0; i < MaxLeaderboardEntries; i++)
        {
            string playerInitials = PlayerPrefs.GetString($"Leaderboard_Name_{i}", "---");
            int playerScore = PlayerPrefs.GetInt($"Leaderboard_Score_{i}", 0);
            float playerDistance = PlayerPrefs.GetFloat($"Leaderboard_Distance_{i}", 0);

            leaderboardEntries.Add(new LeaderboardEntry(playerInitials, playerScore, playerDistance));
        }

        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
    }

    void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        leaderboardText.text = " Top 15 Players \n\n";

        int maxEntries = Mathf.Min(leaderboardEntries.Count, MaxLeaderboardEntries);
        for (int i = 0; i < maxEntries; i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardEntries[i].name} - {leaderboardEntries[i].score} pts - {Mathf.FloorToInt(leaderboardEntries[i].distance)} m\n";
        }
    }
}
