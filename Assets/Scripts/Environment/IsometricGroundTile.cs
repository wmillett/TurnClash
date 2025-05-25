using UnityEngine;

public class IsometricGroundTile : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private BoxCollider boxCollider;
    
    private Vector2Int gridPosition;
    private bool isWalkable = true;
    private Material defaultMaterial;
    private Material highlightMaterial;

    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            // If no collider exists, create one for click detection
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

        // Store default material
        if (meshRenderer != null)
            defaultMaterial = meshRenderer.material;
    }

    public void Initialize(Vector2Int position, bool walkable = true)
    {
        gridPosition = position;
        isWalkable = walkable;
            
        // Keep colliders enabled for click detection
        // Colliders are needed for both walkable (for movement preview clicks) and non-walkable tiles
        if (boxCollider != null)
        {
            boxCollider.enabled = true; // Always enabled for click detection
            boxCollider.isTrigger = walkable; // Walkable tiles are triggers, non-walkable are solid
        }
    }

    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsWalkable() => isWalkable;

    public void SetHighlight(bool highlight)
    {
        if (meshRenderer != null)
        {
            if (highlight)
            {
                // Create a highlight material if it doesn't exist
                if (highlightMaterial == null)
                {
                    highlightMaterial = new Material(defaultMaterial);
                    highlightMaterial.color = new Color(1f, 1f, 0.5f, 1f); // Yellow tint
                }
                meshRenderer.material = highlightMaterial;
            }
            else
            {
                meshRenderer.material = defaultMaterial;
            }
        }
    }

    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        if (boxCollider != null)
        {
            boxCollider.enabled = true; // Always enabled for click detection
            boxCollider.isTrigger = walkable; // Walkable tiles are triggers, non-walkable are solid
        }
    }
} 