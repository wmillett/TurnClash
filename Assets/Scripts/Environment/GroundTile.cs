using UnityEngine;

public class GroundTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    
    private Vector2Int gridPosition;
    private bool isWalkable = true;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(Vector2Int position, Sprite sprite, bool walkable = true)
    {
        gridPosition = position;
        isWalkable = walkable;
        
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
            
        if (boxCollider != null)
            boxCollider.enabled = !walkable;
    }

    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsWalkable() => isWalkable;
} 