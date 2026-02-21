using System.Collections.Generic;
using UnityEngine;

public class GridSnappingSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1f; // size of one block unit
    public bool requireNearbyBlock = false; // only snap if near another block
    public float nearbyCheckRadius = 1f;

    // Keep track of occupied positions (optional, prevents overlap)
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    // ==================== PUBLIC METHODS ====================

    /// <summary>
    /// Snap a world position to the nearest grid position
    /// </summary>
    public Vector3 SnapPosition(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Place a block at a valid snapped position
    /// </summary>
    public void PlaceBlock(Block block)
    {
        if (block == null) return;

        Vector3 targetPos = block.transform.position;

        // Optionally require nearby block to snap
        if (requireNearbyBlock)
        {
            Collider[] nearby = Physics.OverlapSphere(targetPos, nearbyCheckRadius);
            bool foundBlock = false;

            foreach (Collider col in nearby)
            {
                Block otherBlock = col.GetComponent<Block>();
                if (otherBlock != null && otherBlock != block)
                {
                    foundBlock = true;
                    break;
                }
            }

            if (!foundBlock)
            {
                // Don't snap if no nearby block
                return;
            }
        }

        // Snap to grid
        Vector3 snappedPos = SnapPosition(targetPos);

        // Optional: prevent overlapping
        if (!occupiedPositions.Contains(snappedPos))
        {
            block.transform.position = snappedPos;
            block.OnRelease(); // make sure physics is enabled
            occupiedPositions.Add(snappedPos);
        }
        else
        {
            Debug.LogWarning("Position already occupied: " + snappedPos);
        }
    }

    /// <summary>
    /// Remove a block from the occupied positions (e.g., when picked up)
    /// </summary>
    public void RemoveBlock(Block block)
    {
        if (block == null) return;

        Vector3 pos = SnapPosition(block.transform.position);
        if (occupiedPositions.Contains(pos))
        {
            occupiedPositions.Remove(pos);
        }
    }
}
