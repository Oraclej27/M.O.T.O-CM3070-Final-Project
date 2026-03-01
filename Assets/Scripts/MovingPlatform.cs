//using UnityEngine;
//using System.Collections;

//public class MovingPlatform : MonoBehaviour
//{
//    [Header("Platform Settings")]
//    public Animator platformAnimator;
//    public string isMovingBool = "IsMoving";

//    [Header("Colliders")]
//    public Collider platformCollider; // The solid collider (non-trigger)
//    public Collider detectionTrigger;  // The trigger collider above the platform

//    [Header("Debug")]
//    public bool showDebugLogs = true;

//    private bool robotOnPlatform = false;
//    private bool isMoving = false;
//    //private Transform robotTransform;
//    //private Transform originalParent;

//    void Start()
//    {
//        // Make sure detection trigger is set as trigger
//        if (detectionTrigger != null)
//        {
//            detectionTrigger.isTrigger = true;
//        }

//        // Ensure platform starts stopped
//        if (platformAnimator != null)
//        {
//            platformAnimator.SetBool(isMovingBool, false);
//        }
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        // Check if robot entered the detection zone
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = true;
//            //robotTransform = other.transform;

//            //originalParent = robotTransform.parent;
//            //robotTransform.SetParent(transform);

//            if (showDebugLogs)
//                Debug.Log("Robot detected on platform!");

//            UpdatePlatformMovement();
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        // Check if robot left the detection zone
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = false;

//            //if (robotTransform != null)
//            //    robotTransform.SetParent(originalParent);

//            //robotTransform = null;

//            if (showDebugLogs)
//                Debug.Log(" Robot left platform");

//            UpdatePlatformMovement();
//        }
//    }

//    void UpdatePlatformMovement()
//    {
//        bool shouldMove = robotOnPlatform;

//        if (shouldMove != isMoving)
//        {
//            if (!shouldMove && platformAnimator != null)
//            {
//                // Instead of immediate stop, let animation finish current cycle
//                AnimatorStateInfo stateInfo = platformAnimator.GetCurrentAnimatorStateInfo(0);
//                float normalizedTime = stateInfo.normalizedTime % 1f;

//                if (normalizedTime > 0.5f)
//                {
//                    // Near end of cycle, let it complete
//                    StartCoroutine(WaitForAnimationComplete());
//                    return;
//                }
//            }

//            SetMovement(shouldMove);
//        }
//    }

//    IEnumerator WaitForAnimationComplete()
//    {
//        yield return new WaitForSeconds(0.5f); // Adjust based on your animation
//        SetMovement(false);
//    }

//    void SetMovement(bool move)
//    {
//        isMoving = move;

//        if (platformAnimator != null)
//        {
//            platformAnimator.SetBool(isMovingBool, move);

//            if (showDebugLogs)
//                Debug.Log($"Platform movement: {(move ? "START" : "STOP")}");
//        }
//    }

//    // Optional: Visualize the detection zone in editor
//    void OnDrawGizmosSelected()
//    {
//        if (detectionTrigger != null)
//        {
//            Gizmos.color = new Color(0, 1, 0, 0.3f);

//            // Try to get box collider bounds
//            BoxCollider box = detectionTrigger as BoxCollider;
//            if (box != null)
//            {
//                Gizmos.matrix = detectionTrigger.transform.localToWorldMatrix;
//                Gizmos.DrawCube(box.center, box.size);
//            }
//            else
//            {
//                // Fallback to just drawing the trigger position
//                Gizmos.DrawWireSphere(detectionTrigger.transform.position, 0.5f);
//            }
//        }
//    }
//}
//using UnityEngine;
//using System.Collections;

//public class MovingPlatform : MonoBehaviour
//{
//    [Header("Platform Settings")]
//    public Animator platformAnimator;
//    public string isMovingBool = "IsMoving";

//    [Header("Colliders")]
//    public Collider platformCollider; // The solid collider (non-trigger)
//    public Collider detectionTrigger;  // The trigger collider above the platform

