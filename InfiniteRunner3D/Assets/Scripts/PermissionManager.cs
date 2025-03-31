using UnityEngine;
using UnityEngine.Android;
using System.Collections;

public class PermissionManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckPermissionsBeforeStart());
    }

    IEnumerator CheckPermissionsBeforeStart()
    {
        // 🔹 Request ACTIVITY_RECOGNITION permission
        while (!Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
        {
            Debug.Log("🚨 Waiting for user to grant ACTIVITY_RECOGNITION...");
            Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION");
            yield return new WaitForSeconds(2);
        }

        // 🔹 Request VIBRATE permission
        if (!Permission.HasUserAuthorizedPermission("android.permission.VIBRATE"))
        {
            Debug.Log("🚨 Requesting VIBRATE permission...");
            Permission.RequestUserPermission("android.permission.VIBRATE");
            yield return new WaitForSeconds(2);
        }

        Debug.Log("✅ All permissions granted! Game starting...");
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("🎮 Starting game...");
        // SceneManager.LoadScene("Level1"); // You can uncomment and load your level here
    }
}
