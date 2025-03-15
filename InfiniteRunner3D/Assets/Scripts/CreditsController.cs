using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject creditsPanel;   // Assign Credits Panel
    public GameObject mainMenuPanel;  // Assign Main Menu Panel
    public AudioSource mainMenuMusic; // Assign Main Menu Music AudioSource
    public AudioSource creditsMusic;  // Assign Credits Music AudioSource
    public CanvasGroup canvasGroup;   // For fade effect
    public RectTransform creditsText; // Assign scrolling text
    public Button returnButton;       // Assign Return Button

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public float scrollSpeed = 80f; // Increased for smoother AAA-style scrolling
    public float musicFadeDuration = 2.5f;
    public float autoReturnDelay = 3f;

    private float startY;
    private float endY;
    private bool isScrolling = false;

    void Start()
    {
        startY = creditsText.anchoredPosition.y;
        endY = startY + creditsText.rect.height + 400; // Adjusted spacing for smooth scrolling

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    public void ShowCredits()
    {
        // First, activate the CreditsPanel to allow coroutines
        creditsPanel.SetActive(true);
        StartCoroutine(FadeInCredits());
    }

    IEnumerator FadeInCredits()
    {
        mainMenuPanel.SetActive(false);
        StopMainMenuMusic();
        PlayCreditsMusic();

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        isScrolling = true;
        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(0.5f); // Small delay to ensure UI updates

        // Ensure Credits Text starts from the bottom
        creditsText.anchoredPosition = new Vector2(creditsText.anchoredPosition.x, startY);

        // Force enable to ensure visibility
        creditsText.gameObject.SetActive(true);

        isScrolling = true;

        while (creditsText.anchoredPosition.y < endY && isScrolling)
        {
            creditsText.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(autoReturnDelay);
        StartCoroutine(FadeOutCredits());
    }

    IEnumerator FadeOutCredits()
    {
        isScrolling = false; // Stop scrolling
        StartCoroutine(FadeOutMusic());

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;

        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        PlayMainMenuMusic();
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = creditsMusic.volume;
        float elapsedTime = 0;

        while (elapsedTime < musicFadeDuration)
        {
            creditsMusic.volume = Mathf.Lerp(startVolume, 0, elapsedTime / musicFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        creditsMusic.volume = 0;
        creditsMusic.Stop();
    }
    public void ReturnToMainMenu()
    {
        Debug.Log("Return Button Clicked!"); // Debug log for testing

        StopAllCoroutines();
        isScrolling = false;

        creditsMusic.Stop();
        PlayMainMenuMusic();

        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        Debug.Log("✅ Switched back to Main Menu!");
    }

    private void StopMainMenuMusic()
    {
        if (mainMenuMusic != null && mainMenuMusic.isPlaying)
        {
            mainMenuMusic.Stop();
        }
    }

    private void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null && !mainMenuMusic.isPlaying)
        {
            mainMenuMusic.Play();
        }
    }

    private void PlayCreditsMusic()
    {
        if (creditsMusic != null)
        {
            creditsMusic.volume = 1f;
            creditsMusic.Play();
        }
    }
}
