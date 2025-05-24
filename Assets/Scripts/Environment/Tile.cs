using UnityEngine;
using TurnClash.Environment;

public class Tile : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private bool isWalkable = true;
    [SerializeField] private float movementCost = 1f;
    
    // Reference to any unit currently occupying this tile
    private Unit occupiedUnit;
    
    // Properties
    public Vector2Int GridPosition => gridPosition;
    public bool IsWalkable => isWalkable;
    public float MovementCost => movementCost;
    public Unit OccupiedUnit => occupiedUnit;
    
    private void Awake()
    {
        // Initialize grid position based on world position
        Vector3 worldPos = transform.position;
        gridPosition = new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.z)
        );
    }
    
    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
        // Update world position to match grid position
        transform.position = new Vector3(position.x, 0, position.y);
    }
    
    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
    }
    
    public void SetMovementCost(float cost)
    {
        movementCost = cost;
    }
    
    public bool IsOccupied()
    {
        return occupiedUnit != null;
    }
    
    public void SetOccupiedUnit(Unit unit)
    {
        occupiedUnit = unit;
    }
    
    public void ClearOccupiedUnit()
    {
        occupiedUnit = null;
    }
    
    // Visual feedback methods
    public void Highlight(bool highlight)
    {
        // TODO: Implement visual highlighting
        // This could change the tile's material or color
    }
    
    public void ShowRange(bool show)
    {
        // TODO: Implement range visualization
        // This could show movement range or attack range
    }
} 