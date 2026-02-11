//using UnityEngine;

//public class RobotPickupController : MonoBehaviour
//{
//    [Header("Pickup Settings")]
//    public Transform holdPoint;
//    public float pickupDistance = 3f;
//    public LayerMask blockLayer;

//    [Header("Snapping")]
//    public BlockSnappingSystem snappingSystem;

//    private Block heldBlock;

//    void Update()
//    {
//        HandleInput();

//        if (heldBlock != null)
//        {
//            MoveHeldBlock();
//        }
//    }

//    void HandleInput()
//    {
//        // F = Pick up / Drop
//        if (Input.GetKeyDown(KeyCode.F))
//        {
//            if (heldBlock == null)
//                TryPickup();
//            else
//                DropBlock();
//        }

//        // R = Toggle state while holding
//        if (Input.GetKeyDown(KeyCode.R) && heldBlock != null)
//        {
//            heldBlock.ToggleState();
//        }
//    }

//    void TryPickup()
//    {
//        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit, pickupDistance, blockLayer))
//        {
//            Block block = hit.collider.GetComponent<Block>();

//            if (block != null && block.currentState == Block.BlockState.Movable)
//            {
//                heldBlock = block;
//                heldBlock.OnPickup();
//            }
//        }
//    }

//    void MoveHeldBlock()
//    {
//        Vector3 targetPosition = holdPoint.position;

//        if (snappingSystem != null)
//        {
//            Vector3 snapPos;
//            bool isValid;

//            bool hasSnap = snappingSystem.GetSnapPosition(
//                holdPoint.position,
//                out snapPos,
//                out isValid
//            );

//            if (hasSnap && isValid)
//            {
//                targetPosition = snapPos;
//            }
//        }

//        heldBlock.transform.position = targetPosition;
//    }

//    void DropBlock()
//    {
//        if (heldBlock == null) return;

//        // Declare snapPos and isValid
//        Vector3 snapPos = Vector3.zero;
//        bool isValid = false;

//        // Get snap position and validity from the snapping system
//        bool hasSnap = snappingSystem != null &&
//                       snappingSystem.GetSnapPosition(
//                           heldBlock.transform.position,
//                           out snapPos,
//                           out isValid
//                       );

//        // If the block can snap to a valid position
//        if (hasSnap && isValid)
//        {
//            heldBlock.transform.position = snapPos;
//        }

//        // Release the block and reset
//        heldBlock.OnRelease();
//        heldBlock = null;
//    }
//}

using UnityEngine;

public class RobotPickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupDistance = 3f;
    public LayerMask blockLayer;

    [Header("Snapping")]
    public BlockSnappingSystem snappingSystem;

    private Block heldBlock;
    private bool canToggleState = true;  // To ensure state toggle works only when not holding a block
    private Quaternion heldRotationOffset;


    void Update()
    {
        HandleInput();

        if (heldBlock != null)
        {
            MoveHeldBlock();
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

    //void TryPickup()
    //{
    //    Block target = GetBlockInFront();

    //    if (target != null)
    //    {
    //        heldBlock = target;
    //        heldBlock.OnPickup();
    //        canToggleState = false;  // Prevent state toggle while holding
    //    }
    //}
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


    void DropBlock()
    {
        if (heldBlock == null) return;

        // Declare snapPos and isValid
        Vector3 snapPos = Vector3.zero;
        bool isValid = false;

        // Get snap position and validity from the snapping system
        bool hasSnap = snappingSystem != null &&
                       snappingSystem.GetSnapPosition(
                           heldBlock.transform.position,
                           out snapPos,
                           out isValid
                       );

        // If the block can snap to a valid position
        if (hasSnap && isValid)
        {
            heldBlock.transform.position = snapPos;
        }

        // Release the block and reset
        heldBlock.OnRelease();
        heldBlock = null;
        canToggleState = true;  // Allow state toggle after dropping
    }

    //void MoveHeldBlock()
    //{
    //    Vector3 targetPosition = holdPoint.position;

    //    if (snappingSystem != null)
    //    {
    //        Vector3 snapPos;
    //        bool isValid;

    //        bool hasSnap = snappingSystem.GetSnapPosition(
    //            holdPoint.position,
    //            out snapPos,
    //            out isValid
    //        );

    //        if (hasSnap && isValid)
    //            targetPosition = snapPos;
    //    }

    //    heldBlock.transform.position = targetPosition;
    //}
    void MoveHeldBlock()
    {
        Vector3 targetPosition = holdPoint.position;

        if (snappingSystem != null)
        {
            Vector3 snapPos;
            bool isValid;

            bool hasSnap = snappingSystem.GetSnapPosition(
                holdPoint.position,
                out snapPos,
                out isValid
            );

            if (hasSnap && isValid)
                targetPosition = snapPos;
        }

        heldBlock.transform.position = targetPosition;

        // Rotate with robot
        heldBlock.transform.rotation = transform.rotation * heldRotationOffset;
    }

    void ToggleTargetBlock()
    {
        Block target = GetBlockInFront();

        if (target != null && canToggleState)
        {
            target.ToggleState();
        }
    }

    //Block GetBlockInFront()
    //{
    //    Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
    //    RaycastHit[] hits = Physics.RaycastAll(ray, pickupDistance, blockLayer);

    //    Block highestBlock = null;
    //    float highestY = float.MinValue;

    //    foreach (RaycastHit hit in hits)
    //    {
    //        Block block = hit.collider.GetComponent<Block>();
    //        if (block != null)
    //        {
    //            if (block.transform.position.y > highestY)
    //            {
    //                highestY = block.transform.position.y;
    //                highestBlock = block;
    //            }
    //        }
    //    }

    //    return highestBlock;
    //}

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


