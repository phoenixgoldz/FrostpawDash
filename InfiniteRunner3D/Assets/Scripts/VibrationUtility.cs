using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Only vibrate if user has allowed it AND we have permission
            if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    // Request vibrator service
                    var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                    if (vibrator != null)
                    {
                        // Check hasVibrator() API to avoid crash on unsupported devices
                        bool hasVibrator = vibrator.Call<bool>("hasVibrator");

                        if (hasVibrator)
                        {
                            vibrator.Call("vibrate", 100);
                            Debug.Log("📳 Native Android vibration triggered");
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ Device does not support vibration");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Vibration service not found.");
                    }
                }
            }
            else
            {
                Debug.Log("🔇 Vibration disabled by PlayerPrefs.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Vibration exception: " + ex.Message);
        }
#else
        if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1)
        {
            Handheld.Vibrate();
            Debug.Log("📳 Editor fallback: Handheld.Vibrate()");
        }
#endif
    }
}
