using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text distanceText;
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button resumeButton;
    public Button quitButton;

    private int lastCheckedDistance = -1; // To prevent multiple updates per frame
    private int score = 0;
    private float distanceTraveled = 0f;
    private bool isPaused = false;
    private AudioSource backgroundMusic;
    private GameObject player;
    private CharacterController characterController;
    private float playerStartZ;

    public static PlayerUIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScore(0);
        UpdateDistance(0);
        pauseButton.onClick.AddListener(TogglePauseMenu);
        resumeButton.onClick.AddListener(TogglePauseMenu);
        quitButton.onClick.AddListener(QuitGame);
        playerStartZ = GameObject.FindGameObjectWithTag("Player").transform.position.z;
        backgroundMusic = Object.FindFirstObjectByType<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (!isPaused && player != null)
        {
            // Calculate distance from start position
            float currentZ = player.transform.position.z;
            distanceTraveled = (currentZ - playerStartZ) / 5;

            //  Ensure distanceText updates correctly
            UpdateDistance(distanceTraveled);

            //  Increase score every meter
            int currentDistance = Mathf.FloorToInt(distanceTraveled);
            if (currentDistance != lastCheckedDistance)
            {
                int diff = currentDistance - lastCheckedDistance;
                lastCheckedDistance = currentDistance;
                UpdateScore(diff); // +1 point per meter
            }

        }
    }
    private int gemCount = 0;

    public void AddGem()
    {
        gemCount++;
        UpdateScore(5); // Each gem = 5 points
    }
    private int totalGems = 0;

    public int GetGemCount()
    {
        return gemCount; // this variable should be tracking total gems
    }

    public int GetScore()
    {
        return score;
    }

    public float GetDistance()
    {
        return distanceTraveled;
    }

    public void UpdateScore(int amount)
    {
        score += amount; //  Add points
        scoreText.text = " " + score; //  Update UI
    }



    public void UpdateDistance(float distance)
    {
        distanceText.text = Mathf.FloorToInt(distance) + "m"; // Ensure it displays whole numbers
    }


    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;

        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isPaused;
        }
    }
    public void QuitGame()
    {
        Time.timeScale = 1; // Resume game before loading menu
        SceneManager.LoadScene("MainMenu");

        // Restart Main Menu music
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene("MainMenu");
        }
    }
}
