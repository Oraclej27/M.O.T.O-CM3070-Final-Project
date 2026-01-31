using UnityEngine;
using System.Collections;

public class RobotEmotionCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform robot;
    public Transform faceTarget; // optional: empty transform near head

    [Header("Base Follow")]
    public Vector3 offset = new Vector3(0, 1.6f, -3f);
    public float followSmooth = 8f;
    public float rotateSmooth = 8f;

    [Header("Emotion Focus")]
    public float emotionFocusTime = 0.8f;
    public float emotionRotateBoost = 2.5f;
    public float emotionZoomOffset = 0.8f;
    public Vector3 emotionFrontOffset = new Vector3(0, 1.6f, 2.2f);

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = -5f;
    public float maxZoom = -1.5f;

    //[Header("Collision")]
    //public float collisionRadius = 0.3f;
    //public LayerMask collisionMask;

    [Header("Shake")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.15f;

    // INTERNAL
    private float focusTimer;
    private bool isFocusingEmotion;
    private Vector3 currentVelocity;
    private Vector3 shakeOffset;
    private float zoomZ;

    void Start()
    {
        zoomZ = offset.z;
    }

    void LateUpdate()
    {
        if (!robot) return;

        HandleZoom();
        HandleFocusTimer();

        // --- OFFSET SELECTION ---
        Vector3 baseOffset = offset;
        baseOffset.z = zoomZ;

        if (isFocusingEmotion)
        {
            baseOffset = robot.rotation * emotionFrontOffset;
            baseOffset.z -= emotionZoomOffset;
        }

        // --- POSITION ---
        //Vector3 desiredPos = robot.position + baseOffset;
        //desiredPos = HandleCollision(desiredPos);
        Transform posTarget = isFocusingEmotion && faceTarget ? faceTarget : robot;
        Vector3 desiredPos = posTarget.position + baseOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos + shakeOffset,
            ref currentVelocity,
            1f / followSmooth
        );

        // --- ROTATION ---
        //Transform lookTarget = faceTarget ? faceTarget : robot;

        //float rotateSpeed =
        //    isFocusingEmotion
        //    ? rotateSmooth * emotionRotateBoost
        //    : rotateSmooth;

        //Quaternion lookRot = Quaternion.LookRotation(
        //    lookTarget.position - transform.position
        //);

        //if (isFocusingEmotion)
        //{
        //    transform.rotation = Quaternion.Slerp(
        //        transform.rotation,
        //        lookRot,
        //        rotateSpeed * Time.deltaTime
        //    );
        //}
        //if (isFocusingEmotion)
        //{
        //    Transform lookTarget = faceTarget ? faceTarget : robot;

        //    float rotateSpeed = rotateSmooth * emotionRotateBoost;

        //    Quaternion lookRot = Quaternion.LookRotation(
        //        lookTarget.position - transform.position
        //    );

        //    transform.rotation = Quaternion.Slerp(
        //        transform.rotation,
        //        lookRot,
        //        rotateSpeed * Time.deltaTime
        //    );
        //}
        Transform lookTarget = isFocusingEmotion && faceTarget ? faceTarget : robot;

        float rotateSpeed =
            isFocusingEmotion
            ? rotateSmooth * emotionRotateBoost
            : rotateSmooth;

        Quaternion lookRot = Quaternion.LookRotation(
            lookTarget.position - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            rotateSpeed * Time.deltaTime
        );
    }

    // ---------------- ZOOM ----------------

    //void HandleZoom()
    //{
    //    float scroll = Input.mouseScrollDelta.y;
    //    if (Mathf.Abs(scroll) > 0.01f && !isFocusingEmotion)
    //    {
    //        zoomZ += scroll * zoomSpeed;
    //        zoomZ = Mathf.Clamp(zoomZ, minZoom, maxZoom);
    //    }


    //}
    void HandleZoom()
    {
        if (isFocusingEmotion) return;

        float zoomInput = 0f;

        // Mouse wheel
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f && !isFocusingEmotion)
        {
            zoomZ += scroll * zoomSpeed;
            zoomZ = Mathf.Clamp(zoomZ, minZoom, maxZoom);
        }

        // Keyboard fallback
        if (Input.GetKey(KeyCode.R)) zoomInput += 1f;
        if (Input.GetKey(KeyCode.F)) zoomInput -= 1f;

        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            zoomZ += zoomInput * zoomSpeed * Time.deltaTime;
            zoomZ = Mathf.Clamp(zoomZ, minZoom, maxZoom);
        }
    }

    // ---------------- COLLISION ----------------

    //Vector3 HandleCollision(Vector3 desiredPos)
    //{
    //    Vector3 origin = robot.position + Vector3.up * 1.2f;
    //    Vector3 dir = desiredPos - origin;
    //    float dist = dir.magnitude;

    //    if (Physics.SphereCast(
    //        origin,
    //        collisionRadius,
    //        dir.normalized,
    //        out RaycastHit hit,
    //        dist,
    //        collisionMask))
    //    {
    //        return hit.point - dir.normalized * collisionRadius;
    //    }

    //    return desiredPos;
    //}

    // ---------------- EMOTION EVENTS ----------------

    public void FocusOnEmotion()
    {
        isFocusingEmotion = true;
        focusTimer = emotionFocusTime;
    }


    //void HandleFocusTimer()
    //{
    //    if (!isFocusingEmotion) return;

    //    focusTimer -= Time.deltaTime;
    //    if (focusTimer <= 0f)
    //    {
    //        isFocusingEmotion = false;
    //    }
    //}
    void HandleFocusTimer()
    {
        if (!isFocusingEmotion) return;

        focusTimer -= Time.deltaTime;
        if (focusTimer <= 0f)
        {
            isFocusingEmotion = false;

            // IMPORTANT: reset camera momentum
            //currentVelocity = Vector3.zero;
            currentVelocity *= 0.2f;
        }
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float t = 0f;

        while (t < shakeDuration)
        {
            shakeOffset = Random.insideUnitSphere * shakeStrength;
            t += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}