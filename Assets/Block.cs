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
    public Color unstableColor = Color.yellow;
    public float emissionIntensity = 2.0f;

    [Header("Physics Settings")]
    public bool isBeingHeld = false;
    public bool isGrounded = false;

    [Header("Snapping")]
    public bool useSnapping = true;
    public float autoSnapDistance = 0.15f;

    // Private references
    private Rigidbody rb;
    private Collider blockCollider;
    private Material ledMaterial;
    private Color currentLEDColor;

    // Connection system
    private List<Block> connectedBlocks = new List<Block>();
    private float connectionCheckRadius = 1.1f; // Slightly larger than block size
    private HashSet<Block> snappedTo = new HashSet<Block>();


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
        // Get components
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

    //void Start()
    //{
    //    UpdateConnectedBlocks();
    //}
    IEnumerator Start()
    {
        yield return new WaitForFixedUpdate(); // wait for physics
        UpdateConnectedBlocks();
    }

    void Update()
    {
        CheckGrounded();
        UpdateVisuals();
    }

    void FixedUpdate()
    {
        if (currentState == BlockState.Immovable && !isBeingHeld)
        {
            LockImmovablePosition();
        }
    }

    // ==================== PUBLIC METHODS ====================

    public void ChangeState(BlockState newState, Block source = null)
    {
        if (currentState == newState) return;

        Debug.Log($"{gameObject.name} changing state from {currentState} to {newState}");
        currentState = newState;

        UpdatePhysicsState();
        UpdateLEDColor();

        // Play effect
        PlayStateChangeEffect();

        // Propagate to connected blocks (excluding source to avoid infinite loops)
        if (source == null) source = this;
        PropagateStateToConnectedBlocks(newState, source);
    }

    public void ToggleState()
    {
        BlockState newState = (currentState == BlockState.Movable)
            ? BlockState.Immovable
            : BlockState.Movable;

        ChangeState(newState);
    }

    public void OnPickup()
    {
        isBeingHeld = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        UpdateLEDColor();
    }

    //public void OnRelease()
    //{
    //    isBeingHeld = false;
    //    UpdatePhysicsState();
    //    UpdateConnectedBlocks();
    //    UpdateLEDColor();
    //}
    //public void OnRelease()
    //{
    //    isBeingHeld = false;

    //    if (currentState == BlockState.Immovable)
    //    {
    //        rb.isKinematic = true;
    //        rb.constraints = RigidbodyConstraints.FreezeAll;
    //    }
    //    else
    //    {
    //        rb.isKinematic = false;
    //        rb.constraints = RigidbodyConstraints.None;
    //    }

    //    UpdateConnectedBlocks();
    //    UpdateLEDColor();
    //}
    public void OnRelease()
    {
        isBeingHeld = false;

        snappedTo.Clear();

        if (currentState == BlockState.Immovable)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        UpdateConnectedBlocks();
        UpdateLEDColor();
    }


    public List<Block> GetConnectedBlocks(bool includeSelf = false)
    {
        List<Block> result = new List<Block>(connectedBlocks);
        if (includeSelf) result.Add(this);
        return result;
    }

    //public void UpdateConnectedBlocks()
    //{
    //    connectedBlocks.Clear();

    //    Collider[] nearbyColliders = Physics.OverlapSphere(
    //        transform.position,
    //        connectionCheckRadius
    //    );

    //    foreach (Collider col in nearbyColliders)
    //    {
    //        Block otherBlock = col.GetComponent<Block>();
    //        if (otherBlock != null && otherBlock != this)
    //        {
    //            if (IsTouching(otherBlock))
    //            {
    //                connectedBlocks.Add(otherBlock);
    //            }
    //        }
    //    }

    //    Debug.Log($"{gameObject.name} is connected to {connectedBlocks.Count} blocks");
    //}
    public void UpdateConnectedBlocks()
    {
        connectedBlocks.Clear();

        foreach (Block block in snappedTo)
        {
            if (block != null)
                connectedBlocks.Add(block);
        }

        Debug.Log($"{gameObject.name} is connected to {connectedBlocks.Count} blocks");
    }


    //void OnCollisionStay(Collision collision)
    //{
    //    Block other = collision.collider.GetComponent<Block>();
    //    if (other == null) return;
    //    if (isBeingHeld) return;

    //    TrySnapToBlock(other);
    //}
    void OnCollisionEnter(Collision collision)
    {
        Block other = collision.collider.GetComponent<Block>();
        if (other == null) return;
        if (isBeingHeld) return;

        // Snap once
        if (!snappedTo.Contains(other))
        {
            TrySnapToBlock(other);
            snappedTo.Add(other);
            other.snappedTo.Add(this);
        }

        // Then update connections
        UpdateConnectedBlocks();
    }


    //void TrySnapToBlock(Block other)
    //{
    //    Vector3 delta = transform.position - other.transform.position;
    //    Vector3 snapDir;

    //    float ax = Mathf.Abs(delta.x);
    //    float ay = Mathf.Abs(delta.y);
    //    float az = Mathf.Abs(delta.z);

    //    // Choose dominant axis (true face detection)
    //    if (ay > ax && ay > az)
    //        snapDir = new Vector3(0, Mathf.Sign(delta.y), 0);
    //    else if (ax > az)
    //        snapDir = new Vector3(Mathf.Sign(delta.x), 0, 0);
    //    else
    //        snapDir = new Vector3(0, 0, Mathf.Sign(delta.z));

    //    float size = blockCollider.bounds.size.x; // assumes cubes
    //    Vector3 snappedPos = other.transform.position + snapDir * size;

    //    rb.position = snappedPos;
    //    rb.linearVelocity = Vector3.zero;
    //    rb.angularVelocity = Vector3.zero;

    //    UpdateConnectedBlocks();
    //}
    void TrySnapToBlock(Block other)
    {
        Bounds a = blockCollider.bounds;
        Bounds b = other.blockCollider.bounds;

        Vector3 delta = a.center - b.center;
        Vector3 snapDir;

        float ax = Mathf.Abs(delta.x);
        float ay = Mathf.Abs(delta.y);
        float az = Mathf.Abs(delta.z);

        // Determine dominant axis
        if (ay > ax && ay > az)
            snapDir = Vector3.up * Mathf.Sign(delta.y);
        else if (ax > az)
            snapDir = Vector3.right * Mathf.Sign(delta.x);
        else
            snapDir = Vector3.forward * Mathf.Sign(delta.z);

        // Distance = sum of extents on that axis
        Vector3 offset =
            new Vector3(
                snapDir.x * (a.extents.x + b.extents.x),
                snapDir.y * (a.extents.y + b.extents.y),
                snapDir.z * (a.extents.z + b.extents.z)
            );

        Vector3 snappedPos = b.center + offset;

        rb.position = snappedPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }


    // ==================== PRIVATE METHODS ====================

    void UpdatePhysicsState()
    {
        if (rb == null) return;

        switch (currentState)
        {
            case BlockState.Movable:
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.None;
                rb.mass = blockWeight;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 0.5f;
                break;

            case BlockState.Immovable:
                if (!isBeingHeld)
                {
                    rb.isKinematic = true; // This makes it immovable
                    rb.constraints = RigidbodyConstraints.FreezeAll;

                    // Also freeze rotation
                    rb.freezeRotation = true;
                }
                break;
        }

        Debug.Log($"{gameObject.name} physics: Kinematic={rb.isKinematic}, Constraints={rb.constraints}");
    }

    void LockImmovablePosition()
    {
        // For extra safety, ensure position doesn't drift
        if (rb.isKinematic)
        {
            // Kinematic rigidbodies stay put automatically
            // But we can also lock transform
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
        }
        else
        {
            targetColor = (currentState == BlockState.Movable) ? movableColor : immovableColor;
        }

        // Add pulsing if held
        if (isBeingHeld)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.7f;
            targetColor *= pulse;
        }

        currentLEDColor = targetColor;
        ledMaterial.SetColor("_EmissionColor", targetColor * emissionIntensity);
        ledMaterial.EnableKeyword("_EMISSION");

        // Force update
        ledRenderer.UpdateGIMaterials();
    }

    void UpdateVisuals()
    {
        // Could add wobble/stability indicators here
    }

    void CheckGrounded()
    {
        float checkDistance = 0.2f;
        Vector3 rayStart = transform.position - Vector3.up * (blockCollider.bounds.extents.y - 0.01f);
        isGrounded = Physics.Raycast(rayStart, Vector3.down, checkDistance);
    }

  
    //protected virtual bool IsTouching(Block otherBlock)
    //{
    //    float tolerance = 0.02f;
    //    return Vector3.Distance(
    //        blockCollider.bounds.ClosestPoint(otherBlock.transform.position),
    //        otherBlock.blockCollider.bounds.ClosestPoint(transform.position)
    //    ) < tolerance;
    //}


    void PropagateStateToConnectedBlocks(BlockState newState, Block source, HashSet<Block> visited = null)
    {
        if (visited == null) visited = new HashSet<Block>();
        if (visited.Contains(this)) return;

        visited.Add(this);

        foreach (Block connectedBlock in connectedBlocks)
        {
            if (connectedBlock != source && !visited.Contains(connectedBlock))
            {
                if (connectedBlock.currentState != newState)
                {
                    connectedBlock.ChangeState(newState, this);
                }

                // Continue propagation
                connectedBlock.PropagateStateToConnectedBlocks(newState, this, visited);
            }
        }
    }

    void PlayStateChangeEffect()
    {
        StartCoroutine(FlashEffect());
    }

    IEnumerator FlashEffect()
    {
        if (ledMaterial == null) yield break;

        Color originalColor = currentLEDColor;
        Color flashColor = Color.white * 5f;

        ledMaterial.SetColor("_EmissionColor", flashColor);
        yield return new WaitForSeconds(0.1f);

        ledMaterial.SetColor("_EmissionColor", originalColor * emissionIntensity);
    }

    //public void TryAutoSnapToGrid()
    //{
    //    if (!useSnapping) return;

    //    // Find nearby blocks to snap to
    //    Collider[] nearbyBlocks = Physics.OverlapSphere(
    //        transform.position,
    //        autoSnapDistance,
    //        LayerMask.GetMask("Block") // Make sure blocks are on "Block" layer
    //    );

    //    Vector3 averagePosition = Vector3.zero;
    //    int count = 0;

    //    foreach (Collider col in nearbyBlocks)
    //    {
    //        if (col.gameObject != gameObject && col.GetComponent<Block>() != null)
    //        {
    //            averagePosition += col.transform.position;
    //            count++;
    //        }
    //    }

    //    if (count > 0)
    //    {
    //        averagePosition /= count;

    //        // Snap to grid based on nearby blocks
    //        float gridSize = 1.0f;
    //        Vector3 gridPosition = new Vector3(
    //            Mathf.Round(averagePosition.x / gridSize) * gridSize,
    //            transform.position.y, // Keep current height
    //            Mathf.Round(averagePosition.z / gridSize) * gridSize
    //        );

    //        // Only snap if it's a significant improvement
    //        if (Vector3.Distance(transform.position, gridPosition) < autoSnapDistance)
    //        {
    //            transform.position = gridPosition;
    //            Debug.Log($"Block auto-snapped to grid at {gridPosition}");
    //        }
    //    }
    //}

    [ContextMenu("Force Update Connections")]
    void ForceUpdateConnections()
    {
        UpdateConnectedBlocks();
    }


    //// Call this when block is released
    //public void OnReleaseWithSnap()
    //{
    //    OnRelease(); // Your existing method

    //    // Try to snap to grid
    //    TryAutoSnapToGrid();
    //}


    // ==================== EDITOR & DEBUG ====================

    void OnDrawGizmosSelected()
    {
        // Connection radius (cyan sphere)
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Semi-transparent cyan
        Gizmos.DrawWireSphere(transform.position, connectionCheckRadius);

        // Connection lines (green)
        Gizmos.color = Color.green;
        foreach (Block connectedBlock in connectedBlocks)
        {
            if (connectedBlock != null)
            {
                Gizmos.DrawLine(transform.position, connectedBlock.transform.position);
            }
        }

        // State indicator cube (red/green wireframe)
        Gizmos.color = (currentState == BlockState.Movable) ? Color.green : Color.red;
        Vector3 size = Vector3.one * 1.05f;
        Gizmos.DrawWireCube(transform.position, size);

        // Ground check ray (yellow if grounded, gray if not)
        Gizmos.color = isGrounded ? Color.yellow : Color.gray;
        Vector3 rayStart = transform.position - Vector3.up * (GetComponent<Collider>().bounds.extents.y - 0.01f);
        Gizmos.DrawRay(rayStart, Vector3.down * 0.2f);

        // Current state text
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
                                 currentState.ToString(),
                                 new GUIStyle()
                                 {
                                     normal = new GUIStyleState() { textColor = Color.white },
                                     fontSize = 10
                                 });
#endif
    }

    void OnValidate()
    {
        // Preview color in editor
        if (ledRenderer != null && ledRenderer.sharedMaterial != null && Application.isEditor && !Application.isPlaying)
        {
            Material tempMat = new Material(ledRenderer.sharedMaterial);
            Color previewColor = (currentState == BlockState.Movable) ? movableColor : immovableColor;
            tempMat.SetColor("_EmissionColor", previewColor * emissionIntensity);
            ledRenderer.sharedMaterial = tempMat;
        }
    }
}
