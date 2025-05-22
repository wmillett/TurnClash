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
            boxCollider = GetComponent<BoxCollider>();

        // Store default material
        if (meshRenderer != null)
            defaultMaterial = meshRenderer.material;
    }

    public void Initialize(Vector2Int position, bool walkable = true)
    {
        gridPosition = position;
        isWalkable = walkable;
            
        if (boxCollider != null)
            boxCollider.enabled = !walkable;
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
            boxCollider.enabled = !walkable;
    }
} 