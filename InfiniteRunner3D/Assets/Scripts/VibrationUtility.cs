using UnityEngine;

public static class VibrationUtility
{
    public static void VibrateShort()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate(); // Simple Android-compatible vibration
#endif
    }
}
