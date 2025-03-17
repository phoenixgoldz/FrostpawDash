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
    private bool isSliding = false;
    private bool isJumping = false;

    private bool stuck = false;
    public float speed = 10f;
    public float turnSpeed = 5f;
    public float jumpForce = 10f;

    public float moveSpeed = 3f;
    public float pathWidth = 4f;

    private float currentKeyboardShift = 0;
    private float shiftVelocity = 0;

    private Vector2 startTouchPosition, swipeDelta;

    public InputActionMap playerControls;

    // Keyboard Controls
    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction shiftAction;

    // Audio
    private AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip hitObstacleSFX;

    // Touch Controls
    private InputAction tiltAction;
    private float currentTilt => tiltAction.ReadValue<float>();

    void Start()
    {
        // Ensure the player starts at Y = 0
        Vector3 playerStartPosition = transform.position;
        playerStartPosition.y = 0.5f; // Slightly above the floor to prevent clipping
        transform.position = playerStartPosition;
        audioSource = GetComponent<AudioSource>(); // Get AudioSource component

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Ensure the Rigidbody doesn't sink into the floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;

        CreateKeyboardControls();
        AddMobileControls();

        playerControls.Enable();
        StartCoroutine(EnsureGrounded());

    }
    public float GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
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
    {    // Always maintain constant forward movement speed
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveSpeed); // Ensure consistent forward speed

        UpdateAnimatorParameters();
        if (isMoving)
        {
            // Move Player forward
            MovePlayer();
        }

        //  Detect when player is in the air (falling)
        if (rb.linearVelocity.y < -0.1f && !IsGrounded()) // Falling downwards
        {
            isFalling = true;
            animator.SetBool("IsFalling", true);
        }

        //  Detect when player lands
        if (isFalling && IsGrounded())
        {
            isFalling = false;
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false); // Ensure jumping resets too
        }

        // Check if Player falls off
        if (transform.position.y < -5f) Die();
/*
        if (!IsGrounded())
        {
            Debug.Log("❌ Character is NOT grounded!");
        }
        else
        {
            Debug.Log("✅ Character is grounded!");
        }*/
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

            LevelAudioManager.instance.PlaySound(hitObstacleSFX);

            Die();
        }


        if (collision.gameObject.CompareTag("PathTrigger"))
        {
            Debug.Log("🏁 Landed! Resetting Falling & Running.");

            isJumping = false;
            isFalling = false;
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsRunning", true);
        }
    }

    void OnTriggerEnter(Collider other) { if (other.gameObject.CompareTag("PathTrigger")) stuck = true; }

    void OnTriggerExit(Collider other) { if (other.gameObject.CompareTag("PathTrigger")) stuck = false; }

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
        float tiltValue = Input.acceleration.x; // Read from the accelerometer
        float tiltSensitivity = PlayerPrefs.GetFloat("ControlSensitivity", 1.0f); // Allow customization

        Debug.Log($"Tilt Value: {tiltValue}");

        if (Mathf.Abs(tiltValue) > 0.05f) // Small threshold to prevent jitter
        {
            ShiftHorizontally(tiltValue * tiltSensitivity * 2f); // Increase movement speed
        }
        else
        {
            ResetTurnAnimations();
        }
    }


    void MovePlayer()
    {
        shiftVelocity = 0;
        Debug.Log($"🏃 Player Speed: {rb.linearVelocity.z}");
        Vector3 currentVelocity = rb.linearVelocity;

        // Ensure the speed is always maintained at 5
        rb.linearVelocity = new Vector3(currentVelocity.x, currentVelocity.y, 5f);

        // Detect Phone Tilt
        DetectTilt();

        // Detect Keyboard Horizontal Input (Band-aid solution, bear with me please)
        if (currentKeyboardShift != 0) ShiftHorizontally(currentKeyboardShift);

        // Move Player
        rb.linearVelocity = new Vector3(shiftVelocity, rb.linearVelocity.y, stuck ? 0 : speed);
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

            LevelAudioManager.instance.PlaySound(jumpSFX); // Play jump sound

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
        float minX = -3.5f; // Adjusted min X position
        float maxX = 3.33f; // Adjusted max X position
        float laneWidth = 3f; // Adjust lane width if necessary

        float targetX = Mathf.Clamp(transform.position.x + (direction * laneWidth), minX, maxX);

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
        Debug.Log("❌ Player died! Saving score & displaying leaderboard...");

        // Save last score and distance
        PlayerPrefs.SetInt("LastScore", PlayerUIManager.Instance.GetScore());
        PlayerPrefs.SetFloat("LastDistance", PlayerUIManager.Instance.GetDistance());
        PlayerPrefs.Save();

        // Find Leaderboard UI and enable it
        LeaderboardUI leaderboard = Object.FindFirstObjectByType<LeaderboardUI>();

        if (leaderboard != null)
        {
            leaderboard.gameObject.SetActive(true); // Ensure it's enabled
            leaderboard.ShowLeaderboard();
        }
        else
        {
            Debug.LogError("❌ LeaderboardUI not found! Ensure it's in the scene.");
        }
    }
    bool IsGrounded()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f; 

        bool grounded = Physics.Raycast(origin, Vector3.down, out hit, 0.5f);

        if (grounded)
        {
            Debug.Log($"✅ Grounded on: {hit.collider.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Character is NOT grounded!");
        }

        return grounded;
    }
}