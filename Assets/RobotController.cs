// =============================================
// Script: RobotController.cs
// Purpose: Handles robot movement, emotional states, and recovery.
//
// Communicates with:
//   - RobotPickupController: Reads IsHoldingBlock property.
//   - SoundController: Calls PlayBumpSound, PlayBallHitSound, etc.
//   - RobotEmotionCamera: Calls FocusOnEmotion and Shake.
//   - Rob13ColorManager / EmotionChanger: Updates visuals via SetEmotion.
//
// Usage: Attached to the robot GameObject.
// =============================================
using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundStickForce = -2f;

    [Header("Weight")]
#pragma warning disable 0414
    [SerializeField] private float robotWeight = 3f;
#pragma warning disable 0414

    [Header("Debug / Collision")]
    [SerializeField] private int bumpCount;
    [SerializeField] private int ballHitCount;
    [SerializeField] private float ballHitCooldownTime = 1.0f;
    [SerializeField] private float bumpCooldownTime = 0.5f;

    [Header("References")]
    [SerializeField] private Rob13ColorManager robotColorManager;
    [SerializeField] private EmotionChanger emotionChanger;
    [SerializeField] private RobotEmotionCamera cameraController;

    [Header("Animation Repeat")]
    [SerializeField] private int playCount = 1;

    [Header("Animation Timing")]
    [SerializeField] private float hitAnimationDuration = 0.6f;
    [SerializeField] private float angerDuration = 2.0f;

    [Header("Recovery UI")]
    [SerializeField] private GameObject recoveryPrompt;
    [SerializeField] private float promptDisplayDelay = 1f;

    private bool _isHoldingBlock;
    public bool IsHoldingBlock
    {
        get => _isHoldingBlock;
        set
        {
            _isHoldingBlock = value;
        }
    }

    private Animator anim;
    private CharacterController controller;
    private string animationName;
    private Coroutine currentDanceCoroutine;
    private bool isDancing = false;
    private SoundController soundController;
    private Coroutine promptCoroutine;
    private bool applyCollapseGravity = false;
    private Vector3 gravityVelocity = Vector3.zero;
    private const float GRAVITY_FORCE = 20f;
    private RobotState currentState = RobotState.Normal;

    private float run;
    private float runVelocity = 1f;
    private float verticalVelocity;
    private float ballHitCooldown;
    private float bumpCooldown;

    private enum RobotState
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
        soundController = FindFirstObjectByType<SoundController>();

        if (recoveryPrompt != null)
            recoveryPrompt.SetActive(false);
    }

    void Update()
    {
        HandleMovement();

        if (ballHitCooldown > 0f)
            ballHitCooldown -= Time.deltaTime;

        if (bumpCooldown > 0f)
            bumpCooldown -= Time.deltaTime;

        if (currentState == RobotState.Fallen)
        {
            if (Input.GetAxis("Vertical") > 0.35f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                HideRecoveryPrompt();
                StartCoroutine(RecoverFromFallen());
            }
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

        if (applyCollapseGravity && !controller.isGrounded)
        {
            ApplyCollapseGravity();
        }
    }

    void ApplyCollapseGravity()
    {
        if (controller.isGrounded)
        {
            gravityVelocity = Vector3.zero;
            applyCollapseGravity = false;
            return;
        }

        gravityVelocity += Physics.gravity * GRAVITY_FORCE * Time.deltaTime;
        controller.Move(gravityVelocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (currentState != RobotState.Normal && currentState != RobotState.Angry)
        {
            anim.SetFloat("Side", 0);
            anim.SetFloat("Speed", 0);
            anim.SetFloat("run", 0);
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        anim.SetFloat("Side", horizontal);
        anim.SetFloat("Speed", vertical);

        Vector3 move = transform.forward * vertical + transform.right * horizontal;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move *= speed;

        if (Input.GetKey(KeyCode.LeftShift) && run < 1)
            run += Time.deltaTime * runVelocity;
        else if (run > 0)
            run -= Time.deltaTime * runVelocity;

        anim.SetFloat("run", run);

        Vector3 finalMove = move + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        bool moving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        float currentSpeed = Mathf.Abs(vertical) * speed;
        float maxSpeed = speed;
        if (soundController != null)
            soundController.SetMoving(moving && controller.isGrounded, currentSpeed, maxSpeed);
    }

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
        if (currentState == RobotState.Fallen || isDancing || IsHoldingBlock)
            return;

        if (soundController != null)
            soundController.PlayBumpSound();

        bumpCount++;
        Debug.Log("BUMP COUNT = " + bumpCount);

        StartCoroutine(ProcessBumpRoutine());
    }

    IEnumerator ProcessBumpRoutine()
    {
        anim.SetBool("Hit", true);
        anim.SetInteger("vary", GetNextNumber(3));
        SetEmotion(0);

        yield return new WaitForSeconds(hitAnimationDuration);
        anim.SetBool("Hit", false);

        if (bumpCount >= 6)
        {
            if (soundController != null)
                soundController.PlayAngrySound();

            cameraController.FocusOnEmotion();
            anim.SetBool("Angry", true);
            SetEmotion(7);
            bumpCount = 0;
            currentState = RobotState.Angry;

            yield return new WaitForSeconds(angerDuration);

            anim.SetBool("Angry", false);
            currentState = RobotState.Normal;
            SetEmotion(0);
        }
    }

    public void RegisterBallHit()
    {
        if (ballHitCooldown > 0f) return;
        if (isDancing) return;
        if (currentState == RobotState.Crying || currentState == RobotState.Fallen) return;

        ballHitCooldown = ballHitCooldownTime;

        if (soundController != null)
            soundController.PlayBallHitSound();

        ballHitCount++;
        Debug.Log($"BALL HIT #{ballHitCount}");

        if (ballHitCount == 1)
        {
            StartFirstHitDance();
        }
        else if (ballHitCount == 2)
        {
            StartCoroutine(SecondHitCrySequence());
        }
        else if (ballHitCount >= 3)
        {
            ThirdHitFallWithGravity();
        }
    }

    void StartFirstHitDance()
    {
        if (currentDanceCoroutine != null)
        {
            StopCoroutine(currentDanceCoroutine);
            CleanUpDance();
        }

        cameraController.Shake();
        animationName = "Dance1";
        robotColorManager.IsRainbowCycles = true;
        SetEmotion(8);

        if (soundController != null)
            soundController.PlayDanceSound();

        isDancing = true;
        currentDanceCoroutine = StartCoroutine(PlayAnimationMultipleTimes());
    }

    IEnumerator SecondHitCrySequence()
    {
        anim.SetBool("Hit", true);
        anim.SetInteger("vary", GetNextNumber(3));
        SetEmotion(0);

        yield return new WaitForSeconds(hitAnimationDuration);
        anim.SetBool("Hit", false);

        if (soundController != null)
            soundController.PlayCrySound();

        if (isDancing)
        {
            CleanUpDance();
        }

        cameraController.Shake();
        cameraController.FocusOnEmotion();
        anim.SetBool("Cry", true);
        SetEmotion(8);
        currentState = RobotState.Crying;

        yield return new WaitForSeconds(2.0f);

        anim.SetBool("Cry", false);
        currentState = RobotState.Normal;
        SetEmotion(0);
    }

    void ThirdHitFallWithGravity()
    {
        if (soundController != null)
            soundController.PlayFallSound();

        RobotPickupController pickup = GetComponent<RobotPickupController>();
        if (pickup != null && pickup.HeldBlock != null)   
        {
            pickup.DropBlock();
        }

        if (isDancing)
        {
            CleanUpDance();
        }

        bool wasJumping = anim.GetBool("Jump");

        if (wasJumping)
        {
            Debug.Log("Third hit detected while jumping! Stopping jump animation.");
            anim.SetBool("Jump", false);
        }

        if (!controller.isGrounded)
        {
            applyCollapseGravity = true;
            gravityVelocity = Vector3.zero;

            StartCoroutine(MonitorGroundingDuringCollapse());
        }
        else
        {
            applyCollapseGravity = false;
        }

        anim.SetBool("FallBack", true);
        SetEmotion(5);
        currentState = RobotState.Fallen;

        if (promptCoroutine != null)
            StopCoroutine(promptCoroutine);
        promptCoroutine = StartCoroutine(ShowPromptAfterDelay());
    }

    IEnumerator ShowPromptAfterDelay()
    {
        yield return new WaitForSeconds(promptDisplayDelay);
        ShowRecoveryPrompt();
    }

    IEnumerator MonitorGroundingDuringCollapse()
    {
        while (!controller.isGrounded && currentState == RobotState.Fallen)
        {
            controller.Move(Vector3.down * 5f * Time.deltaTime);
            yield return null;
        }

        if (controller.isGrounded)
        {
            applyCollapseGravity = false;
            cameraController.Shake(0.3f);
        }
    }

    IEnumerator PlayAnimationMultipleTimes()
    {
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

        anim.SetBool("Dance1", false);
        robotColorManager.IsRainbowCycles = false;
        ResetEmo();
        isDancing = false;
    }

    public void SetEmotion(int emoNumber)
    {
        robotColorManager.ChangeBodyColor(emoNumber);
        emotionChanger.SetEmotionEyes(emoNumber);
        emotionChanger.SetEmotionMouth(emoNumber);
    }

    void ResetEmo()
    {
        SetEmotion(0);
        anim.SetBool("reset", true);
    }

    IEnumerator RecoverFromFallen()
    {
        HideRecoveryPrompt();
        yield return new WaitForSeconds(2.0f);

        if (!anim.GetBool("FallBack"))
        {
            CompleteRecovery();
        }
    }

    void CompleteRecovery()
    {
        HideRecoveryPrompt();
        if (isDancing)
        {
            CleanUpDance();
            if (currentDanceCoroutine != null)
            {
                StopCoroutine(currentDanceCoroutine);
                currentDanceCoroutine = null;
            }
        }

        applyCollapseGravity = false;
        gravityVelocity = Vector3.zero;

        currentState = RobotState.Normal;
        ballHitCount = 0;
        bumpCount = 0;
        SetEmotion(0);
        isDancing = false;

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
        for (int i = 0; i < 10; i++)
        {
            if (!controller.isGrounded)
            {
                controller.Move(Vector3.down * 10f * Time.deltaTime);
            }
            else
            {
                break;
            }
            yield return null;
        }

        if (!controller.isGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f))
            {
                Vector3 groundPos = hit.point;
                groundPos.y += 0.05f;
                transform.position = groundPos;
            }
        }
    }

    void ShowRecoveryPrompt()
    {
        if (recoveryPrompt != null && currentState == RobotState.Fallen)
        {
            recoveryPrompt.SetActive(true);
        }
    }

    void HideRecoveryPrompt()
    {
        if (recoveryPrompt != null)
            recoveryPrompt.SetActive(false);
    }

    // Utility ----
    int currentNumber = 0;
    public int GetNextNumber(int N)
    {
        int result = currentNumber;
        currentNumber = (currentNumber + 1) % (N + 1);
        return result;
    }
}