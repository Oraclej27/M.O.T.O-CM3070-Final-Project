//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Block : MonoBehaviour
//{
//    [Header("Block Settings")]
//    public BlockType blockType = BlockType.Cube;
//    public BlockState currentState = BlockState.Movable;
//    public float blockWeight = 1.0f;

//    [Header("Visual Settings")]
//    public Renderer ledRenderer;
//    public Color movableColor = Color.green;
//    public Color immovableColor = Color.red;
//    public Color heldColor = Color.blue;
//    public Color unstableColor = Color.yellow;
//    public float emissionIntensity = 2.0f;

//    [Header("Physics Settings")]
//    public bool isBeingHeld = false;
//    public bool isGrounded = false;

//    // Private references
//    private Rigidbody rb;
//    private Collider blockCollider;
//    private Material ledMaterial;
//    private Color currentLEDColor;

//    // Connection system
//    private List<Block> connectedBlocks = new List<Block>();
//    private float connectionCheckRadius = 1.1f; // Slightly larger than block size

//    // State management
//    public enum BlockState
//    {
//        Movable,    // Normal physics, can be picked up
//        Immovable   // Fixed position, acts as wall/floor
//    }

//    public enum BlockType
//    {
//        Cube,
//        Ramp,
//        Special // For future expansion
//    }

//    void Awake()
//    {
//        // Get components
//        rb = GetComponent<Rigidbody>();
//        blockCollider = GetComponent<Collider>();

//        // Setup LED material (use instance to not affect prefab)
//        if (ledRenderer != null)
//        {
//            ledMaterial = new Material(ledRenderer.material);
//            ledRenderer.material = ledMaterial;
//            UpdateLEDColor();
//        }

//        // Set initial physics based on state
//        UpdatePhysicsState();
//    }

//    void Start()
//    {
//        // Initial connection check
//        UpdateConnectedBlocks();
//    }

//    void Update()
//    {
//        // Check if grounded
//        CheckGrounded();

//        // Visual updates
//        UpdateVisuals();
//    }

//    void FixedUpdate()
//    {
//        // Physics updates
//        if (currentState == BlockState.Immovable && !isBeingHeld)
//        {
//            LockImmovablePosition();
//        }
//    }

//    // ==================== PUBLIC METHODS ====================

//    public void ChangeState(BlockState newState)
//    {
//        if (currentState == newState) return;

//        currentState = newState;
//        UpdatePhysicsState();
//        UpdateLEDColor();

//        // Propagate to connected blocks
//        PropagateStateToConnectedBlocks(newState);

//        // Visual/audio feedback
//        PlayStateChangeEffect();
//    }

//    public void ToggleState()
//    {
//        BlockState newState = (currentState == BlockState.Movable)
//            ? BlockState.Immovable
//            : BlockState.Movable;

//        ChangeState(newState);
//    }

//    public void OnPickup()
//    {
//        isBeingHeld = true;

//        // If immovable, temporarily become movable while held
//        if (currentState == BlockState.Immovable)
//        {
//            rb.isKinematic = false;
//            rb.constraints = RigidbodyConstraints.None;
//        }

//        // Disable collisions with player (to avoid pushing)
//        // This will be handled by the pickup system

//        UpdateLEDColor();
//    }

//    public void OnRelease()
//    {
//        isBeingHeld = false;

//        // Re-enable physics constraints based on state
//        UpdatePhysicsState();

//        // Re-check connections
//        UpdateConnectedBlocks();

//        UpdateLEDColor();
//    }

//    public List<Block> GetConnectedBlocks(bool includeSelf = false)
//    {
//        List<Block> result = new List<Block>(connectedBlocks);
//        if (includeSelf) result.Add(this);
//        return result;
//    }

//    public void UpdateConnectedBlocks()
//    {
//        connectedBlocks.Clear();

//        // Find nearby blocks
//        Collider[] nearbyColliders = Physics.OverlapSphere(
//            transform.position,
//            connectionCheckRadius
//        );

//        foreach (Collider col in nearbyColliders)
//        {
//            Block otherBlock = col.GetComponent<Block>();
//            if (otherBlock != null && otherBlock != this)
//            {
//                // Check if actually touching (not just nearby)
//                if (IsTouching(otherBlock))
//                {
//                    connectedBlocks.Add(otherBlock);
//                }
//            }
//        }
//    }

//    // ==================== PRIVATE METHODS ====================

//    void UpdatePhysicsState()
//    {
//        if (rb == null) return;

//        switch (currentState)
//        {
//            case BlockState.Movable:
//                rb.isKinematic = false;
//                rb.constraints = RigidbodyConstraints.None;
//                rb.mass = blockWeight;
//                break;

//            case BlockState.Immovable:
//                if (!isBeingHeld) // Don't lock if being held
//                {
//                    rb.isKinematic = true; // Or use constraints for physics interaction
//                    rb.constraints = RigidbodyConstraints.FreezeAll;
//                }
//                break;
//        }
//    }

//    void LockImmovablePosition()
//    {
//        // Keep immovable blocks in place (kinematic does this automatically)
//        // Alternatively, use constraints for slight physics interaction
//        // transform.position = originalPosition; // If using constraints
//    }

//    void UpdateLEDColor()
//    {
//        if (ledMaterial == null) return;

//        Color targetColor;

//        if (isBeingHeld)
//        {
//            targetColor = heldColor;
//        }
//        else
//        {
//            switch (currentState)
//            {
//                case BlockState.Movable:
//                    targetColor = movableColor;
//                    break;
//                case BlockState.Immovable:
//                    targetColor = immovableColor;
//                    break;
//                default:
//                    targetColor = movableColor;
//                    break;
//            }
//        }

