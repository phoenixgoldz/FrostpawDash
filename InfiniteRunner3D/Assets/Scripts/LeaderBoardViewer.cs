using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardViewer : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject leaderboardPanel; // ✅ The leaderboard panel to show/hide
    public GameObject mainMenuPanel; // ✅ The main menu panel to return to
    public TMP_Text leaderboardText; // ✅ Single text box for leaderboard data

    private const int MaxLeaderboardEntries = 15; // ✅ Show top 15 players
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
        // Ensure the leaderboard panel is hidden at the start
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
        LoadLeaderboardData();
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel == null || mainMenuPanel == null) return;

        leaderboardPanel.SetActive(true);
        mainMenuPanel.SetActive(false); // Hide main menu panel when viewing leaderboard
        DisplayLeaderboard();
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel == null || mainMenuPanel == null) return;

        leaderboardPanel.SetActive(false);
        mainMenuPanel.SetActive(true); // Show main menu panel when exiting leaderboard
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

        // Sort by score (highest first)
        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
    }

    void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        leaderboardText.text = "🏆 Leaderboard 🏆\n\n";

        int maxEntries = Mathf.Min(leaderboardEntries.Count, MaxLeaderboardEntries);
        for (int i = 0; i < maxEntries; i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardEntries[i].name} - {leaderboardEntries[i].score} pts - {Mathf.FloorToInt(leaderboardEntries[i].distance)} m\n";
        }
    }
}
