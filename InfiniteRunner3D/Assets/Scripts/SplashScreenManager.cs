using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    public RawImage videoDisplay;
    public RenderTexture renderTexture;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        videoPlayer.loopPointReached += OnVideoFinished;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;
    }

    void Start()
    {
        StartCoroutine(InitializeAudioAfterDelay(0.1f));
        StartCoroutine(PrepareVideoAfterDelay(0.2f));

        if (renderTexture != null)
        {
            renderTexture.Release(); // Clear previous frame data
            videoPlayer.targetTexture = renderTexture;
            videoDisplay.texture = renderTexture;
        }
        else
        {
            videoPlayer.targetTexture = null;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoDisplay.texture = videoPlayer.texture;
        }

        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted += (VideoPlayer vp) =>
        {
            audioSource.Play();
            videoPlayer.Play();
        };

        StartCoroutine(FadeIn());
    }

    IEnumerator PrepareVideoAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        videoPlayer.Prepare();
    }

    IEnumerator InitializeAudioAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (AudioManager.instance != null)
            audioSource.volume = AudioManager.instance.musicSource.volume;
    }

    IEnumerator FadeIn()
    {
        float t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            fadeCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(FadeOutAndLoadScene("LoadingScene")); // ✅ Transitions to loading screen
    }

    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
        SceneManager.LoadScene(sceneName);
    }
}
