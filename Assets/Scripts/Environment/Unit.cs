using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private int movementRange = 3;
    [SerializeField] private int team = 0; // 0 for player, 1 for enemy
    
    private Tile currentTile;
    private bool hasMoved = false;
    
    // Properties
    public int MovementRange => movementRange;
    public int Team => team;
    public bool HasMoved => hasMoved;
    public Tile CurrentTile => currentTile;
    
    private void Start()
    {
        // Find the tile we're currently on
        UpdateCurrentTile();
    }
    
    public void UpdateCurrentTile()
    {
        // Cast a ray downward to find the tile we're standing on
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                SetCurrentTile(tile);
            }
        }
    }
    
    public void SetCurrentTile(Tile tile)
    {
        // Clear the old tile's occupation
        if (currentTile != null)
        {
            currentTile.ClearOccupiedUnit();
        }
        
        // Set the new tile
        currentTile = tile;
        if (currentTile != null)
        {
            currentTile.SetOccupiedUnit(this);
            // Update position to be on the tile
            transform.position = new Vector3(
                currentTile.GridPosition.x,
                transform.position.y,
                currentTile.GridPosition.y
            );
        }
    }
    
    public void MoveToTile(Tile targetTile)
    {
        if (targetTile != null && targetTile.IsWalkable && !targetTile.IsOccupied())
        {
            SetCurrentTile(targetTile);
            hasMoved = true;
        }
    }
    
    public void ResetTurn()
    {
        hasMoved = false;
    }
    
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
    
    public void SetMovementRange(int range)
    {
        movementRange = range;
    }
} 