﻿using UnityEngine;
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

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        if (AudioManager.instance != null)
        {
            audioSource.volume = AudioManager.instance.musicSource.volume;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            videoPlayer.targetTexture = renderTexture;
        }

        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
        }

        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += (VideoPlayer vp) =>
        {
            audioSource.Play();
            videoPlayer.Play();
        };

        videoPlayer.loopPointReached += OnVideoFinished;

        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;

        StartCoroutine(FadeIn());
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
        StartCoroutine(FadeOutAndLoadScene("LoadingScene")); // ✅ Fixed: Now loads LoadingScene correctly
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
