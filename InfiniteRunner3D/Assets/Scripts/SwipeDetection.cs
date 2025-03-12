using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetection : MonoBehaviour
{
    public static SwipeDetection instance;

    public delegate void Swipe(Vector2 direction);
    public event Swipe swipePerformed;

    [SerializeField] private InputAction touchPosition, touchPress;
    private Vector2 initialTouchPos;
    private Vector2 currentTouchPos => touchPosition.ReadValue<Vector2>();

    private const float minSwipeDistance = 15f; // Lowered for instant jump response

    private void Awake()
    {
        instance = this;
        touchPosition.Enable();
        touchPress.Enable();

        touchPress.performed += OnTouchPressed;
        touchPress.canceled += OnTouchReleased;
    }

    private void OnDestroy()
    {
        touchPosition.Disable();
        touchPress.Disable();

        touchPress.performed -= OnTouchPressed;
        touchPress.canceled -= OnTouchReleased;
    }

    private void OnTouchPressed(InputAction.CallbackContext context)
    {
        initialTouchPos = currentTouchPos;
    }

    private void OnTouchReleased(InputAction.CallbackContext context)
    {
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        Vector2 delta = currentTouchPos - initialTouchPos;
        Vector2 swipeDirection = Vector2.zero;

        // **Ensure each swipe up is detected separately**
        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > minSwipeDistance)
        {
            swipeDirection.y = Mathf.Sign(delta.y); // 1 = Up, -1 = Down
        }
        else if (Mathf.Abs(delta.x) > minSwipeDistance)
        {
            swipeDirection.x = Mathf.Sign(delta.x); // -1 = Left, 1 = Right
        }

        if (swipeDirection != Vector2.zero)
        {
            Debug.Log($"Swipe detected: {swipeDirection}");
            swipePerformed?.Invoke(swipeDirection);

            // **Ensure every swipe up always triggers a jump instantly**
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null && swipeDirection.y > 0) // Only for up swipes
            {
                playerController.HandleSwipe(swipeDirection);
            }
        }
    }
}
