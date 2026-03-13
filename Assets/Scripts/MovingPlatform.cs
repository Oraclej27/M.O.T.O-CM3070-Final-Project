// =============================================
// Script: MovingPlatform.cs
// Purpose: Moves a platform back and forth between two points. Automatically moves any CharacterController standing on top.
//
// Communicates with:
//   - CharacterController: Detects and moves player when on platform by checking bounds.
//
// Usage: Attached to a platform GameObject with a BoxCollider.
// =============================================
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