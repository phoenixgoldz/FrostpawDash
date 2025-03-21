using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator != null)
            {
                AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", 500, 255); // 500ms, max strength
                vibrator.Call("vibrate", vibrationEffect);
            }
        }
#else
        Handheld.Vibrate(); // Works in editor or as fallback
#endif
    }
}
