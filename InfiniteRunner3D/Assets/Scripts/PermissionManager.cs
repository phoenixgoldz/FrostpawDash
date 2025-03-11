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
        while (!Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
        {
            Debug.Log("🚨 Waiting for user to grant permission...");
            Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION");
            yield return new WaitForSeconds(2);
        }

        Debug.Log("✅ Permission granted! Game starting...");
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("🎮 Starting game...");
        // Example: SceneManager.LoadScene("Level1"); 
    }
}
