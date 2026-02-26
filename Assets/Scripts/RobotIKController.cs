using UnityEngine;

public class RobotIKController : MonoBehaviour
{
    [Header("IK Settings")]
    public float ikWeight = 1f;
    public float transitionSpeed = 5f;

    // Internal - these are set by code, not in Inspector
    private Transform rightHandTarget;
    private Transform lookAtTarget;

    private Animator animator;
    private bool useIK = false;
    private float currentIKWeight = 0f;
    private Lever currentLever;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!useIK || currentLever == null)
        {
            // Smoothly fade out IK
            currentIKWeight = Mathf.Lerp(currentIKWeight, 0f, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Smoothly fade in IK
            currentIKWeight = Mathf.Lerp(currentIKWeight, ikWeight, Time.deltaTime * transitionSpeed);
        }

        // Apply Right Hand IK
        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, currentIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, currentIKWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        // Apply Look At IK
        if (lookAtTarget != null)
        {
            animator.SetLookAtWeight(currentIKWeight, 0.3f, 0.5f, 0.5f);
            animator.SetLookAtPosition(lookAtTarget.position);
        }
    }

    public void GrabLever(Lever lever)
    {
        currentLever = lever;

        // Set targets from lever
        rightHandTarget = lever.handTarget;
        lookAtTarget = lever.transform;

        useIK = true;

        // Trigger animation
        //animator.SetTrigger("PullLever");
        Debug.Log("IK: Grabbing lever");
    }

    public void ReleaseLever()
    {
        useIK = false;
        currentLever = null;

        // Clear targets (optional)
        rightHandTarget = null;
        lookAtTarget = null;
    }
}
