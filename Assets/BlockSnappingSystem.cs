using UnityEngine;

public class BlockSnappingSystem : MonoBehaviour
{
    [Header("Snap Settings")]
    public float snapThreshold = 0.3f;
    public LayerMask blockLayer;

    [Header("Visual Feedback")]
    public GameObject snapIndicatorPrefab;
    public Material validSnapMaterial;
    public Material invalidSnapMaterial;

    private GameObject currentIndicator;
    private Renderer indicatorRenderer;

    void Start()
    {
        CreateIndicator();
    }

    // Call this every frame when moving a block
    public bool GetSnapPosition(Vector3 blockPosition, out Vector3 snapPosition, out bool isValid)
    {
        snapPosition = blockPosition;
        isValid = false;

        // Find nearest grid position
        Vector3 gridPos = SnapToGrid(blockPosition);

        // Check if there's a block at this position (except the one being held)
        Collider[] colliders = Physics.OverlapBox(gridPos, Vector3.one * 0.45f);
        bool hasBlockHere = false;

        foreach (Collider col in colliders)
        {
            Block block = col.GetComponent<Block>();
            if (block != null && !block.isBeingHeld)
            {
                hasBlockHere = true;

                Vector3 snap = SnapToFace(block.transform.position, blockPosition);
                snapPosition = snap;
                isValid = CheckStablePosition(snap);
                ShowIndicator(snap, isValid);
                return true;
            }
        }

        // If no block here and close enough to grid, snap
        float distanceToGrid = Vector3.Distance(blockPosition, gridPos);
        if (!hasBlockHere && distanceToGrid < snapThreshold)
        {
            snapPosition = gridPos;
            isValid = CheckStablePosition(gridPos);
            ShowIndicator(gridPos, isValid);
            return true;
        }

        HideIndicator();
        return false;
    }

    Vector3 SnapToFace(Vector3 blockPos, Vector3 targetPos)
    {
        Vector3 dir = (targetPos - blockPos).normalized;

        Vector3 snapDir =
            Mathf.Abs(dir.x) > Mathf.Abs(dir.z)
            ? new Vector3(Mathf.Sign(dir.x), 0, 0)
            : new Vector3(0, 0, Mathf.Sign(dir.z));

        return blockPos + snapDir;
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float gridSize = 1f; // one unit grid

        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }


    bool CheckStablePosition(Vector3 position)
    {
        // Can't place above 3 blocks high
        if (position.y > 3f) return false;

        // If not on ground, must have support below
        if (position.y > 0.5f)
        {
            Vector3 checkBelow = position - Vector3.up;
            Collider[] below = Physics.OverlapBox(checkBelow, Vector3.one * 0.45f, Quaternion.identity, blockLayer);
            return below.Length > 0;
        }

        return true;
    }

    void CreateIndicator()
    {
        if (snapIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(snapIndicatorPrefab);
        }
        else
        {
            // Create default indicator
            currentIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentIndicator.transform.localScale = new Vector3(1.02f, 0.05f, 1.02f);
            Destroy(currentIndicator.GetComponent<Collider>());
        }

        indicatorRenderer = currentIndicator.GetComponent<Renderer>();
        currentIndicator.SetActive(false);
    }

    void ShowIndicator(Vector3 position, bool isValid)
    {
        if (currentIndicator == null) return;

        currentIndicator.transform.position = position;

        if (indicatorRenderer != null)
        {
            indicatorRenderer.material = isValid ?
                (validSnapMaterial != null ? validSnapMaterial : CreateDefaultMaterial(Color.green)) :
                (invalidSnapMaterial != null ? invalidSnapMaterial : CreateDefaultMaterial(Color.red));
        }

        currentIndicator.SetActive(true);
    }

    void HideIndicator()
    {
        if (currentIndicator != null)
            currentIndicator.SetActive(false);
    }

    Material CreateDefaultMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(color.r, color.g, color.b, 0.3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        return mat;
    }
}