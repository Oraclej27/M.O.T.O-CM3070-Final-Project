// =============================================
// Script: RobotEmotionCamera.cs
// Purpose: Controls the camera that follows the robot, with special focus during emotional states and screen shake effects.
//
// Communicates with:
//   - RobotController: Called via FocusOnEmotion() and Shake() when robot experiences emotional events.
//
// Usage: Attached to the main camera GameObject, uses robot and faceTarget references.
// =============================================
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
    [SerializeField] private Transform robot;
    [SerializeField] private Transform faceTarget;

    [Header("Camera States")]
    [SerializeField] private CameraState normalState = new CameraState();
    [SerializeField]
    private CameraState emotionState = new CameraState()
    {
        offset = new Vector3(0, 1.6f, 2.2f),
        positionSmooth = 12f,
        rotationSmooth = 12f,
        useFollowYaw = false,
        fov = 45f
    };

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = -5f;
    [SerializeField] private float maxZoom = -1.5f;

    [Header("Recenter")]
    [SerializeField] private float recenterSpeed = 4f;
    [SerializeField] private bool autoRecenter = false;
    [SerializeField] private float autoRecenterDelay = 2f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.15f;

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
        targetFOV = normalState.fov;

        if (cam) cam.fieldOfView = targetFOV;
    }

    void LateUpdate()
    {
        if (!robot || !cam) return;

        HandleInput();
        UpdateZoom();
        UpdateEmotionTimer();

        CameraState state = isEmotionFocus ? emotionState : normalState;

        targetFOV = state.fov;
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref fovVelocity, 0.3f);

        Vector3 desiredPosition = CalculateDesiredPosition(state);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition + shakeOffset,
            ref positionVelocity,
            1f / state.positionSmooth
        );

        Quaternion desiredRotation = CalculateDesiredRotation(state, desiredPosition);
        transform.rotation = SmoothDampRotation(transform.rotation, desiredRotation, ref rotationVelocity, 1f / state.rotationSmooth);
    }

    Vector3 CalculateDesiredPosition(CameraState state)
    {
        Vector3 offset = state.offset;
        offset.z = currentZoom;

        if (state.useFollowYaw)
        {
            Quaternion yawRotation = Quaternion.Euler(0f, followYaw, 0f);
            offset = yawRotation * offset;
        }

        if (isEmotionFocus && faceTarget)
        {
            Quaternion faceRotation = Quaternion.LookRotation(-robot.forward);
            offset = faceRotation * offset;
            return faceTarget.position + offset;
        }

        return robot.position + offset;
    }

    Quaternion CalculateDesiredRotation(CameraState state, Vector3 desiredPosition)
    {
        Vector3 lookTargetPosition = (isEmotionFocus && faceTarget) ? faceTarget.position : robot.position;
        Vector3 lookDirection = lookTargetPosition - desiredPosition;

        if (isEmotionFocus)
        {
            lookDirection.y = Mathf.Clamp(lookDirection.y, -0.3f, 0.3f);
        }

        return Quaternion.LookRotation(lookDirection);
    }

    void HandleInput()
    {
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

    void UpdateZoom()
    {
        if (isEmotionFocus) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom += scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            timeSinceLastInput = 0f;
        }

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
            positionVelocity *= 0.3f;
        }
    }

    public void FocusOnEmotion(float customDuration = 0f)
    {
        isEmotionFocus = true;
        emotionFocusTimer = customDuration > 0 ? customDuration : 1.5f;

        positionVelocity = Vector3.zero;
        rotationVelocity = Quaternion.identity;
        yawVelocity = 0f;

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
            float decay = 1f - (elapsed / shakeDuration);
            shakeOffset = originalShakeOffset + Random.insideUnitSphere * (shakeStrength * strengthMultiplier * decay);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

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

    // Debug -- 
    void OnDrawGizmosSelected()
    {
        if (!robot) return;

        if (faceTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(faceTarget.position, 0.1f);
            Gizmos.DrawLine(robot.position, faceTarget.position);
        }

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