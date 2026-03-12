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
            if (block != null && block != blockToIgnore)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryPlace(Block block, Vector3 worldPosition)
    {
        float placeX = worldPosition.x;
        float placeZ = worldPosition.z;

        Vector3 rayOrigin = new Vector3(placeX, 100f, placeZ);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 200f, blockLayer);

        RaycastHit? bestHit = null;
        float highestY = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == block.gameObject)
                continue;

            Collider col = hit.collider;
            float top = col.bounds.max.y;

            if (top > highestY)
            {
                highestY = top;
                bestHit = hit;
            }
        }

        float newY;

        if (bestHit.HasValue)
        {
            newY = highestY + (gridSize / 2f);
        }
        else
        {
            newY = gridSize / 2f;
            Debug.Log($"Placing on ground at y={newY}");
        }

        Vector3 finalPos = new Vector3(placeX, newY, placeZ);
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
            return true;
        }
        else
        {
            Debug.LogWarning($"Cannot place block at {finalPos} - position occupied by another block");
            return false;
        }
    }

    public bool GetPlacementInfo(Vector3 worldPosition, out Vector3 previewPosition, out bool isValid)
    {
        float placeX = worldPosition.x;
        float placeZ = worldPosition.z;

        Vector3 rayOrigin = new Vector3(placeX, 100f, placeZ);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 200f, blockLayer);

        float highestY = float.MinValue;
        Collider bestHit = null;

        foreach (RaycastHit hit in hits)
        {
            Block hitBlock = hit.collider.GetComponent<Block>();

            if (hitBlock != null && hitBlock.isBeingHeld)
                continue;

            Collider col = hit.collider;
            float top = col.bounds.max.y;

            if (top > highestY)
            {
                highestY = top;
                bestHit = col;
            }
        }

        float newY;

        if (bestHit != null)
        {
            newY = highestY + (gridSize / 2f);
        }
        else
        {
            newY = gridSize / 2f;
        }

        previewPosition = new Vector3(placeX, newY, placeZ);

        Collider[] blocksAtPos = Physics.OverlapBox(previewPosition, Vector3.one * (gridSize * 0.45f), Quaternion.identity, blockLayer);

        isValid = true;
        foreach (Collider col in blocksAtPos)
        {
            Block b = col.GetComponent<Block>();
            if (b != null && !b.isBeingHeld)
            {
                isValid = false;

                break;
            }
        }

        return true;
    }
}
