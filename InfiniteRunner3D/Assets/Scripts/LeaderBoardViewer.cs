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
        public int gems;
        public LeaderboardEntry(string name, int score, float distance, int gems)
        {
            this.name = name;
            this.score = score;
            this.distance = distance;
            this.gems = gems;
        }
    }

    void Start()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        EnsureLeaderboardDefaults(); 
    }
    void OnEnable()
    {
        Debug.Log("📊 [LeaderboardViewer] OnEnable called. Reloading leaderboard...");
        LoadLeaderboardData();  // Force fresh data from PlayerPrefs
        DisplayLeaderboard();
    }
    public void ShowLeaderboard()
    {
        Debug.Log("📊 Showing leaderboard...");
        if (leaderboardPanel == null || mainMenuPanel == null)
        {
            Debug.LogWarning("⚠️ leaderboardPanel or mainMenuPanel is null.");
            return;
        }

        leaderboardPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        LoadLeaderboardData(); // 👈 Ensure it's always fresh!
        DisplayLeaderboard();  // 👈 Force update text!
    }
    public void ForceRefreshLeaderboard()
    {
        Debug.Log("🔄 Force Refreshing leaderboard from PlayerPrefs...");
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
    void LoadLeaderboardData()
    {
        leaderboardEntries.Clear();

        for (int i = 0; i < MaxLeaderboardEntries; i++)
        {
            string name = PlayerPrefs.GetString($"Leaderboard_Name_{i}", "---");
            int score = PlayerPrefs.GetInt($"Leaderboard_Score_{i}", 0);
            float distance = PlayerPrefs.GetFloat($"Leaderboard_Distance_{i}", 0);
            int gems = PlayerPrefs.GetInt($"Leaderboard_Gems_{i}", 0); // ✅ load saved gems

            leaderboardEntries.Add(new LeaderboardEntry(name, score, distance, gems));
        }

        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
    }

    void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        leaderboardText.text = " Top 15 Players \n\n#  NAME  SCORE  DIST  GEMS\n";

        for (int i = 0; i < Mathf.Min(leaderboardEntries.Count, MaxLeaderboardEntries); i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardEntries[i].name}  {leaderboardEntries[i].score} pts  {Mathf.FloorToInt(leaderboardEntries[i].distance)}m  {leaderboardEntries[i].gems}\n";
        }

    }
}
