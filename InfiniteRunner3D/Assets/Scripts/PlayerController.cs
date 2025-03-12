using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;

    private bool isMoving = false;
    private bool isFalling = false;

    private bool stuck = false;

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
        StartCoroutine(EnsureGrounded());
    }

    IEnumerator EnsureGrounded()
    {
        yield return new WaitForSeconds(0.5f);
        if (IsGrounded())
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", true);
        }
    }
    void UpdateAnimatorParameters()
    {
        float fallingSpeed = rb.linearVelocity.y;
        Debug.Log($"🎭 FallingSpeed: {fallingSpeed}"); // Log value

        animator.SetFloat("FallingSpeed", fallingSpeed);
        animator.SetFloat("TurnSpeed", Mathf.Abs(shiftVelocity));
        animator.SetFloat("JumpHeight", transform.position.y);
    }


    void OnDestroy()
    {
        SwipeDetection.instance.swipePerformed -= context => HandleSwipe(context);

        playerControls.Disable();
    }

    void FixedUpdate() // Use FixedUpdate for physics-based movement
    {
        UpdateAnimatorParameters();
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

        if (!IsGrounded())
        {
            Debug.Log("❌ Character is NOT grounded!");
        }
        else
        {
            Debug.Log("✅ Character is grounded!");
        }
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
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("🏁 Landed! Resetting Falling & Running.");

            isJumping = false;
            isFalling = false;
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsRunning", true);
        }
    }

    void OnTriggerEnter(Collider other) { if (!other.gameObject.CompareTag("Obstacle")) stuck = true; }

    void OnTriggerExit(Collider other) { if (!other.gameObject.CompareTag("Obstacle")) stuck = false; }

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
    public void HandleSwipe(Vector2 swipeDirection)
    {
        Debug.Log($"🎮 Swipe input received: {swipeDirection}");

        if (Mathf.Abs(swipeDirection.y) > Mathf.Abs(swipeDirection.x)) // Prioritize vertical swipes
        {
            if (swipeDirection.y > 0)
            {
                Debug.Log("⬆️ Jumping...");
                Jump();
            }
            else
            {
                Debug.Log("⬇️ Sliding...");
                Slide();
            }
        }
        else
        {
            if (swipeDirection.x > 0)
            {
                Debug.Log("➡️ Moving Right...");
                ShiftHorizontally(1);
            }
            else
            {
                Debug.Log("⬅️ Moving Left...");
                ShiftHorizontally(-1);
            }
        }
    }

    void DetectTilt()
    {
        float tiltValue = currentTilt; // Get raw accelerometer X-axis value
        Debug.Log("Tilt Value: " + tiltValue);

        float tiltSensitivity = PlayerPrefs.GetFloat("ControlSensitivity", 1.0f); // Load saved sensitivity

        if (Mathf.Abs(tiltValue) > 0.05f)
        {
            float tiltShift = Mathf.Lerp(0, tiltValue * moveSpeed, Time.deltaTime * 2f);
            ShiftHorizontally(tiltShift);
        }
        else
        {
            ResetTurnAnimations();
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
        rb.linearVelocity = new Vector3(shiftVelocity, rb.linearVelocity.y, stuck ? speed * 0.5f : speed);
    }

    void Jump()
    {
        if (IsGrounded() && !isJumping)
        {
            Debug.Log("✅ Jump Triggered!");

            isJumping = true;
            isFalling = false;

            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsRunning", false);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Ensure jumping resets
            Invoke(nameof(ResetJump), 0.3f);
        }
        else
        {
            Debug.Log("❌ Jump ignored (Not grounded or already jumping).");
        }
    }

    void ResetJump()
    {
        if (IsGrounded())
        {
            Debug.Log("🏁 Resetting Jump");
            isJumping = false;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsRunning", true);
        }
    }

    void Slide()
    {
        if (IsGrounded() && !isSliding)
        {
            Debug.Log("✅ Slide Triggered!");

            isSliding = true;
            animator.SetTrigger("isSliding");

            // Reset sliding after 1 second
            Invoke(nameof(StopSliding), 1f);
        }
        else
        {
            Debug.Log("❌ Slide ignored (Not grounded or already sliding).");
        }
    }

    void StopSliding()
    {
        Debug.Log("🏁 Resetting Slide");
        isSliding = false;
        animator.SetBool("isSliding", false);
    }
    void ShiftHorizontally(float direction)
    {
        float laneWidth = 3f; // Adjust lane width if necessary
        float minX = -6f; // Minimum allowed X position
        float maxX = 6f;  // Maximum allowed X position

        // Target lane positions: Clamp between minX and maxX
        float targetX = Mathf.Clamp(transform.position.x + (direction * laneWidth), minX, maxX);

        // Smooth transition to new position
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetX, transform.position.y, transform.position.z), Time.deltaTime * 10f);

        // Update animation for turning
        if (direction > 0)
        {
            animator.SetBool("IsTurningLeft", false);
            animator.SetBool("IsTurningRight", true);
        }
        else if (direction < 0)
        {
            animator.SetBool("IsTurningLeft", true);
            animator.SetBool("IsTurningRight", false);
        }
        else
        {
            ResetTurnAnimations();
        }
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
    private bool IsGrounded()
    {
        float rayLength = 2.1f; // Adjust based on character height
        RaycastHit hit;

        bool grounded = Physics.Raycast(transform.position, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));

        Debug.DrawRay(transform.position, Vector3.down * rayLength, grounded ? Color.green : Color.red);

        if (!grounded)
        {
            Debug.LogWarning("⚠️ Character is NOT grounded!");
        }

        return grounded;
    }
}
