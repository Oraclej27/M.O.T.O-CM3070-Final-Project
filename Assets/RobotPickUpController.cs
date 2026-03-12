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

    [Header("Placement Preview")]
    public GameObject placementPreviewPrefab; 
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float previewAlpha = 0.5f;

    public Block heldBlock;
    private Quaternion heldRotationOffset;

  
    private GameObject previewInstance;
    private Renderer previewRenderer;
    private Material previewMaterial;

    [Header("Lever Interaction")]
    public LayerMask leverLayer;
    public float leverInteractionRange = 3f;
    public RobotIKController ikController;

    private Lever nearbyLever;
    private bool isInteracting = false;

  
    private Animator animator;
    private bool canMoveBlock = false;

    private Lever currentInteractionLever;
    private Collider[] heldBlockColliders;
    private SoundController soundController;


    void Start()
    {
        animator = GetComponent<Animator>();
        soundController = FindFirstObjectByType<SoundController>();

        if (placementPreviewPrefab != null)
        {
            previewInstance = Instantiate(placementPreviewPrefab);
            SetupPreviewMaterial();
        }
        else
        {
            CreatePreview();
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

    void CreatePreview()
    {
        previewInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewInstance.name = "PlacementPreview";
        previewInstance.transform.localScale = Vector3.one * 0.98f; 

        Destroy(previewInstance.GetComponent<Collider>());

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
       
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isInteracting) return; 

            if (nearbyLever != null && heldBlock == null)
            {
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

        if (Input.GetKeyDown(KeyCode.F) && !heldBlock)
        {
            ToggleTargetBlock();
        }
    }

    void CheckForNearbyLever()
    {
        Collider[] levers = Physics.OverlapSphere(transform.position, leverInteractionRange, leverLayer);

        for (int i = 0; i < levers.Length; i++)
        {
            Collider col = levers[i];

            Lever lever = col.GetComponent<Lever>();
            if (lever != null)
            {
                nearbyLever = lever;
                return;
            }
            else
            {
                //Debug.Log($"No Lever  on {col.gameObject.name}");
            }
        }

        nearbyLever = null;
    }

    void StartLeverInteraction()
    {
        if (nearbyLever == null) return;

        isInteracting = true;
        currentInteractionLever = nearbyLever;

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

    public void AnimationEvent_LeverPull()
    {
        if (currentInteractionLever == null)
        {
            Debug.LogError("No lever stored for interaction!");
            return;
        }

        Debug.Log($"EVENT FIRED for lever: {currentInteractionLever.name}");

        if (SoundController.Instance != null)
            SoundController.Instance.PlayLeverPullSound();

        ikController.GrabLever(currentInteractionLever);
        currentInteractionLever.PullLever();
    }

    public void AnimationEvent_LeverPullComplete()
    {
        ikController.ReleaseLever();
        isInteracting = false;
        currentInteractionLever = null; 
        nearbyLever = null;
    }

    void TryPickup()
    {
        Block target = GetBlockInFront();

        if (target != null && target.currentState == Block.BlockState.Movable)
        {
            heldBlock = target;

            heldRotationOffset = Quaternion.Inverse(transform.rotation) * heldBlock.transform.rotation;

            canMoveBlock = false;

            animator.SetTrigger("Pickup");
            GetComponent<RobotController>().isHoldingBlock = true;

            if (previewInstance != null)
                previewInstance.SetActive(false);
        }
    }

    public void DropBlock()
    {
        if (heldBlock == null) return;

        Vector3 previewPos;
        bool isValid;
        snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

        if (!isValid)
        {
            animator.SetTrigger("No");
            return; 
        }
        animator.SetTrigger("Place");
    }

    void MoveHeldBlock()
    {
        heldBlock.transform.position = holdPoint.position;
        heldBlock.transform.rotation = transform.rotation * heldRotationOffset;
    }

    void UpdatePlacementPreview()
    {
        if (previewInstance == null || snappingSystem == null || heldBlock == null) return;

        Vector3 previewPos;
        bool isValid;

        snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

        previewInstance.transform.position = previewPos;
        previewInstance.transform.rotation = heldBlock.transform.rotation;

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

        Block highestBlock = null;
        float highestY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            Block block = hit.collider.GetComponent<Block>();
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

    public void AnimationEvent_GrabBlock()
    {
        if (heldBlock != null)
        {
            if (SoundController.Instance != null)
                SoundController.Instance.PlayPickupSound();

            heldBlock.OnPickup();
            GetComponent<RobotController>().isHoldingBlock = true;
            animator.SetBool("isHolding", true);

            canMoveBlock = true;

            heldBlockColliders = heldBlock.GetComponentsInChildren<Collider>();
            Collider robotCollider = GetComponent<Collider>();

            foreach (Collider blockCollider in heldBlockColliders)
            {
                if (blockCollider != null)
                {
                    Physics.IgnoreCollision(robotCollider, blockCollider, true);
                    Debug.Log($"Ignoring collision with {blockCollider.name}");
                }
            }

            if (previewInstance != null)
                previewInstance.SetActive(true);
        }
    }

    public void AnimationEvent_ReleaseBlock()
    {
        if (heldBlock != null && heldBlockColliders != null)
        {
            Collider robotCollider = GetComponent<Collider>();

            foreach (Collider blockCollider in heldBlockColliders)
            {
                if (blockCollider != null)
                {
                    Physics.IgnoreCollision(robotCollider, blockCollider, false);
                    Debug.Log($"Re-enabling collision with {blockCollider.name}");
                }
            }

            heldBlockColliders = null;

            Vector3 previewPos;
            bool isValid;
            snappingSystem.GetPlacementInfo(heldBlock.transform.position, out previewPos, out isValid);

            if (!isValid)
            {
                Debug.LogWarning("Position became invalid during placement animation!");
                animator.SetBool("isHolding", true);
                return;
            }

            bool placed = snappingSystem.TryPlace(heldBlock, heldBlock.transform.position);

            if (placed)
            {
                if (SoundController.Instance != null)
                    SoundController.Instance.PlayPlaceSound();

                heldBlock = null;
                canMoveBlock = false;
                GetComponent<RobotController>().isHoldingBlock = false;
                animator.SetBool("isHolding", false);

                if (previewInstance != null)
                    previewInstance.SetActive(false);
            }
            else
            {
                animator.SetBool("isHolding", true);
            }
        }
    }

    //public void AnimationEvent_PickupComplete()
    //{
    //    Debug.Log("Pickup animation complete");
    //}

}


