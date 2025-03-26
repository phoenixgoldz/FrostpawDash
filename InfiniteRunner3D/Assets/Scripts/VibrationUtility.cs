using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator != null)
                {
                    vibrator.Call("vibrate", 100);
                    Debug.Log("📳 Native Android vibration triggered");
                }
                else
                {
                    Debug.LogWarning("⚠️ Vibration service not found.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Vibration exception: " + ex.Message);
        }
#else
        Handheld.Vibrate();
        Debug.Log("📳 Editor fallback: Handheld.Vibrate()");
#endif
    }
}
