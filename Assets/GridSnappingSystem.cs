//using UnityEngine;

//public class GridSnappingSystem : MonoBehaviour
//{
//    public float gridSize = 1f;
//    public LayerMask blockLayer;

//    public Vector3 GetSnappedPosition(Vector3 position)
//    {
//        float x = Mathf.Round(position.x / gridSize) * gridSize;
//        float y = Mathf.Round(position.y / gridSize) * gridSize;
//        float z = Mathf.Round(position.z / gridSize) * gridSize;

//        return new Vector3(x, y, z);
//    }

//    public bool CanPlaceAt(Vector3 position)
//    {
//        Vector3 halfExtents = Vector3.one * (gridSize * 0.45f);

//        return !Physics.CheckBox(position, halfExtents, Quaternion.identity, blockLayer);
//    }

//    public bool TryPlace(Block block, Vector3 worldPosition)
//    {
//        float snappedX = Mathf.Round(worldPosition.x / gridSize) * gridSize;
//        float snappedZ = Mathf.Round(worldPosition.z / gridSize) * gridSize;

//        Vector3 rayOrigin = new Vector3(snappedX, worldPosition.y + 10f, snappedZ);

//        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 20f);

//        RaycastHit? bestHit = null;
//        float highestY = float.MinValue;

//        foreach (RaycastHit hit in hits)
//        {
//            if (hit.collider.gameObject == block.gameObject)
//                continue;

//            float top = hit.collider.bounds.max.y;

//            if (top > highestY)
//            {
//                highestY = top;
//                bestHit = hit;
//            }
//        }

//        if (bestHit.HasValue)
//        {
//            float newY = highestY + (gridSize / 2f);

//            Vector3 finalPos = new Vector3(snappedX, newY, snappedZ);

//            block.transform.position = finalPos;
//            block.OnPlaced();
//            return true;
//        }

//        return false;
//    }
//}--------------------------------------------------------------------------------------------------------------------
using UnityEngine;

public class GridSnappingSystem : MonoBehaviour
{
    public float gridSize = 1f;
    public LayerMask blockLayer;

    public Vector3 GetSnappedPosition(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;

        return new Vector3(x, y, z);
    }

    public bool CanPlaceAt(Vector3 position, Block blockToIgnore = null)
    {
        Vector3 halfExtents = Vector3.one * (gridSize * 0.45f);

        Collider[] colliders = Physics.OverlapBox(position, halfExtents, Quaternion.identity, blockLayer);

        foreach (Collider col in colliders)
        {
            Block block = col.GetComponent<Block>();
            // If there's a block here AND it's not the one we're placing
            if (block != null && block != blockToIgnore)
            {
                return false; // Position occupied by another block
            }
        }

        return true; // Position is free
    }

    public bool TryPlace(Block block, Vector3 worldPosition)
    {
        // Snap to grid horizontally
        float snappedX = Mathf.Round(worldPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(worldPosition.z / gridSize) * gridSize;

        // Raycast from way above to find what's below
        Vector3 rayOrigin = new Vector3(snappedX, 100f, snappedZ);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 200f, blockLayer);

        RaycastHit? bestHit = null;
        float highestY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            // Skip the block we're trying to place
            if (hit.collider.gameObject == block.gameObject)
                continue;

            // Get the top of this block
            Collider col = hit.collider;
            float top = col.bounds.max.y;

            if (top > highestY)
            {
                highestY = top;
                bestHit = hit;
            }
        }

        // Determine final Y position
        float newY;

        if (bestHit.HasValue)
        {
            // Place on top of existing block
            newY = highestY + (gridSize / 2f);
            Debug.Log($"Placing on top of block at y={highestY}, new y={newY}");
        }
        else
        {
            // Place on ground (assuming ground at y=0)
            newY = gridSize / 2f;
            Debug.Log($"Placing on ground at y={newY}");
        }

        Vector3 finalPos = new Vector3(snappedX, newY, snappedZ);

        // IMPORTANT: When stacking, we ONLY check if the position is occupied
        // by a block that ISN'T the one we're placing on
        // We need to allow stacking, so we check if there's a block at the EXACT same spot

        // Check if position is empty (no block at this exact spot)
        Vector3 checkPos = finalPos;
        bool positionEmpty = true;

        Collider[] blocksAtPos = Physics.OverlapBox(checkPos, Vector3.one * (gridSize * 0.45f), Quaternion.identity, blockLayer);
        foreach (Collider col in blocksAtPos)
        {
            Block b = col.GetComponent<Block>();
            if (b != null && b != block)
            {
                positionEmpty = false;
                Debug.Log($"Position occupied by {b.name}");
                break;
            }
        }

        if (positionEmpty)
        {
            block.transform.position = finalPos;
            block.OnPlaced();
            Debug.Log($"Block placed at {finalPos}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot place block at {finalPos} - position occupied by another block");
            return false;
        }
    }
}
