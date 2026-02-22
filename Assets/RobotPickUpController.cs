using UnityEngine;
//using System.Collections;
public class RobotPickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupDistance = 3f;
    public LayerMask blockLayer;

    [Header("Snapping")]
    public GridSnappingSystem snappingSystem;

    //------------------------------------------------------------
    [Header("Placement Preview")]
    public GameObject placementPreviewPrefab; // Assign a semi-transparent block prefab
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float previewAlpha = 0.5f;
    //---------------------------------------------------------

    private Block heldBlock;
    //private bool canToggleState = true;  // To ensure state toggle works only when not holding a block
    private Quaternion heldRotationOffset;

    //------------------------------------------
    private GameObject previewInstance;
    private Renderer previewRenderer;
    private Material previewMaterial;
    //---------------------------------------------
    //private Vector3 currentPreviewPosition;
    //private bool currentPreviewValid;

    private Animator animator;
    private bool canMoveBlock = false;


    //void Start()
    //{
    //    if (placementPreviewPrefab != null)
    //    {
    //        previewInstance = Instantiate(placementPreviewPrefab);
    //        previewInstance.SetActive(false);

    //        // Setup preview material
    //        previewRenderer = previewInstance.GetComponent<Renderer>();
    //        if (previewRenderer != null)
    //        {
    //            previewMaterial = new Material(previewRenderer.material);
    //            previewRenderer.material = previewMaterial;
    //        }
    //    }
    //    else
    //    {
    //        // Create a simple preview if none provided
    //        CreateSimplePreview();
    //    }
    //}

    void Start()
    {
        animator = GetComponent<Animator>();

        if (placementPreviewPrefab != null)
        {
            previewInstance = Instantiate(placementPreviewPrefab);
            SetupPreviewMaterial();
        }
        else
        {
            CreateSimplePreview();
        }

        previewInstance.SetActive(false);
    }

    void SetupPreviewMaterial()
    {
        previewRenderer = previewInstance.GetComponent<Renderer>();
        if (previewRenderer != null)
        {
            previewMaterial = new Material(previewRenderer.material);
            previewRenderer.material = previewMaterial;
        }
    }

    void CreateSimplePreview()
    {
        previewInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewInstance.name = "PlacementPreview";
        previewInstance.transform.localScale = Vector3.one * 0.98f; // Slightly smaller than blocks

        // Remove collider
        Destroy(previewInstance.GetComponent<Collider>());

        // Create transparent material
        previewMaterial = new Material(Shader.Find("Standard"));
        previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        previewMaterial.EnableKeyword("_ALPHABLEND_ON");
        previewMaterial.renderQueue = 3000;

        previewRenderer = previewInstance.GetComponent<Renderer>();
        previewRenderer.material = previewMaterial;

        previewInstance.SetActive(false);
    }

    void Update()
    {
        HandleInput();

        if (heldBlock != null)
        {
            //MoveHeldBlock();
            //UpdatePlacementPreview();
            if (canMoveBlock)
            {
                MoveHeldBlock();
                UpdatePlacementPreview();
            }
        }
        else if (previewInstance != null && previewInstance.activeSelf)
        {
            previewInstance.SetActive(false);
        }
    }


    void HandleInput()
    {
        // SPACE = Pick up / Drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (heldBlock == null)
                TryPickup();
            else
                DropBlock();
        }

        // F = Toggle state only when not holding the block
        if (Input.GetKeyDown(KeyCode.F) && !heldBlock)
        {
            ToggleTargetBlock();
        }
    }

    void TryPickup()
    {
        Block target = GetBlockInFront();

        if (target != null && target.currentState == Block.BlockState.Movable)
        {
            heldBlock = target;

            //// Store relative rotation
            heldRotationOffset = Quaternion.Inverse(transform.rotation) * heldBlock.transform.rotation;

            canMoveBlock = false;

            animator.SetTrigger("Pickup");
            //StartCoroutine(GrabBlockAfterDelay(1.5f));
            GetComponent<RobotController>().isHoldingBlock = true;

            //heldBlock.OnPickup();



            //GetComponent<RobotController>().isHoldingBlock = true;

            //Show preview when holding
            //if (previewInstance != null)
            //    previewInstance.SetActive(true);

            if (previewInstance != null)
                previewInstance.SetActive(false);
        }
    }

    //IEnumerator GrabBlockAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);

    //    if (heldBlock != null)
    //    {
    //        heldBlock.OnPickup();
    //        heldRotationOffset = Quaternion.Inverse(transform.rotation) * heldBlock.transform.rotation;
    //        GetComponent<RobotController>().isHoldingBlock = true;
    //        animator.SetBool("isHolding", true);
    //    }
    //}

    //void DropBlock()
    //{
    //    if (heldBlock == null) return;

    //    bool placed = snappingSystem.TryPlace(
    //        heldBlock,
    //        heldBlock.transform.position
    //    );

    //    if (!placed)
    //    {
    //        Debug.Log("Cannot place here!");
    //        return;
    //    }

    //    heldBlock = null;
    //    GetComponent<RobotController>().isHoldingBlock = false;
    //}
    void DropBlock()
    {
        if (heldBlock == null) return;

        Vector3 previewPos;
        bool isValid;
        snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

        if (!isValid)
        {
            Debug.Log("Cannot place here - position invalid");
            animator.SetTrigger("No");
            // Optional: Play a "cannot place" sound/effect
            return; // DON'T trigger animation
        }

    

        animator.SetTrigger("Place");
        //StartCoroutine(ReleaseBlockAfterDelay(0.3f));

        //bool placed = snappingSystem.TryPlace(
        //    heldBlock,
        //    heldBlock.transform.position
        //);

        //if (placed)
        //{
        //    heldBlock = null;
        //    GetComponent<RobotController>().isHoldingBlock = false;

        //    // Hide preview after placing
        //    if (previewInstance != null)
        //        previewInstance.SetActive(false);
        //}
        //else
        //{
        //    // Optional: feedback that placement failed
        //    Debug.Log("Cannot place here!");
        //}
    }

    //IEnumerator ReleaseBlockAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);

    //    if (heldBlock != null)
    //    {
    //        bool placed = snappingSystem.TryPlace(heldBlock, heldBlock.transform.position);

    //        if (placed)
    //        {
    //            heldBlock = null;
    //            GetComponent<RobotController>().isHoldingBlock = false;
    //            animator.SetBool("isHolding", false);
    //        }

    //        if (previewInstance != null)
    //            previewInstance.SetActive(false);
    //    }
    //}
    //--------------------------------
    void MoveHeldBlock()
    {
        heldBlock.transform.position = holdPoint.position;
        heldBlock.transform.rotation = transform.rotation * heldRotationOffset;
    }
    //--------------------------------------------------------


    //void ToggleTargetBlock()
    //{
    //    Block target = GetBlockInFront();

    //    if (target != null)
    //    {
    //        target.ToggleState();
    //    }
    //}

    //Block GetBlockInFront()
    //{
    //    Vector3 origin = transform.position + Vector3.up * 1f;
    //    Vector3 halfExtents = new Vector3(0.5f, 1.0f, 0.5f);

    //    RaycastHit[] hits = Physics.BoxCastAll(
    //        origin,
    //        halfExtents,
    //        transform.forward,
    //        transform.rotation,
    //        pickupDistance,
    //        blockLayer
    //    );

    //    Block highestBlock = null;
    //    float highestY = float.MinValue;

    //    foreach (RaycastHit hit in hits)
    //    {
    //        Block block = hit.collider.GetComponent<Block>();

    //        if (block == null) continue;
    //        if (block.isBeingHeld) continue;
    //        if (block.currentState != Block.BlockState.Movable) continue;

    //        float y = block.transform.position.y;

    //        if (y > highestY)
    //        {
    //            highestY = y;
    //            highestBlock = block;
    //        }
    //    }

    //    return highestBlock;
    //}
    void UpdatePlacementPreview()
    {
        if (previewInstance == null || snappingSystem == null || heldBlock == null) return;

        // Get where the block WOULD land if dropped NOW
        Vector3 previewPos;
        bool isValid;

        snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

        // Store for debug
        //currentPreviewPosition = previewPos;
        //currentPreviewValid = isValid;

        // Position the preview at the DROP location, not at the held block
        previewInstance.transform.position = previewPos;
        previewInstance.transform.rotation = heldBlock.transform.rotation;

        // Update color based on validity
        if (previewMaterial != null)
        {
            Color previewColor = isValid ? validColor : invalidColor;
            previewColor.a = previewAlpha;
            previewMaterial.color = previewColor;

            // Debug log to confirm color changes
            //Debug.Log($"Preview at {previewPos} - Valid: {isValid} - Color: {(isValid ? "GREEN" : "RED")}");
        }

        previewInstance.SetActive(true);
    }

    void ToggleTargetBlock()
    {
        Block target = GetBlockInFront();

        if (target != null)
        {
            target.ToggleState();
           // Debug.Log($"Toggled {target.name} to {target.currentState}");
        }
        else
        {
            Debug.Log("No block in front to toggle");
        }
    }

    Block GetBlockInFront()
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 halfExtents = new Vector3(0.5f, 1.0f, 0.5f);

        RaycastHit[] hits = Physics.BoxCastAll(
            origin,
            halfExtents,
            transform.forward,
            transform.rotation,
            pickupDistance,
            blockLayer
        );

        // REMOVED falling block check - you don't need it anymore

        // Pick the highest block (for stacking)
        Block highestBlock = null;
        float highestY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            Block block = hit.collider.GetComponent<Block>();
            // REMOVED state filter - F key should work on any block
            if (block != null && !block.isBeingHeld)
            {
                float y = block.transform.position.y;
                if (y > highestY)
                {
                    highestY = y;
                    highestBlock = block;
                }
            }
        }

        return highestBlock;
    }

    void OnDestroy()
    {
        if (previewInstance != null)
            Destroy(previewInstance);
    }

    // Called by Animation Event at the exact frame hand touches block
    public void AnimationEvent_GrabBlock()
    {
        if (heldBlock != null)
        {
            heldBlock.OnPickup();
            //heldRotationOffset = Quaternion.Inverse(transform.rotation) * heldBlock.transform.rotation;
            GetComponent<RobotController>().isHoldingBlock = true;
            animator.SetBool("isHolding", true);

            canMoveBlock = true;

            // Show preview AFTER block is grabbed
            if (previewInstance != null)
                previewInstance.SetActive(true);

            Debug.Log("Animation Event: Grabbed block at exact frame");
        }
    }


    // Called by Animation Event at the exact frame hand releases block
    //public void AnimationEvent_ReleaseBlock()
    //{
    //    if (heldBlock != null)
    //    {
    //        bool placed = snappingSystem.TryPlace(heldBlock, heldBlock.transform.position);

    //        if (placed)
    //        {
    //            heldBlock = null;
    //            canMoveBlock = false;
    //            GetComponent<RobotController>().isHoldingBlock = false;
    //            animator.SetBool("isHolding", false);
    //            Debug.Log("Animation Event: Released block at exact frame");
    //        }
    //    }
    //}

    public void AnimationEvent_ReleaseBlock()
    {
        if (heldBlock != null)
        {
            // Double-check validity (in case something changed during animation)
            Vector3 previewPos;
            bool isValid;
            snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

            if (!isValid)
            {
                Debug.LogWarning("Position became invalid during placement animation!");
                // Return to holding state
                animator.SetBool("isHolding", true);
                return;
            }

            bool placed = snappingSystem.TryPlace(heldBlock, heldBlock.transform.position);

            if (placed)
            {
                heldBlock = null;
                canMoveBlock = false;
                GetComponent<RobotController>().isHoldingBlock = false;
                animator.SetBool("isHolding", false);

                if (previewInstance != null)
                    previewInstance.SetActive(false);

                Debug.Log("Animation Event: Released block at exact frame");
            }
            else
            {
                Debug.LogWarning("TryPlace failed despite valid preview!");
                // Return to holding state
                animator.SetBool("isHolding", true);
            }
        }
    }

    // Optional: Called when pickup animation completes
    public void AnimationEvent_PickupComplete()
    {
        // Already transitioning to HoldBlock automatically
        Debug.Log("Pickup animation complete");
    }
}


