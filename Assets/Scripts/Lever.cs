using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    public Transform handTarget; 
    public float interactionRange = 2f;
    public LayerMask robotLayer;

    [Header("Animation")]
    public Animator leverAnimator;
    public string pullTriggerName = "Pull";
    public string resetTriggerName = "Reset";

    [Header("Events")]
    public UnityEvent onLeverPulled;

    [Header("IK Hint")]
    public Transform elbowHint; 

    [Header("Box Connection")]
    public BoxContainer targetBox;

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