//    [Header("Debug")]
//    public bool showDebugLogs = true;

//    private bool robotOnPlatform = false;
//    private bool isMoving = false;
//    private Transform robotTransform;
//    private Vector3 lastPlatformPosition;
//    private Vector3 platformVelocity;

//    void Start()
//    {
//        if (detectionTrigger != null)
//            detectionTrigger.isTrigger = true;

//        if (platformAnimator != null)
//            platformAnimator.SetBool(isMovingBool, false);

//        lastPlatformPosition = transform.position;
//    }

//    void FixedUpdate()
//    {
//        // Calculate platform velocity for physics-based movement
//        platformVelocity = (transform.position - lastPlatformPosition) / Time.fixedDeltaTime;
//        lastPlatformPosition = transform.position;

//        // If robot is on platform, move it with the platform
//        if (robotOnPlatform && isMoving && robotTransform != null)
//        {
//            Rigidbody robotRb = robotTransform.GetComponent<Rigidbody>();
//            if (robotRb != null)
//            {
//                // Add platform velocity to robot (keeps robot's own movement too)
//                robotRb.linearVelocity += platformVelocity;
//            }
//            else
//            {
//                // Fallback: direct position update if no rigidbody
//                robotTransform.position += platformVelocity * Time.fixedDeltaTime;
//            }
//        }
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = true;
//            robotTransform = other.transform;

//            if (showDebugLogs)
//                Debug.Log(" Robot detected on platform!");

//            UpdatePlatformMovement();
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = false;
//            robotTransform = null;

//            if (showDebugLogs)
//                Debug.Log(" Robot left platform");

//            UpdatePlatformMovement();
//        }
//    }

//    void UpdatePlatformMovement()
//    {
//        bool shouldMove = robotOnPlatform;

//        if (shouldMove != isMoving)
//        {
//            if (!shouldMove && platformAnimator != null)
//            {
//                AnimatorStateInfo stateInfo = platformAnimator.GetCurrentAnimatorStateInfo(0);
//                float normalizedTime = stateInfo.normalizedTime % 1f;

//                if (normalizedTime > 0.5f)
//                {
//                    StartCoroutine(WaitForAnimationComplete());
//                    return;
//                }
//            }

//            SetMovement(shouldMove);
//        }
//    }

//    IEnumerator WaitForAnimationComplete()
//    {
//        yield return new WaitForSeconds(0.5f);
//        SetMovement(false);
//    }

//    void SetMovement(bool move)
//    {
//        isMoving = move;

//        if (platformAnimator != null)
//        {
//            platformAnimator.SetBool(isMovingBool, move);

//            if (showDebugLogs)
//                Debug.Log($"Platform movement: {(move ? "START" : "STOP")}");
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (detectionTrigger != null)
//        {
//            Gizmos.color = new Color(0, 1, 0, 0.3f);
//            BoxCollider box = detectionTrigger as BoxCollider;
//            if (box != null)
//            {
//                Gizmos.matrix = detectionTrigger.transform.localToWorldMatrix;
//                Gizmos.DrawCube(box.center, box.size);
//            }
//        }
//    }
//}
//using UnityEngine;
//using System.Collections;

//public class MovingPlatform : MonoBehaviour
//{
//    [Header("Platform Settings")]
//    public Animator platformAnimator;
//    public string isMovingBool = "IsMoving";

//    [Header("Colliders")]
//    public Collider platformCollider;
//    public Collider detectionTrigger;

//    [Header("Debug")]
//    public bool showDebugLogs = true;

//    private bool robotOnPlatform = false;
//    private bool isMoving = false;
//    private Transform robotTransform;
//    private Transform originalParent;

//    void Start()
//    {
//        if (detectionTrigger != null)
//            detectionTrigger.isTrigger = true;

//        if (platformAnimator != null)
//            platformAnimator.SetBool(isMovingBool, false);
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = true;
//            robotTransform = other.transform;

//            // Store original parent
//            originalParent = robotTransform.parent;

