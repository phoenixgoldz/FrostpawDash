using UnityEngine;

public class GemCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerUIManager.Instance.UpdateScore(5);

            // Preserve and restore speed
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Debug.Log($"🔄 Before Destroy: Player Speed = {playerRb.linearVelocity.z}");

                float speedBeforeCollecting = 5f; // Enforce constant speed
                Destroy(gameObject);

                // Restore speed immediately
                playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, playerRb.linearVelocity.y, speedBeforeCollecting);

                Debug.Log($"🚀 After Destroy: Player Speed Restored = {playerRb.linearVelocity.z}");
            }
        }
    }
}
