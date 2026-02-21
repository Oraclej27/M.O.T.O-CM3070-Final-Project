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

        bool placed = snappingSystem.TryPlace(
            heldBlock,
            heldBlock.transform.position
        );

        if (placed)
        {
            heldBlock = null;
            GetComponent<RobotController>().isHoldingBlock = false;
        }
        else
        {
            // Optional: feedback that placement failed
            Debug.Log("Cannot place here!");
        }
    }
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
    void ToggleTargetBlock()
    {
        Block target = GetBlockInFront();

        if (target != null)
        {
            target.ToggleState();
            Debug.Log($"Toggled {target.name} to {target.currentState}");
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
}


