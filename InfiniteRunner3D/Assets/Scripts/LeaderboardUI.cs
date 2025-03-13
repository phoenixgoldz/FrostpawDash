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
    public void TryAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Level 1");

        //  Ensure Player UI reappears on restart
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene("Level 1");
        }
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");

        //  Ensure Player UI reappears in the main menu
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene("MainMenu");
        }
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }
    }
    public void SubmitScore()
    {
        string playerInitials = initialsInput.text.ToUpper();
        if (string.IsNullOrEmpty(playerInitials)) playerInitials = "AAA"; // Default if empty

        bool scoreUpdated = false;

        //  Check if initials already exist in leaderboard
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            if (leaderboardEntries[i].name == playerInitials)
            {
                //  If player already exists, update their best score & distance
                leaderboardEntries[i].score = Mathf.Max(leaderboardEntries[i].score, playerLastScore);
                leaderboardEntries[i].distance = Mathf.Max(leaderboardEntries[i].distance, playerLastDistance);
                scoreUpdated = true;
                break;
            }
        }

        //  If the player's initials are new, add them to the leaderboard
        if (!scoreUpdated)
        {
            leaderboardEntries.Add(new LeaderboardEntry(playerInitials, playerLastScore, playerLastDistance));
        }

        //  Sort leaderboard (highest score first)
        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));

        //  Keep only the top 15 entries
        if (leaderboardEntries.Count > 15) leaderboardEntries.RemoveAt(15);

        //  Save leaderboard data to PlayerPrefs
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            PlayerPrefs.SetString($"Leaderboard_Name_{i}", leaderboardEntries[i].name);
            PlayerPrefs.SetInt($"Leaderboard_Score_{i}", leaderboardEntries[i].score);
            PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", leaderboardEntries[i].distance);
        }
        PlayerPrefs.Save();

        Debug.Log("✅ Score submitted & saved.");

        //  Hide the initials input panel after submission
        enterInitialsPanel.SetActive(false);

        //  Keep Player UI hidden after submitting score
        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        //  Refresh leaderboard
        DisplayLeaderboard();
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

        LoadLeaderboard();
    }

    void ShowKeyboard()
    {
        Debug.Log("⌨️ Opening Android Keyboard...");
        initialsInput.ActivateInputField();
    }
    public void ShowLeaderboard()
    {
        Debug.Log("📌 Showing Leaderboard...");
        LoadLeaderboard(); // Ensure leaderboard is up to date

        playerLastScore = PlayerPrefs.GetInt("LastScore", 0);
        playerLastDistance = PlayerPrefs.GetFloat("LastDistance", 0);

        //  Hide Player UI when leaderboard appears
        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        //  Check if player qualifies for leaderboard
        bool qualifies = CheckIfPlayerBeatsLeaderboard(playerLastScore);

        //  Show or hide initials input field & submit button
        if (qualifies)
        {
            enterInitialsPanel.SetActive(true);
            submitButton.gameObject.SetActive(true);
        }
        else
        {
            enterInitialsPanel.SetActive(false);
            submitButton.gameObject.SetActive(false);
        }

        leaderboardPanel.SetActive(true);
        Time.timeScale = 0; // Pause game
        DisplayLeaderboard();
    }


    void DisplayLeaderboard()
    {
        // Ensure leaderboard text is cleared before appending
        leaderboardText.text = "LEADERBOARDS\n";

        // Sort leaderboard entries (highest score first)
        leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));

        // Display only the top 15 players in one text box
        int maxEntries = Mathf.Min(leaderboardEntries.Count, 15);
        for (int i = 0; i < maxEntries; i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardEntries[i].name} - {leaderboardEntries[i].score} pts - {Mathf.FloorToInt(leaderboardEntries[i].distance)} m\n";
        }
    }

    bool CheckIfPlayerBeatsLeaderboard(int newScore)
    {
        //  Always allow a new score if there are fewer than 15 entries
        if (leaderboardEntries.Count < 15) return true;

        //  Otherwise, check if newScore is higher than at least one existing score
        foreach (LeaderboardEntry entry in leaderboardEntries)
        {
            if (newScore > entry.score)
            {
                return true;
            }
        }
        return false;
    }

    void LoadLeaderboard()
    {
        if (!PlayerPrefs.HasKey("Leaderboard_Initialized"))
        {
            Debug.Log("🎮 First-time setup: Creating default leaderboard.");
            PlayerPrefs.SetInt("Leaderboard_Initialized", 1);

            for (int i = 0; i < 15; i++)
            {
                PlayerPrefs.SetString($"Leaderboard_Name_{i}", i == 0 ? "TJH" : "AAA");
                PlayerPrefs.SetInt($"Leaderboard_Score_{i}", i == 0 ? 500 : Random.Range(200, 480));
                PlayerPrefs.SetFloat($"Leaderboard_Distance_{i}", i == 0 ? 150f : Random.Range(50f, 140f));
            }
            PlayerPrefs.Save();
        }

        leaderboardEntries.Clear();
        for (int i = 0; i < 15; i++)
        {
            leaderboardEntries.Add(new LeaderboardEntry(
                PlayerPrefs.GetString($"Leaderboard_Name_{i}"),
                PlayerPrefs.GetInt($"Leaderboard_Score_{i}"),
                PlayerPrefs.GetFloat($"Leaderboard_Distance_{i}")
            ));
        }
    }
}