//        // Add pulsing effect if held
//        if (isBeingHeld)
//        {
//            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.7f;
//            targetColor *= pulse;
//        }

//        currentLEDColor = targetColor;
//        ledMaterial.SetColor("_EmissionColor", targetColor * emissionIntensity);
//        ledMaterial.EnableKeyword("_EMISSION");
//    }

//    void UpdateVisuals()
//    {
//        // Update LED
//        UpdateLEDColor();

//        // Add any other visual effects here
//        // (e.g., unstable warning if stacked too high)
//    }

//    void CheckGrounded()
//    {
//        // Simple ground check
//        float checkDistance = 0.1f;
//        Vector3 checkStart = transform.position - Vector3.up * (blockCollider.bounds.extents.y - 0.01f);

//        isGrounded = Physics.Raycast(checkStart, Vector3.down, checkDistance);

//        // Optional: Visual feedback for unstable stacks
//        if (!isGrounded && currentState == BlockState.Movable && !isBeingHeld)
//        {
//            // Could trigger warning LED
//        }
//    }

//    protected virtual bool IsTouching(Block otherBlock)
//    {
//        // More precise touch detection
//        float touchThreshold = 0.05f; // 5cm tolerance
//        float distance = Vector3.Distance(transform.position, otherBlock.transform.position);

//        // Approximate block size (assuming 1x1x1 cube)
//        float combinedSize = 1.0f + touchThreshold;

//        return distance <= combinedSize;
//    }

//    void PropagateStateToConnectedBlocks(BlockState newState, int depth = 0, int maxDepth = 10)
//    {
//        // Safety: prevent infinite recursion
//        if (depth >= maxDepth) return;

//        foreach (Block connectedBlock in connectedBlocks)
//        {
//            if (connectedBlock.currentState != newState)
//            {
//                connectedBlock.ChangeState(newState);

//                // Recursively propagate (with depth limit)
//                connectedBlock.PropagateStateToConnectedBlocks(newState, depth + 1, maxDepth);
//            }
//        }
//    }

//    void PlayStateChangeEffect()
//    {
//        // Placeholder for effects
//        // We'll add particle system and sound later

//        // Quick flash effect
//        StartCoroutine(FlashEffect());
//    }

//    IEnumerator FlashEffect()
//    {
//        if (ledMaterial == null) yield break;

//        Color originalColor = currentLEDColor;
//        Color flashColor = Color.white * 3f;

//        ledMaterial.SetColor("_EmissionColor", flashColor);
//        yield return new WaitForSeconds(0.1f);

//        ledMaterial.SetColor("_EmissionColor", originalColor * emissionIntensity);
//    }

//    // ==================== EDITOR & DEBUG ====================

//    void OnDrawGizmosSelected()
//    {
//        // Draw connection radius
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(transform.position, connectionCheckRadius);

//        // Draw connections to other blocks
//        Gizmos.color = Color.green;
//        foreach (Block connectedBlock in connectedBlocks)
//        {
//            if (connectedBlock != null)
//            {
//                Gizmos.DrawLine(transform.position, connectedBlock.transform.position);
//            }
//        }

//        // Draw state indicator
//        Gizmos.color = (currentState == BlockState.Movable) ? Color.green : Color.red;
//        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);
//    }

//    void OnValidate()
//    {
//        // Update color in editor for preview
//        if (ledRenderer != null && ledRenderer.sharedMaterial != null)
//        {
//            Material tempMat = new Material(ledRenderer.sharedMaterial);
//            Color previewColor = (currentState == BlockState.Movable) ? movableColor : immovableColor;
//            tempMat.SetColor("_EmissionColor", previewColor * emissionIntensity);
//            ledRenderer.sharedMaterial = tempMat;
//        }
//    }
//}
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

    // Private references
    private Rigidbody rb;
    private Collider blockCollider;
    private Material ledMaterial;
    private Color currentLEDColor;

    // Connection system
    private List<Block> connectedBlocks = new List<Block>();
    private float connectionCheckRadius = 1.1f; // Slightly larger than block size

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

    void Start()
    {
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

        // Even immovable blocks should be movable while held
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;

        UpdateLEDColor();
    }

    public void OnRelease()
    {
        isBeingHeld = false;

        // Re-enable physics based on state
        UpdatePhysicsState();

        // Re-check connections
        UpdateConnectedBlocks();

        UpdateLEDColor();
    }

    public List<Block> GetConnectedBlocks(bool includeSelf = false)
    {
        List<Block> result = new List<Block>(connectedBlocks);
        if (includeSelf) result.Add(this);
        return result;
    }

    public void UpdateConnectedBlocks()
    {
        connectedBlocks.Clear();

        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            connectionCheckRadius
        );

        foreach (Collider col in nearbyColliders)
        {
            Block otherBlock = col.GetComponent<Block>();
            if (otherBlock != null && otherBlock != this)
            {
                if (IsTouching(otherBlock))
                {
                    connectedBlocks.Add(otherBlock);
                }
            }
        }

        Debug.Log($"{gameObject.name} is connected to {connectedBlocks.Count} blocks");
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

                    // Set velocity to zero
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
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

    protected virtual bool IsTouching(Block otherBlock)
    {
        // Check if blocks are within touching distance
        float distance = Vector3.Distance(transform.position, otherBlock.transform.position);
        float blockSize = 1.0f; // Assuming 1x1x1 cubes
        float tolerance = 0.05f;

        return distance <= blockSize + tolerance;
    }

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