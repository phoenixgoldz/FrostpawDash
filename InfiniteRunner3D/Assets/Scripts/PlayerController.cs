using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    private bool isMoving = false;
    private bool isFalling = false;

    public float speed = 10f;
    public float turnSpeed = 5f;
    public float jumpForce = 10f;

    public float moveSpeed = 3f;
    public float pathWidth = 4f;

    private float currentKeyboardShift = 0;
    private float shiftVelocity = 0;

    private bool isJumping = false;
    private bool isSliding = false;

    private Vector2 startTouchPosition, swipeDelta;

    public InputActionMap playerControls;

    // Keyboard Controls
    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction shiftAction;

    // Touch Controls
    private InputAction tiltAction;
    private float currentTilt => tiltAction.ReadValue<float>();


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Prevents Constraint Issues (hopefully)
        // Thank you https://discussions.unity.com/t/rigibody-constraints-do-not-work-still-moves-a-little/205580, you are amazing
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;

        CreateKeyboardControls();
        AddMobileControls();

        playerControls.Enable();
    }

    void OnDestroy()
    {
        SwipeDetection.instance.swipePerformed -= context => HandleSwipe(context);

        playerControls.Disable();
    }

    void FixedUpdate() // Use FixedUpdate for physics-based movement
    {
        if (isMoving)
        {
            // Move Player forward
            MovePlayer();
        }

        // ✅ Detect when player is in the air (falling)
        if (rb.linearVelocity.y < -0.1f && !IsGrounded()) // Falling downwards
        {
            isFalling = true;
            animator.SetBool("IsFalling", true);
        }

        // ✅ Detect when player lands
        if (isFalling && IsGrounded())
        {
            isFalling = false;
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false); // Ensure jumping resets too
        }

        // Check if Player falls off
        if (transform.position.y < -5f) Die();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PathTrigger"))
        {
            isMoving = true;
            Debug.Log("✅ Player touched path! Movement activated.");
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("❌ Hit an Obstacle!");
            Die();
        }

        // ✅ Detect landing from a jump
        if (collision.gameObject.CompareTag("PathTrigger"))
        {
            Debug.Log("🏁 Landed! Resetting Falling & Running.");
            isJumping = false;
            isFalling = false; // Immediately reset falling state
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", true);
        }
    }

    private void CreateKeyboardControls()
    {
        if (playerControls.FindAction("Jump") == null) jumpAction = playerControls.AddAction("Jump", binding: "<Keyboard>/Space", interactions: "press");
        else jumpAction = playerControls.FindAction("Jump");
        jumpAction.performed += _ => Jump();

        if (playerControls.FindAction("Slide") == null) slideAction = playerControls.AddAction("Slide", binding: "<Keyboard>/s", interactions: "press");
        else slideAction = playerControls.FindAction("Slide");
        slideAction.performed += _ => Slide();

        if (playerControls.FindAction("Shift Horizontally") == null)
        {
            shiftAction = playerControls.AddAction("Shift Horizontally", interactions: "hold");
            shiftAction.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            shiftAction.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
        }
        else shiftAction = playerControls.FindAction("Shift Horizontally");
        shiftAction.started += context => currentKeyboardShift = context.ReadValue<float>();
        shiftAction.canceled += _ => { currentKeyboardShift = 0; shiftVelocity = 0; };
    }

    private void AddMobileControls()
    {
        SwipeDetection.instance.swipePerformed += context => HandleSwipe(context);

        // Enable Gyro Sensors
        // Thank you https://discussions.unity.com/t/tutorial-for-input-system-and-accelerometer-gyro/790202/2, extremely appreciated
        if (UnityEngine.InputSystem.Gyroscope.current != null) InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);

        if (playerControls.FindAction("Tilt") == null) tiltAction = playerControls.AddAction("Tilt", binding: "<Accelerometer>/acceleration/x");
    }

    void HandleSwipe(Vector2 swipeDirection)
    {
        // print("Screen Swiped");

        if (swipeDirection.y == 1) Jump();
        else if (swipeDirection.y == -1) Slide();
    }

    void DetectTilt()
    {
        Debug.Log("Tilt Value: " + currentTilt); // Add this for debugging
    if (Mathf.Abs(currentTilt) > 0.2f) // Try reducing threshold
    {
        ShiftHorizontally(currentTilt);
    }
    }

    void MovePlayer()
    {
        shiftVelocity = 0;

        // Detect Phone Tilt
        DetectTilt();

        // Detect Keyboard Horizontal Input (Band-aid solution, bear with me please)
        if (currentKeyboardShift != 0) ShiftHorizontally(currentKeyboardShift);

        // Move Player
        rb.linearVelocity = new Vector3(shiftVelocity, rb.linearVelocity.y, speed);
    }

    void Jump()
    {
        if (IsGrounded() && !isJumping)
        {
            isJumping = true;
            isFalling = false;

            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", false);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // ✅ Invoke ResetJump faster
            Invoke(nameof(ResetJump), 0.3f);
        }
    }

    void ResetJump()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);

        // ✅ Check if player is still in air to transition to falling
        if (!IsGrounded())
        {
            animator.SetBool("IsFalling", true);
        }
    }

    void Slide()
    {
        if (!isSliding)
        {
            isSliding = true;
            animator.SetTrigger("IsSliding");
            Invoke(nameof(StopSliding), 1f);
        }
    }

    void StopSliding() { isSliding = false; }

    void ShiftHorizontally(float direction)
    {
        if ((transform.position.x > -pathWidth || direction > 0) && (transform.position.x < pathWidth || direction < 0)) shiftVelocity = direction * moveSpeed;

        // ✅ Adjust animations for left/right movement
        if (direction > 0)
        {
            animator.SetBool("IsTurningLeft", false);
            animator.SetBool("IsTurningRight", true);
        }
        if (direction < 0)
        {
            animator.SetBool("IsTurningLeft", true);
            animator.SetBool("IsTurningRight", false);
        }

        Invoke(nameof(ResetTurnAnimations), 0.3f);
    }

    void ResetTurnAnimations()
    {
        animator.SetBool("IsTurningLeft", false);
        animator.SetBool("IsTurningRight", false);
    }

    void Die()
    {
        Debug.Log("❌ Player died! Saving score & transitioning to leaderboard...");
        // SceneManager.LoadScene("Leaderboard");

        SceneManager.LoadScene("MainMenu");  // Redirects to the Main Menu, for now
    }

    // Thank you https://discussions.unity.com/t/how-can-i-check-if-my-rigidbody-player-is-grounded/256346, this is very nice
    bool IsGrounded() { return Physics.Raycast(transform.position, Vector3.down, 0.6f); }
}
