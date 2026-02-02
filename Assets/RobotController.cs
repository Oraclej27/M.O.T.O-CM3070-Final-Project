using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.0f;
    public float run = 0;
    float runVelocity = 1f;

    [Header("Debug / Collision")]
    [SerializeField] private int bumpCount;
    [SerializeField] private int ballHitCount;
    [SerializeField] float ballHitCooldownTime = 1.0f;
    float ballHitCooldown;

    [SerializeField] private float bumpCooldownTime = 0.5f;
    private float bumpCooldown;

    [Header("References")]
    public Rob13ColorManager robotColorManager;
    public EmotionChanger emotionChanger;
    public RobotEmotionCamera cameraController;

    [Header("Animation Repeat")]
    public int playCount = 1;

    [Header("Animation Timing")]
    public float hitAnimationDuration = 0.6f;
    public float angerDuration = 2.0f;

    // INTERNAL STATE
    private Animator anim;
    private CharacterController controller;
    private string animationName;
    private Coroutine currentDanceCoroutine;
    private bool isDancing = false;

    // FIX: Track if we need to apply gravity during collapse
    private bool applyCollapseGravity = false;
    private Vector3 gravityVelocity = Vector3.zero;
    private const float GRAVITY_FORCE = 20f; // Stronger than default gravity

    // FSM-lite
    private RobotState currentState = RobotState.Normal;

    enum RobotState
    {
        Normal,
        Angry,
        Crying,
        Fallen
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.SetFloat("speedMultiplier", speed);
    }

    void Update()
    {
        HandleMovement();
        HandleJump();

        if (ballHitCooldown > 0f)
            ballHitCooldown -= Time.deltaTime;

        if (bumpCooldown > 0f)
            bumpCooldown -= Time.deltaTime;

        // Check for recovery from fallen state via movement
        if (currentState == RobotState.Fallen &&
            (Input.GetAxis("Vertical") > 0.35f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f))
        {
            StartCoroutine(RecoverFromFallen());
        }

        // Strafe inputs
        if (Input.GetKeyDown(KeyCode.Q))
            anim.SetBool("StrafeLeft", true);
        if (Input.GetKeyUp(KeyCode.Q))
            anim.SetBool("StrafeLeft", false);

        if (Input.GetKeyDown(KeyCode.E))
            anim.SetBool("StrafeRight", true);
        if (Input.GetKeyUp(KeyCode.E))
            anim.SetBool("StrafeRight", false);

        // FIX: Apply gravity during collapsed state if in air
        if (applyCollapseGravity && !controller.isGrounded)
        {
            ApplyCollapseGravity();
        }
    }

    // FIX: Apply strong gravity during collapse
    void ApplyCollapseGravity()
    {
        if (controller.isGrounded)
        {
            gravityVelocity = Vector3.zero;
            applyCollapseGravity = false;
            return;
        }

        // Apply gravity
        gravityVelocity += Physics.gravity * GRAVITY_FORCE * Time.deltaTime;

        // Move with CharacterController
        controller.Move(gravityVelocity * Time.deltaTime);

        // Debug: Show we're falling
        Debug.Log($"Applying collapse gravity. Velocity: {gravityVelocity.y:F2}, Position Y: {transform.position.y:F2}");
    }

    // -------------------- MOVEMENT --------------------
    void HandleMovement()
    {
        // Don't allow movement during emotional states
        if (currentState == RobotState.Normal || currentState == RobotState.Angry)
        {
            anim.SetFloat("Side", Input.GetAxis("Horizontal"));
            anim.SetFloat("Speed", Input.GetAxis("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift) && run < 1)
                run += Time.deltaTime * runVelocity;
            else if (run > 0)
                run -= Time.deltaTime * runVelocity;

            anim.SetFloat("run", run);
        }
        else
        {
            // Stop movement input during emotional states
            anim.SetFloat("Side", 0);
            anim.SetFloat("Speed", 0);
            anim.SetFloat("run", 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentState == RobotState.Normal && !isDancing)
            anim.SetBool("Jump", true);
    }

    // -------------------- BUMP LOGIC --------------------
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (currentState == RobotState.Fallen || isDancing) return;
        if (hit.normal.y > 0.7f) return;
        if (bumpCooldown > 0f) return;

        if (hit.collider.CompareTag("Ball"))
        {
            Debug.Log("BALL HIT via controller");
            return;
        }
        else
        {
            Debug.Log("BUMP detected with: " + hit.collider.name);
            RegisterBump();
        }

        bumpCooldown = bumpCooldownTime;
    }

    public void RegisterBump()
    {
        if (currentState == RobotState.Fallen || isDancing) return;

        bumpCount++;
        Debug.Log("BUMP COUNT = " + bumpCount);

        StartCoroutine(ProcessBumpRoutine());
    }

    IEnumerator ProcessBumpRoutine()
    {
        anim.SetBool("Hit", true);
        anim.SetInteger("vary", GetNextNumber(3));
        setEmotion(0);

        yield return new WaitForSeconds(hitAnimationDuration);
        anim.SetBool("Hit", false);

        if (bumpCount >= 3)
        {
            cameraController.FocusOnEmotion();
            anim.SetBool("Angry", true);
            setEmotion(7);
            bumpCount = 0;
            currentState = RobotState.Angry;

            yield return new WaitForSeconds(angerDuration);

            anim.SetBool("Angry", false);
            currentState = RobotState.Normal;
            setEmotion(0);
        }
    }

    // -------------------- BALL HIT LOGIC --------------------
    public void RegisterBallHit()
    {
        // Protection against concurrent hits
        if (ballHitCooldown > 0f) return;
        if (isDancing) return;
        if (currentState == RobotState.Crying || currentState == RobotState.Fallen) return;

        ballHitCooldown = ballHitCooldownTime;

        ballHitCount++;
        Debug.Log($"BALL HIT #{ballHitCount}");

        if (ballHitCount == 1)
        {
            // FIRST HIT: Dance only
            StartFirstHitDance();
        }
        else if (ballHitCount == 2)
        {
            // SECOND HIT: Hit animation THEN crying
            StartCoroutine(SecondHitCrySequence());
        }
        else if (ballHitCount >= 3)
        {
            // THIRD HIT: Fall immediately - WITH CONTINUOUS GRAVITY
            ThirdHitFallWithGravity();
        }
    }

    void StartFirstHitDance()
    {
        // Stop any existing dance first
        if (currentDanceCoroutine != null)
        {
            StopCoroutine(currentDanceCoroutine);
            CleanUpDance();
        }

        cameraController.Shake();
        animationName = "Dance1";
        robotColorManager.isRainbowCycles = true;
        setEmotion(8);

        isDancing = true;
        currentDanceCoroutine = StartCoroutine(PlayAnimationMultipleTimes());
    }

    IEnumerator SecondHitCrySequence()
    {
        // 1. Play hit animation
        anim.SetBool("Hit", true);
        anim.SetInteger("vary", GetNextNumber(3));
        setEmotion(0);

        yield return new WaitForSeconds(hitAnimationDuration);
        anim.SetBool("Hit", false);

        // 2. Stop any active dance
        if (isDancing)
        {
            CleanUpDance();
        }

        // 3. Start crying
        cameraController.Shake();
        cameraController.FocusOnEmotion();
        anim.SetBool("Cry", true);
        setEmotion(8);
        currentState = RobotState.Crying;

        yield return new WaitForSeconds(2.0f);

        // 4. Stop crying
        anim.SetBool("Cry", false);
        currentState = RobotState.Normal;
        setEmotion(0);
    }

    // FIXED: Third hit with continuous gravity
    void ThirdHitFallWithGravity()
    {
        // Stop any active dance first
        if (isDancing)
        {
            CleanUpDance();
        }

        // Check if we're jumping/in air
        bool wasJumping = anim.GetBool("Jump");

        if (wasJumping)
        {
            Debug.Log("Third hit detected while jumping! Stopping jump animation.");
            anim.SetBool("Jump", false);
        }

        // Check if we're grounded
        if (!controller.isGrounded)
        {
            Debug.Log($"Collapsing in air! Height: {transform.position.y:F2}. Will apply continuous gravity.");

            // Enable gravity application
            applyCollapseGravity = true;
            gravityVelocity = Vector3.zero; // Reset velocity

            // Start a coroutine to monitor when we hit ground
            StartCoroutine(MonitorGroundingDuringCollapse());
        }
        else
        {
            Debug.Log("Collapsing on ground.");
            applyCollapseGravity = false;
        }

        // Play collapse animation
        anim.SetBool("FallBack", true);
        setEmotion(5);
        currentState = RobotState.Fallen;
    }

    IEnumerator MonitorGroundingDuringCollapse()
    {
        // Keep applying gravity until we're grounded
        while (!controller.isGrounded && currentState == RobotState.Fallen)
        {
            // Extra strong downward force for dramatic fall
            controller.Move(Vector3.down * 5f * Time.deltaTime);
            yield return null;
        }

        // Once grounded, stop gravity application
        if (controller.isGrounded)
        {
            applyCollapseGravity = false;
            Debug.Log("Successfully grounded during collapse.");

            // Optional: Small impact shake when hitting ground
            cameraController.Shake(0.3f);
        }
    }

    // -------------------- DANCE ANIMATION --------------------
    IEnumerator PlayAnimationMultipleTimes()
    {
        Debug.Log($"Dance starting. isDancing={isDancing}, playCount={playCount}");

        for (int i = 0; i < playCount; i++)
        {
            anim.SetBool(animationName, true);
            yield return new WaitForSeconds(1f);
        }

        CleanUpDance();
        currentDanceCoroutine = null;
    }

    void CleanUpDance()
    {
        if (!isDancing) return;

        Debug.Log("Cleaning up dance");
        anim.SetBool("Dance1", false);
        robotColorManager.isRainbowCycles = false;
        resetEmo();
        isDancing = false;
    }

    // -------------------- EMOTIONS --------------------
    public void setEmotion(int emoNumber)
    {
        robotColorManager.ChangeBodyColor(emoNumber);
        emotionChanger.SetEmotionEyes(emoNumber);
        emotionChanger.SetEmotionMouth(emoNumber);
    }

    void resetEmo()
    {
        setEmotion(0);
        anim.SetBool("reset", true);
    }

    // -------------------- RECOVERY SYSTEM (SIMPLIFIED) --------------------
    IEnumerator RecoverFromFallen()
    {
        yield return new WaitForSeconds(2.0f);

        if (!anim.GetBool("FallBack"))
        {
            CompleteRecovery();
        }
    }

    void CompleteRecovery()
    {
        // Stop any active dance
        if (isDancing)
        {
            CleanUpDance();
            if (currentDanceCoroutine != null)
            {
                StopCoroutine(currentDanceCoroutine);
                currentDanceCoroutine = null;
            }
        }

        // Stop gravity application
        applyCollapseGravity = false;
        gravityVelocity = Vector3.zero;

        currentState = RobotState.Normal;
        ballHitCount = 0;
        bumpCount = 0;
        setEmotion(0);
        isDancing = false;

        // Final ground check - if somehow still floating, force down
        if (!controller.isGrounded)
        {
            Debug.LogWarning("Still not grounded after recovery! Forcing down.");
            StartCoroutine(ForceFinalGroundCheck());
        }
        else
        {
            Debug.Log("Robot fully recovered");
        }
    }

    IEnumerator ForceFinalGroundCheck()
    {
        // Try a few times to get grounded
        for (int i = 0; i < 10; i++)
        {
            if (!controller.isGrounded)
            {
                // Apply strong downward force
                controller.Move(Vector3.down * 10f * Time.deltaTime);
            }
            else
            {
                break;
            }
            yield return null;
        }

        // Last resort: if still floating, use raycast to find ground
        if (!controller.isGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f))
            {
                Vector3 groundPos = hit.point;
                groundPos.y += 0.05f; // Slight offset
                transform.position = groundPos;
                Debug.Log("Nuclear option: Raycast ground snap");
            }
        }
    }

    // -------------------- UTILITIES --------------------
    int currentNumber = 0;
    public int GetNextNumber(int N)
    {
        int result = currentNumber;
        currentNumber = (currentNumber + 1) % (N + 1);
        return result;
    }
}