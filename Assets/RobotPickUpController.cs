using UnityEngine;

public class RobotPickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupDistance = 3f;
    public LayerMask blockLayer;

    [Header("Snapping")]
    public GridSnappingSystem snappingSystem;

    private Block heldBlock;
    //private bool canToggleState = true;  // To ensure state toggle works only when not holding a block
    private Quaternion heldRotationOffset;

    void Update()
    {
        HandleInput();

        if (heldBlock != null)
            MoveHeldBlock();
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
            heldBlock.OnPickup();

            // Store relative rotation
            heldRotationOffset = Quaternion.Inverse(transform.rotation) * heldBlock.transform.rotation;

            GetComponent<RobotController>().isHoldingBlock = true;
        }
    }

    //void DropBlock()
    //{
    //    if (heldBlock == null) return;

    //    Vector3 snapPos = Vector3.zero;
    //    bool isValid = false;

    //    bool hasSnap = snappingSystem != null &&
    //                   snappingSystem.GetSnapPosition(
    //                       heldBlock.transform.position,
    //                       out snapPos,
    //                       out isValid
    //                   );

    //    if (hasSnap && isValid)
    //    {
    //        heldBlock.GetComponent<Rigidbody>().MovePosition(snapPos);
    //    }

    //    heldBlock.OnRelease();
    //    heldBlock = null;

    //    GetComponent<RobotController>().isHoldingBlock = false; // ADD THIS
    //}
    void DropBlock()
    {
        if (heldBlock == null) return;

        // Remove from previous occupied positions
        snappingSystem.RemoveBlock(heldBlock);

        // Snap & place
        snappingSystem.PlaceBlock(heldBlock);

        heldBlock = null;
        GetComponent<RobotController>().isHoldingBlock = false;
    }


    void MoveHeldBlock()
    {
        Rigidbody rb = heldBlock.GetComponent<Rigidbody>();

        Vector3 targetPosition = holdPoint.position;
        Vector3 direction = targetPosition - rb.position;

        float followSpeed = 20f; // tune this
        rb.linearVelocity = direction * followSpeed;

        // Rotation
        Quaternion targetRot = transform.rotation * heldRotationOffset;
        rb.MoveRotation(targetRot);
    }


    void ToggleTargetBlock()
    {
        Block target = GetBlockInFront();

        if (target != null)
        {
            target.ToggleState();
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
            if (block != null)
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

}


