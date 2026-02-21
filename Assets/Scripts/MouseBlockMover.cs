//using UnityEngine;

//public class MouseBlockMover : MonoBehaviour
//{
//    [Header("Settings")]
//    public float pickupDistance = 10f;
//    public float moveSpeed = 15f;

//    [Header("References")]
//    public BlockSnappingSystem snappingSystem;

//    // Runtime
//    private Camera mainCamera;
//    private Block heldBlock;
//    private Vector3 holdOffset;
//    private Vector3 targetPosition;

//    void Start()
//    {
//        mainCamera = Camera.main;

//        if (snappingSystem == null)
//            snappingSystem = FindFirstObjectByType<BlockSnappingSystem>();
//    }

//    void Update()
//    {
//        // Pick up
//        if (Input.GetMouseButtonDown(0) && heldBlock == null)
//        {
//            TryPickup();
//        }

//        // Update held block
//        if (heldBlock != null)
//        {
//            UpdateHeldBlock();

//            // Release
//            if (Input.GetMouseButtonUp(0))
//            {
//                ReleaseBlock();
//            }

//            // Cancel
//            if (Input.GetMouseButtonDown(1))
//            {
//                CancelPickup();
//            }
//        }
//    }

//    void TryPickup()
//    {
//        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hit;

//        // IMPORTANT: Remove the layer mask or use "Everything"
//        if (Physics.Raycast(ray, out hit, pickupDistance))
//        {
//            Block block = hit.collider.GetComponent<Block>();
//            if (block != null && !block.isBeingHeld)
//            {
//                PickupBlock(block, hit.point);
//            }
//        }
//    }

//    void PickupBlock(Block block, Vector3 hitPoint)
//    {
//        heldBlock = block;
//        holdOffset = block.transform.position - hitPoint;
//        block.OnPickup();
//        Debug.Log($"Picked up: {block.name}");
//    }

//    void UpdateHeldBlock()
//    {
//        // Get mouse position on horizontal plane
//        Plane plane = new Plane(Vector3.up, heldBlock.transform.position);
//        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

//        if (plane.Raycast(ray, out float distance))
//        {
//            targetPosition = ray.GetPoint(distance) + holdOffset;

//            // Try to snap
//            if (snappingSystem != null)
//            {
//                if (snappingSystem.GetSnapPosition(targetPosition, out Vector3 snapPos, out bool isValid))
//                {
//                    targetPosition = snapPos;
//                }
//            }

//            // Move block
//            heldBlock.transform.position = Vector3.Lerp(
//                heldBlock.transform.position,
//                targetPosition,
//                Time.deltaTime * moveSpeed
//            );
//        }
//    }

//    void ReleaseBlock()
//    {
//        if (snappingSystem != null)
//        {
//            if (snappingSystem.GetSnapPosition(
//                heldBlock.transform.position,
//                out Vector3 finalPos,
//                out bool isValid) && isValid)
//            {
//                heldBlock.transform.position = finalPos;
//            }
//        }

//        heldBlock.OnRelease();
//        heldBlock = null;
//    }

//    void CancelPickup()
//    {
//        if (heldBlock == null) return;

//        heldBlock.OnRelease();
//        Debug.Log("Pickup cancelled");
//        heldBlock = null;
//    }

//    void OnGUI()
//    {
//        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
//        boxStyle.fontSize = 12;

//        if (heldBlock != null)
//        {
//            GUI.Box(new Rect(10, 10, 300, 80), "BLOCK CONTROLS", boxStyle);
//            GUI.Label(new Rect(20, 40, 280, 20), $"Holding: {heldBlock.name}");
//            GUI.Label(new Rect(20, 60, 280, 20), "Release LMB to place | RMB to cancel");
//        }
//        else
//        {
//            GUI.Box(new Rect(10, 10, 250, 40), "Click on a block to pick it up", boxStyle);
//        }
//    }
//}