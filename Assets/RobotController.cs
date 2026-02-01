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
    [SerializeField] float ballHitCooldownTime = 0.5f;
    float ballHitCooldown;

    [SerializeField] private float bumpCooldownTime = 0.5f;
    private float bumpCooldown;

    [Header("References")]
    public Rob13ColorManager robotColorManager;
    public EmotionChanger emotionChanger;
    public RobotEmotionCamera cameraController;

    [Header("Animation Repeat")]
    public int playCount = 1;

    // INTERNAL STATE
    private Animator anim;
    private CharacterController controller;
    private string animationName;

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
        {
            bumpCooldown -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetBool("StrafeLeft", true);
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            anim.SetBool("StrafeLeft", false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetBool("StrafeRight", true);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            anim.SetBool("StrafeRight", false);
        }

    }

    // -------------------- MOVEMENT (UNCHANGED) --------------------

    void HandleMovement()
    {
        anim.SetFloat("Side", Input.GetAxis("Horizontal"));
        anim.SetFloat("Speed", Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift) && run < 1)
        {
            run += Time.deltaTime * runVelocity;
        }
        else if (run > 0)
        {
            run -= Time.deltaTime * runVelocity;
        }

        anim.SetFloat("run", run);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetBool("Jump", true);
        }
    }

    // -------------------- BUMP LOGIC --------------------
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (currentState == RobotState.Fallen) return;

        // Ignore ground
        if (hit.normal.y > 0.7f) return;

        if (bumpCooldown > 0f) return;

        if (hit.collider.CompareTag("Ball"))
        {
            // Safety fallback (most ball hits come from BallHitDetector)
            Debug.Log("BALL HIT via controller");
            //RegisterBallHit();
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
        if (currentState == RobotState.Fallen) return;

        bumpCount++;

        Debug.Log("BUMP COUNT = " + bumpCount);

        anim.SetBool("Hit", true);
        anim.SetInteger("vary", GetNextNumber(3));
        setEmotion(0);

        if (bumpCount >= 3)
        {
            cameraController.FocusOnEmotion();
            anim.SetBool("Angry", true);
            setEmotion(7);
            bumpCount = 0; // reset after angry
            currentState = RobotState.Angry;

            //setEmotion(0);
            //anim.SetBool("reset", true);
        }
    }

    // -------------------- BALL HIT LOGIC --------------------

    public void RegisterBallHit()
    {
        if (ballHitCooldown > 0f) return;
        ballHitCooldown = ballHitCooldownTime;

        if (currentState == RobotState.Fallen) return;

        ballHitCount++;

        Debug.Log("BALL HIT COUNT = " + ballHitCount);

        if (ballHitCount == 1)
        {
            //animationName = "Dance1";
            //robotColorManager.isRainbowCycles = true;
            cameraController.Shake();
            animationName = "Dance1";
            robotColorManager.isRainbowCycles = true;
            setEmotion(8);
            StartCoroutine(PlayAnimationMultipleTimes());
        }
        else if (ballHitCount == 2)
        {
            cameraController.Shake();
            cameraController.FocusOnEmotion();
            setEmotion(0);
            anim.SetBool("Cry", true);
            setEmotion(8);
            currentState = RobotState.Crying;
        }
        else if (ballHitCount >= 3)
        {
            anim.SetBool("FallBack", true);
            setEmotion(5);
            currentState = RobotState.Fallen;
        }
        if (ballHitCooldown > 0f) return;
        ballHitCooldown = ballHitCooldownTime;

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

    // -------------------- HELPERS --------------------

    int currentNumber = 0;
    public int GetNextNumber(int N)
    {
        int result = currentNumber;
        currentNumber = (currentNumber + 1) % (N + 1);
        return result;
    }

    IEnumerator PlayAnimationMultipleTimes()
    {
        for (int i = 0; i < playCount; i++)
        {
            anim.SetBool(animationName, true);
            yield return new WaitForSeconds(1f);
        }

        anim.SetBool(animationName, false);
        robotColorManager.isRainbowCycles = false;
        resetEmo();
    }
}
