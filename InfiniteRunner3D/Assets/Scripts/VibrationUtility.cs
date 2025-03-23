using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator != null)
            {
                vibrator.Call("vibrate", 100);
            }
        }
#else
        Debug.Log("📳 Vibration would trigger here on Android.");
#endif
    }
}
