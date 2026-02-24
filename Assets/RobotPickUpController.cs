using UnityEngine;
using System.Collections;


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
    public GameObject placementPreviewPrefab; 
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float previewAlpha = 0.5f;
    //---------------------------------------------------------

    private Block heldBlock;
    private Quaternion heldRotationOffset;

    //------------------------------------------
    private GameObject previewInstance;
    private Renderer previewRenderer;
    private Material previewMaterial;
    //---------------------------------------------

    [Header("Lever Interaction")]
    public LayerMask leverLayer;
    public float leverInteractionRange = 3f;
    public RobotIKController ikController;

    private Lever nearbyLever;
    private bool isInteracting = false;

    //-------------------------------------------
    private Animator animator;
    private bool canMoveBlock = false;
    private string currentLeverID;


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

        if (ikController == null)
            ikController = GetComponent<RobotIKController>();
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

    //void Update()
    //{
    //    HandleInput();

    //    if (!isInteracting)
    //    {
    //        CheckForNearbyLever();
    //    }

    //    if (heldBlock != null)
    //    {

    //        if (canMoveBlock)
    //        {
    //            MoveHeldBlock();
    //            UpdatePlacementPreview();
    //        }
    //    }
    //    else if (previewInstance != null && previewInstance.activeSelf)
    //    {
    //        previewInstance.SetActive(false);
    //    }
    //}

    void Update()
    {
        HandleInput();

        if (!isInteracting && heldBlock == null)
        {
            CheckForNearbyLever();
        }

        if (heldBlock != null)
        {
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
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (heldBlock == null)
        //        TryPickup();
        //    else
        //        DropBlock();

        //}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"SPACE pressed - nearbyLever: {nearbyLever?.name ?? "null"}, heldBlock: {heldBlock?.name ?? "null"}, isInteracting: {isInteracting}");
            if (isInteracting) return; // Already interacting
            Debug.Log("Already interacting");

            if (nearbyLever != null && heldBlock == null)
            {
                // Interact with lever
                Debug.Log($"Lever detected: {nearbyLever.name}");
                StartLeverInteraction();
            }
            else if (heldBlock == null)
            {
                TryPickup();
            }
            else
            {
                DropBlock();
            }
        }

        // F = Toggle state only when not holding the block
        if (Input.GetKeyDown(KeyCode.F) && !heldBlock)
        {
            ToggleTargetBlock();
        }
    }

    void CheckForNearbyLever()
    {
        Collider[] levers = Physics.OverlapSphere(transform.position, leverInteractionRange, leverLayer);
        Debug.Log($"Found {levers.Length} colliders on Lever layer");

        for (int i = 0; i < levers.Length; i++)
        {
            Collider col = levers[i];
            Debug.Log($"Collider {i}: {col.gameObject.name} on layer {LayerMask.LayerToName(col.gameObject.layer)}");

            Lever lever = col.GetComponent<Lever>();
            if (lever != null)
            {
                nearbyLever = lever;
                Debug.Log($"Found Lever component on {col.gameObject.name}!");
                return;
            }
            else
            {
                Debug.Log($"No Lever component on {col.gameObject.name}");
            }
        }

        nearbyLever = null;
        Debug.Log("No lever found with Lever component");
    }

    void StartLeverInteraction()
    {
        if (nearbyLever == null) return;

        isInteracting = true;
        currentLeverID = nearbyLever.name; // Store the lever's name

        Debug.Log($"Starting lever interaction with {currentLeverID}");

        Vector3 toLever = (nearbyLever.handTarget.position - transform.position).normalized;
        toLever.y = 0;

        StartCoroutine(RotateToFace(toLever, () => {
            animator.SetTrigger("PullLever");
        }));
    }

    IEnumerator RotateToFace(Vector3 direction, System.Action onComplete)
    {
        Debug.Log("Starting rotation");
        float duration = 0.3f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(direction);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        Debug.Log("Rotation complete - calling onComplete");
        onComplete?.Invoke();
    }

    // Animation Event called during pull animation
    public void AnimationEvent_LeverPull(string leverName)  //  STRING parameter
    {
        Debug.Log($"EVENT FIRED for lever: {leverName}");

        // Find the lever by name
        GameObject leverObj = GameObject.Find(leverName);
        if (leverObj == null)
        {
            Debug.LogError($"Could not find lever named {leverName}");
            return;
        }

        Lever lever = leverObj.GetComponent<Lever>();
        if (lever == null) return;

        ikController.GrabLever(lever);
        lever.PullLever();
    }

    // Animation Event when pull completes
    public void AnimationEvent_LeverPullComplete()
    {
        ikController.ReleaseLever();
        isInteracting = false;
        nearbyLever = null;
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
            GetComponent<RobotController>().isHoldingBlock = true;


            if (previewInstance != null)
                previewInstance.SetActive(false);
        }
    }

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
      
    }

    //--------------------------------
    void MoveHeldBlock()
    {
        heldBlock.transform.position = holdPoint.position;
        heldBlock.transform.rotation = transform.rotation * heldRotationOffset;
    }
    //--------------------------------------------------------


    void UpdatePlacementPreview()
    {
        if (previewInstance == null || snappingSystem == null || heldBlock == null) return;

        // Get where the block WOULD land if dropped NOW
        Vector3 previewPos;
        bool isValid;

        snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

        // Position the preview at the DROP location, not at the held block
        previewInstance.transform.position = previewPos;
        previewInstance.transform.rotation = heldBlock.transform.rotation;

        // Update color based on validity
        if (previewMaterial != null)
        {
            Color previewColor = isValid ? validColor : invalidColor;
            previewColor.a = previewAlpha;
            previewMaterial.color = previewColor;

        }

        previewInstance.SetActive(true);
    }

    void ToggleTargetBlock()
    {
        Block target = GetBlockInFront();

        if (target != null)
        {
            target.ToggleState();
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
            GetComponent<RobotController>().isHoldingBlock = true;
            animator.SetBool("isHolding", true);

            canMoveBlock = true;

            // Show preview AFTER block is grabbed
            if (previewInstance != null)
                previewInstance.SetActive(true);

            Debug.Log("Animation Event: Grabbed block at exact frame");
        }
    }


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


