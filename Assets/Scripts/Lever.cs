// =============================================
// Script: Lever.cs
// Purpose: Represents a pullable lever. Plays animation, invokes UnityEvent
//
// Communicates with:
//   - RobotIKController: Provides handTarget for IK grabbing.
//   - RobotPickupController: Called via PullLever() from animation event.
//   - BoxContainer: Triggers OpenContainer(). 
//
// Usage: Attached to lever GameObject.
// =============================================
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private Transform handTarget;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask robotLayer;

    [Header("Animation")]
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private string pullTriggerName = "Pull";
    //[SerializeField] private string resetTriggerName = "Reset";

    [Header("Events")]
    [SerializeField] private UnityEvent onLeverPulled;

    [Header("IK Hint")]
    [SerializeField] private Transform elbowHint;

    [Header("Box Connection")]
    [SerializeField] private BoxContainer targetBox;

    // Public properties .. read only
    public Transform HandTarget => handTarget;
    public float InteractionRange => interactionRange;
    public LayerMask RobotLayer => robotLayer;
    public Transform ElbowHint => elbowHint;

    private bool isPulled = false;

    public void PullLever()
    {
        if (isPulled) return;

        isPulled = true;

        if (leverAnimator != null)
            leverAnimator.SetTrigger(pullTriggerName);

        onLeverPulled?.Invoke();

        Debug.Log("Lever pulled!");
    }

    public void TriggerBoxOpen()
    {
        if (targetBox != null)
        {
            targetBox.OpenContainer();
        }
        else
        {
            Debug.LogError("No box assigned to lever!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (handTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(handTarget.position, 0.1f);
            Gizmos.DrawLine(transform.position, handTarget.position);

            if (elbowHint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(elbowHint.position, 0.1f);
            }
        }
    }
}