using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    public Transform handTarget; // Where robot's hand should go
    public float interactionRange = 2f;
    public LayerMask robotLayer;

    [Header("Animation")]
    public Animator leverAnimator;
    public string pullTriggerName = "Pull";
    public string resetTriggerName = "Reset";

    [Header("Events")]
    public UnityEvent onLeverPulled;

    [Header("IK Hint")]
    public Transform elbowHint; // Optional: helps arm bend correctly

    private bool isPulled = false;

    public void PullLever()
    {
        if (isPulled) return;

        isPulled = true;

        // Play lever animation
        if (leverAnimator != null)
            leverAnimator.SetTrigger(pullTriggerName);

        // Invoke events
        onLeverPulled?.Invoke();

        Debug.Log("Lever pulled!");
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

            // Draw hint for elbow
            if (elbowHint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(elbowHint.position, 0.1f);
            }
        }
    }
}
