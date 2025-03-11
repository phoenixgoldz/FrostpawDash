using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetection : MonoBehaviour
{
    public static SwipeDetection instance;

    public delegate void Swipe(Vector2 direction);
    public event Swipe swipePerformed;

    [SerializeField] private InputAction touchPosition, touchPress;

    [SerializeField] private float swipeResistance;
    [SerializeField] private Vector2 initialTouchPos;
    [SerializeField] private Vector2 currentTouchPos => touchPosition.ReadValue<Vector2>();


    private void Awake()
    {
        instance = this;

        touchPosition.Enable();
        touchPress.Enable();

        touchPress.performed += _ => { initialTouchPos = currentTouchPos; };
        touchPress.canceled += _ => DetectSwipe();

        swipeResistance = Screen.width / 6;
    }

    private void OnDestroy()
    {
        touchPosition.performed -= _ => { initialTouchPos = currentTouchPos; };
        touchPress.canceled -= _ => DetectSwipe();
    }

    private void DetectSwipe()
    {
        Debug.Log($"Initial Touch: {initialTouchPos}, Current Touch: {currentTouchPos}");
    Vector2 delta = currentTouchPos - initialTouchPos;
    Vector2 swipeDirection = Vector2.zero;

    if (Mathf.Abs(delta.x) > swipeResistance)
        swipeDirection.x = Mathf.Clamp(delta.x, -1, 1);

    if (Mathf.Abs(delta.y) > swipeResistance)
        swipeDirection.y = Mathf.Clamp(delta.y, -1, 1);

    if (swipeDirection != Vector2.zero)
    {
        Debug.Log("Swipe detected: " + swipeDirection);
        swipePerformed?.Invoke(swipeDirection);
    }
    }

}
