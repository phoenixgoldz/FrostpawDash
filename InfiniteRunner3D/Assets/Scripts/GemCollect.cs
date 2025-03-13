using UnityEngine;

public class GemCollect : MonoBehaviour
{
    public AudioClip gemCollectSFX;

    void Start()
    {
        // Ensure Gems spawn within adjusted range (-3.5f to 3.33f)
        Vector3 gemPosition = transform.position;
        gemPosition.x = Random.Range(-3.5f, 3.33f);
        transform.position = gemPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerUIManager.Instance.UpdateScore(5);

            if (AudioManager.instance != null && gemCollectSFX != null)
            {
                AudioManager.instance.PlaySFX(gemCollectSFX);
            }

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
