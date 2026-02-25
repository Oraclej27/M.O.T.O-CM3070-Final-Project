using UnityEngine;
using System.Collections;

public class RobotEmotionCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraState
    {
        public Vector3 offset = new Vector3(0, 1.6f, -3f);
        public float positionSmooth = 8f;
        public float rotationSmooth = 8f;
        public bool useFollowYaw = true;
        public float fov = 60f;
    }

    [Header("References")]
    public Transform robot;
    public Transform faceTarget; // Should be child of robot at head position

    [Header("Camera States")]
    public CameraState normalState = new CameraState();
    public CameraState emotionState = new CameraState()
    {
        offset = new Vector3(0, 1.6f, 2.2f),
        positionSmooth = 12f,
        rotationSmooth = 12f,
        useFollowYaw = false,
        fov = 45f
    };

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = -5f;
    public float maxZoom = -1.5f;

    [Header("Recenter")]
    public float recenterSpeed = 4f;
    public bool autoRecenter = false;
    public float autoRecenterDelay = 2f;

    [Header("Shake")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.15f;

    //[Header("Mouse Look")]
    //public float mouseSensitivity = 2f;
    //public float minPitch = -30f;
    //public float maxPitch = 60f;
    //public bool invertY = false;

    //// Add these variables with your other private state
    //private float mouseYaw;
    //private float mousePitch;
    //private float mouseYawVelocity;
    //private float mousePitchVelocity;

    // Private State
    private Camera cam;
    private float currentZoom;
    private float followYaw;
    private float yawVelocity;
    private Vector3 positionVelocity;
    private Quaternion rotationVelocity;
    private float timeSinceLastInput;
    private bool isEmotionFocus;
    private float emotionFocusTimer;
    private Vector3 shakeOffset;
    private float targetFOV;
    private float fovVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;

        currentZoom = normalState.offset.z;
        followYaw = robot.eulerAngles.y;

        //mouseYaw = followYaw;
        //mousePitch = 15f;

        targetFOV = normalState.fov;

        if (cam) cam.fieldOfView = targetFOV;
    }

    void LateUpdate()
    {
        if (!robot || !cam) return;

        HandleInput();
        UpdateZoom();
        UpdateEmotionTimer();

        // Get current state settings
        CameraState state = isEmotionFocus ? emotionState : normalState;

        // Update FOV
        targetFOV = state.fov;
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref fovVelocity, 0.3f);

        // Calculate desired position
        Vector3 desiredPosition = CalculateDesiredPosition(state);

        // Apply smooth position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition + shakeOffset,
            ref positionVelocity,
            1f / state.positionSmooth
        );

        // Calculate and apply rotation
        Quaternion desiredRotation = CalculateDesiredRotation(state, desiredPosition);
        transform.rotation = SmoothDampRotation(transform.rotation, desiredRotation, ref rotationVelocity, 1f / state.rotationSmooth);
    }

    Vector3 CalculateDesiredPosition(CameraState state)
    {
        Vector3 offset = state.offset;
        offset.z = currentZoom;

        // Apply follow yaw only in normal state when enabled
        if (state.useFollowYaw)
        {
            Quaternion yawRotation = Quaternion.Euler(0f, followYaw, 0f);
            offset = yawRotation * offset;
        }

        // For emotion state, frame the face FROM THE FRONT
        if (isEmotionFocus && faceTarget)
        {
            // KEY FIX: Use robot's forward direction for emotion framing
            Quaternion faceRotation = Quaternion.LookRotation(-robot.forward); // Look at face FROM front
            offset = faceRotation * offset;
            return faceTarget.position + offset;
        }

        return robot.position + offset;
    }

    Quaternion CalculateDesiredRotation(CameraState state, Vector3 desiredPosition)
    {
        Vector3 lookTargetPosition = (isEmotionFocus && faceTarget) ? faceTarget.position : robot.position;
        Vector3 lookDirection = lookTargetPosition - desiredPosition;

        // Special handling for emotion focus to avoid weird angles
        if (isEmotionFocus)
        {
            // Keep camera mostly level, just look at face
            lookDirection.y = Mathf.Clamp(lookDirection.y, -0.3f, 0.3f);
        }

        return Quaternion.LookRotation(lookDirection);
    }
    //Quaternion CalculateDesiredRotation(CameraState state, Vector3 desiredPosition)
    //{
    //    Vector3 lookTargetPosition = (isEmotionFocus && faceTarget) ? faceTarget.position : robot.position;

    //    if (isEmotionFocus)
    //    {
    //        // Emotion focus - look directly at face
    //        Vector3 lookDirection = lookTargetPosition - desiredPosition;
    //        lookDirection.y = Mathf.Clamp(lookDirection.y, -0.3f, 0.3f);
    //        return Quaternion.LookRotation(lookDirection);
    //    }
    //    else
    //    {
    //        // Normal mode - use mouse pitch for vertical look
    //        Quaternion yawRotation = Quaternion.Euler(0f, followYaw, 0f);
    //        Quaternion pitchRotation = Quaternion.Euler(mousePitch, 0f, 0f);
    //        return yawRotation * pitchRotation;
    //    }
    //}

    void HandleInput()
    {
        // Right-click recenter
        if (Input.GetMouseButton(0))
        {
            float targetYaw = robot.eulerAngles.y;
            followYaw = Mathf.SmoothDampAngle(followYaw, targetYaw, ref yawVelocity, 1f / recenterSpeed);
            timeSinceLastInput = 0f;
        }
        else if (autoRecenter)
        {
            timeSinceLastInput += Time.deltaTime;
            if (timeSinceLastInput > autoRecenterDelay)
            {
                float targetYaw = robot.eulerAngles.y;
                followYaw = Mathf.SmoothDampAngle(followYaw, targetYaw, ref yawVelocity, 1f / (recenterSpeed * 0.5f));
            }
        }
    }
    //void HandleInput()
    //{
    //    if (isEmotionFocus) return; // Don't move camera during emotion focus

    //    // Mouse look (only when right mouse button is held)
    //    if (Input.GetMouseButton(1))
    //    {
    //        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
    //        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1);

    //        mouseYaw += mouseX;
    //        mousePitch -= mouseY;

    //        // Clamp pitch to prevent flipping
    //        mousePitch = Mathf.Clamp(mousePitch, minPitch, maxPitch);

    //        // Update followYaw with mouse Yaw
    //        followYaw = mouseYaw;

    //        timeSinceLastInput = 0f;
    //    }
    //    else if (autoRecenter)
    //    {
    //        timeSinceLastInput += Time.deltaTime;
    //        if (timeSinceLastInput > autoRecenterDelay)
    //        {
    //            // Smoothly recenter to robot's forward
    //            float targetYaw = robot.eulerAngles.y;
    //            mouseYaw = Mathf.SmoothDampAngle(mouseYaw, targetYaw, ref mouseYawVelocity, 1f / recenterSpeed);
    //            mousePitch = Mathf.SmoothDamp(mousePitch, 15f, ref mousePitchVelocity, 1f / recenterSpeed);

    //            followYaw = mouseYaw;
    //        }
    //    }
    //}

    void UpdateZoom()
    {
        if (isEmotionFocus) return; // No zoom during emotion focus

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom += scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            timeSinceLastInput = 0f;
        }

        // Keyboard zoom fallback
        float zoomInput = 0f;
        //if (Input.GetKey(KeyCode.R)) zoomInput += 1f;
        //if (Input.GetKey(KeyCode.F)) zoomInput -= 1f;

        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            currentZoom += zoomInput * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            timeSinceLastInput = 0f;
        }
    }

    void UpdateEmotionTimer()
    {
        if (!isEmotionFocus) return;

        emotionFocusTimer -= Time.deltaTime;
        if (emotionFocusTimer <= 0f)
        {
            isEmotionFocus = false;
            // Gentle exit from emotion focus
            positionVelocity *= 0.3f;
        }
    }

    // ----- PUBLIC INTERFACE -----

    public void FocusOnEmotion(float customDuration = 0f)
    {
        isEmotionFocus = true;
        emotionFocusTimer = customDuration > 0 ? customDuration : 1.5f; // Default 1.5 seconds

        // Reset velocities for clean transition
        positionVelocity = Vector3.zero;
        rotationVelocity = Quaternion.identity;
        yawVelocity = 0f;

        // Optional: Force camera behind robot after emotion
        followYaw = robot.eulerAngles.y;
    }

    public void Shake(float strengthMultiplier = 1f)
    {
        StopCoroutine(nameof(ShakeRoutine));
        StartCoroutine(ShakeRoutine(strengthMultiplier));
    }

    IEnumerator ShakeRoutine(float strengthMultiplier = 1f)
    {
        float elapsed = 0f;
        Vector3 originalShakeOffset = shakeOffset;

        while (elapsed < shakeDuration)
        {
            // Decaying shake
            float decay = 1f - (elapsed / shakeDuration);
            shakeOffset = originalShakeOffset + Random.insideUnitSphere * (shakeStrength * strengthMultiplier * decay);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    // ----- HELPER METHODS -----

    static Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Quaternion velocity, float smoothTime)
    {
        if (Time.deltaTime < Mathf.Epsilon) return current;

        var dot = Quaternion.Dot(current, target);
        var sign = dot > 0f ? 1f : -1f;
        target.x *= sign;
        target.y *= sign;
        target.z *= sign;
        target.w *= sign;

        var result = new Vector4(
            Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime),
            Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime),
            Mathf.SmoothDamp(current.z, target.z, ref velocity.z, smoothTime),
            Mathf.SmoothDamp(current.w, target.w, ref velocity.w, smoothTime)
        ).normalized;

        return new Quaternion(result.x, result.y, result.z, result.w);
    }

    // ----- DEBUG GIZMOS -----
    void OnDrawGizmosSelected()
    {
        if (!robot) return;

        // Draw emotion focus target
        if (faceTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(faceTarget.position, 0.1f);
            Gizmos.DrawLine(robot.position, faceTarget.position);
        }

        // Draw camera position preview
        if (Application.isPlaying)
        {
            CameraState state = isEmotionFocus ? emotionState : normalState;
            Vector3 previewPos = CalculateDesiredPosition(state);

            Gizmos.color = isEmotionFocus ? Color.red : Color.cyan;
            Gizmos.DrawSphere(previewPos, 0.15f);
            Gizmos.DrawLine(previewPos, isEmotionFocus && faceTarget ? faceTarget.position : robot.position);
        }
    }
}
