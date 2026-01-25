using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float runAcceleration = 2f;
    public float gravity = 20f;
    public float jumpForce = 8f;

    [Header("Emotion Systems")]
    public Rob13ColorManager colorManager;
    public EmotionChanger emotionChanger;

    [Header("Hit & Reaction Settings")]
    public int bumpsBeforeAngry = 3;
    public int hitsBeforeCry = 2;
    public int hitsBeforeDeath = 3;

    private CharacterController controller;
    private Animator anim;

    private float verticalVelocity;
    private float runValue;

    private int bumpCount = 0;
    private int ballHitCount = 0;
    private bool isFallen = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    // ---------------- MOVEMENT ----------------
    void HandleMovement()
    {
        if (isFallen) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        anim.SetFloat("Side", horizontal);
        anim.SetFloat("Speed", vertical);

        // Run ramp
        if (Input.GetKey(KeyCode.LeftShift))
            runValue = Mathf.MoveTowards(runValue, 1f, Time.deltaTime * runAcceleration);
        else
            runValue = Mathf.MoveTowards(runValue, 0f, Time.deltaTime * runAcceleration);

        anim.SetFloat("run", runValue);

        Vector3 move = transform.forward * vertical + transform.right * horizontal;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = jumpForce;
            anim.SetBool("Jump", true);
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity -= gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // ---------------- COLLISIONS ----------------
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isFallen) return;

        if (hit.collider.CompareTag("Ball"))
        {
            HandleBallHit(hit);
        }
        else
        {
            HandleBump();
        }
    }

    // ---------------- GAMEPLAY REACTIONS ----------------
    void HandleBump()
    {
        bumpCount++;

        if (bumpCount >= bumpsBeforeAngry)
        {
            SetEmotion(7); // Angry
            bumpCount = 0;
        }
    }

    void HandleBallHit(ControllerColliderHit hit)
    {
        ballHitCount++;

        Vector3 hitDir = hit.moveDirection;
        TriggerHitAnimation(hitDir);

        if (ballHitCount == hitsBeforeCry)
        {
            TriggerFall(hitDir);
            SetEmotion(8); // Cry
        }
        else if (ballHitCount >= hitsBeforeDeath)
        {
            TriggerDeathFall(hitDir);
        }
    }

    // ---------------- ANIMATION HELPERS ----------------
    void TriggerHitAnimation(Vector3 hitDirection)
    {
        anim.SetBool("Hit", true);

        // Left / Right hit variation
        int varyValue = hitDirection.x > 0 ? 1 : 2;
        anim.SetInteger("vary", varyValue);
    }

    void TriggerFall(Vector3 hitDirection)
    {
        isFallen = true;

        if (hitDirection.z > 0)
            anim.SetBool("FallBack", true);
        else
            anim.SetBool("FallFront", true);
    }

    void TriggerDeathFall(Vector3 hitDirection)
    {
        TriggerFall(hitDirection);
        SetEmotion(5); // Death
    }

    // ---------------- EMOTIONS ----------------
    void SetEmotion(int emotionID)
    {
        colorManager.ChangeBodyColor(emotionID);
        emotionChanger.SetEmotionEyes(emotionID);
        emotionChanger.SetEmotionMouth(emotionID);
    }

    // ---------------- ANIMATION EVENT HOOK ----------------
    // Safe fallback if animations don’t auto-recover
    public void OnRecoveredFromFall()
    {
        isFallen = false;
        anim.SetBool("FallFront", false);
        anim.SetBool("FallBack", false);
    }
}
