using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            if (vibrator != null)
            {
                vibrator.Call("vibrate", 100); // vibrate for 100ms
            }
        }
#else
        Debug.Log("📱 Android vibration triggered (native method fallback).");
#endif
    }
}