//            // Make robot a child of the platform
//            robotTransform.SetParent(transform);

//            if (showDebugLogs)
//                Debug.Log("Robot attached to platform!");

//            UpdatePlatformMovement();
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
//        {
//            robotOnPlatform = false;

//            // Detach robot from platform
//            if (robotTransform != null)
//                robotTransform.SetParent(originalParent);

//            robotTransform = null;

//            if (showDebugLogs)
//                Debug.Log(" Robot detached from platform");

//            UpdatePlatformMovement();
//        }
//    }

//    void UpdatePlatformMovement()
//    {
//        bool shouldMove = robotOnPlatform;

//        if (shouldMove != isMoving)
//        {
//            SetMovement(shouldMove);
//        }
//    }

//    void SetMovement(bool move)
//    {
//        isMoving = move;

//        if (platformAnimator != null)
//        {
//            platformAnimator.SetBool(isMovingBool, move);

//            if (showDebugLogs)
//                Debug.Log($"Platform movement: {(move ? "START" : "STOP")}");
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (detectionTrigger != null)
//        {
//            Gizmos.color = new Color(0, 1, 0, 0.3f);
//            BoxCollider box = detectionTrigger as BoxCollider;
//            if (box != null)
//            {
//                Gizmos.matrix = detectionTrigger.transform.localToWorldMatrix;
//                Gizmos.DrawCube(box.center, box.size);
//            }
//        }
//    }
//}
//--------------------------------------------------------------------
//using UnityEngine;

//public class MovingPlatform : MonoBehaviour
//{
//    public Transform pointA;
//    public Transform pointB;
//    public float speed = 2f;

//    private Vector3 target;
//    private Vector3 lastPosition;

//    void Start()
//    {
//        target = pointB.position;
//        lastPosition = transform.position;
//    }

//    void Update()
//    {
//        // Move platform
//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            target,
//            speed * Time.deltaTime
//        );

//        if (Vector3.Distance(transform.position, target) < 0.05f)
//        {
//            target = target == pointA.position ? pointB.position : pointA.position;
//        }

//        // Calculate movement delta
//        Vector3 platformDelta = transform.position - lastPosition;

//        // Move any character standing on it
//        foreach (Collider col in Physics.OverlapBox(
//            transform.position + Vector3.up * 1f,
//            new Vector3(1f, 0.5f, 1f)))
//        {
//            CharacterController cc = col.GetComponent<CharacterController>();
//            if (cc != null && cc.isGrounded)
//            {
//                cc.Move(platformDelta);
//            }
//        }

//        lastPosition = transform.position;
//    }
//}
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Vector3 target;
    private Vector3 lastPosition;

    private BoxCollider platformCollider;

    void Start()
    {
        target = pointB.position;
        lastPosition = transform.position;
        platformCollider = GetComponent<BoxCollider>();
    }

    void LateUpdate()
    {
        // Move platform
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            target = target == pointA.position ? pointB.position : pointA.position;
        }

        Vector3 platformDelta = transform.position - lastPosition;

        if (platformDelta != Vector3.zero)
        {
            MoveCharacterControllersOnTop(platformDelta);
        }

        lastPosition = transform.position;
    }

    void MoveCharacterControllersOnTop(Vector3 delta)
    {
        CharacterController[] controllers = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);

        foreach (CharacterController cc in controllers)
        {
            // Check if bottom of controller is touching top of platform
            float controllerBottom = cc.bounds.min.y;
            float platformTop = platformCollider.bounds.max.y;

            bool isOnTop =
                controllerBottom >= platformTop - 0.1f &&
                controllerBottom <= platformTop + 0.2f &&
                cc.bounds.max.x > platformCollider.bounds.min.x &&
                cc.bounds.min.x < platformCollider.bounds.max.x &&
                cc.bounds.max.z > platformCollider.bounds.min.z &&
                cc.bounds.min.z < platformCollider.bounds.max.z;

            if (isOnTop)
            {
                cc.Move(delta);
            }
        }
    }
}