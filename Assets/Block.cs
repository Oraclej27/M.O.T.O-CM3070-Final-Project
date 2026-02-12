using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockType blockType = BlockType.Cube;
    public BlockState currentState = BlockState.Movable;
    public float blockWeight = 1.0f;

    [Header("Visual Settings")]
    public Renderer ledRenderer;
    public Color movableColor = Color.green;
    public Color immovableColor = Color.red;
    public Color heldColor = Color.blue;
    public float emissionIntensity = 2.0f;

    [Header("Physics Settings")]
    public bool isBeingHeld = false;
    public bool isGrounded = false;

    // Private references
    private Rigidbody rb;
    private Collider blockCollider;
    private Material ledMaterial;
    private Color currentLEDColor;

    // Connection system
    private List<Block> connectedBlocks = new List<Block>();
    //private HashSet<Block> snappedTo = new HashSet<Block>();
    private float connectionCheckDistance = 1.1f; // For scanning adjacent blocks

    // State management
    public enum BlockState
    {
        Movable,    // Normal physics, can be picked up
        Immovable   // Fixed position, acts as wall/floor
    }

    public enum BlockType
    {
        Cube,
        Ramp,
        Special
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        blockCollider = GetComponent<Collider>();

        // Setup LED material
        if (ledRenderer != null)
        {
            ledMaterial = new Material(ledRenderer.material);
            ledRenderer.material = ledMaterial;
        }

        UpdatePhysicsState();
        UpdateLEDColor();
    }

    void Update()
    {
        CheckGrounded();
    }

    void FixedUpdate()
    {
        if (currentState == BlockState.Immovable && !isBeingHeld)
        {
            LockImmovablePosition();
        }
    }

    // ==================== PUBLIC METHODS ====================

    public void ToggleState()
    {
        // Switch between Movable and Immovable
        currentState = (currentState == BlockState.Movable)
            ? BlockState.Immovable
            : BlockState.Movable;

        UpdatePhysicsState();
        UpdateLEDColor();

        Debug.Log($"{gameObject.name} toggled to {currentState}");
    }

    public void OnPickup()
    {
        isBeingHeld = true;

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None;

        UpdateLEDColor();
    }

    public void OnRelease()
    {
        isBeingHeld = false;
        //snappedTo.Clear();
        rb.useGravity = true;
        UpdateLEDColor();
    }

    // ==================== PRIVATE METHODS ====================

    void UpdatePhysicsState()
    {
        if (rb == null) return;

        if (currentState == BlockState.Movable)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.mass = blockWeight;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.useGravity = true;
        }
        else if (currentState == BlockState.Immovable && !isBeingHeld)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.useGravity = false;
        }
    }

    void LockImmovablePosition()
    {
        if (rb.isKinematic)
        {
            rb.MovePosition(transform.position);
            rb.MoveRotation(transform.rotation);
        }
    }

    void UpdateLEDColor()
    {
        if (ledMaterial == null) return;

        Color targetColor;

        if (isBeingHeld)
        {
            targetColor = heldColor;
            // Pulse when held
            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.7f;
            targetColor *= pulse;
        }
        else
        {
            targetColor = (currentState == BlockState.Movable) ? movableColor : immovableColor;
        }

        currentLEDColor = targetColor;
        ledMaterial.SetColor("_EmissionColor", targetColor * emissionIntensity);
        ledMaterial.EnableKeyword("_EMISSION");
        ledRenderer.UpdateGIMaterials();
    }

    void CheckGrounded()
    {
        if (blockCollider == null) return;

        float checkDistance = 0.2f;
        Vector3 rayStart = transform.position - Vector3.up * (blockCollider.bounds.extents.y - 0.01f);
        isGrounded = Physics.Raycast(rayStart, Vector3.down, checkDistance);
    }

    // ==================== EDITOR & DEBUG ====================

    void OnDrawGizmosSelected()
    {
        // Connection lines (green)
        Gizmos.color = Color.green;
        foreach (Block connectedBlock in connectedBlocks)
        {
            if (connectedBlock != null)
            {
                Gizmos.DrawLine(transform.position, connectedBlock.transform.position);
            }
        }

        // State indicator (red/green wireframe)
        Gizmos.color = (currentState == BlockState.Movable) ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.05f);

        // Connection scan radius (cyan - semi-transparent)
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, connectionCheckDistance);

        // Ground check ray
        Gizmos.color = isGrounded ? Color.yellow : Color.gray;
        if (blockCollider != null)
        {
            Vector3 rayStart = transform.position - Vector3.up * (blockCollider.bounds.extents.y - 0.01f);
            Gizmos.DrawRay(rayStart, Vector3.down * 0.2f);
        }

        // State text
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
                                 currentState.ToString(),
                                 new GUIStyle()
                                 {
                                     normal = new GUIStyleState() { textColor = Color.white },
                                     fontSize = 10,
                                     fontStyle = FontStyle.Bold
                                 });
#endif
    }

    void OnValidate()
    {
        // Preview color in editor (non-play mode only)
        if (ledRenderer != null && ledRenderer.sharedMaterial != null && !Application.isPlaying)
        {
            Material tempMat = new Material(ledRenderer.sharedMaterial);
            Color previewColor = (currentState == BlockState.Movable) ? movableColor : immovableColor;
            tempMat.SetColor("_EmissionColor", previewColor * emissionIntensity);
            ledRenderer.sharedMaterial = tempMat;
        }
    }
}