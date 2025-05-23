using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TurnClash.Units;

public class UnitSpawner : MonoBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject unitPlaceholder1Prefab;
    [SerializeField] private GameObject unitPlaceholder2Prefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private int unitsPerType = 2;
    [SerializeField] private float unitHeightOffset = 0.5f; // Height above the tile
    
    private IsometricGroundManager groundManager;
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    
    private void Start()
    {
        // Find the ground manager
        groundManager = FindObjectOfType<IsometricGroundManager>();
        
        if (groundManager == null)
        {
            Debug.LogError("IsometricGroundManager not found! Cannot spawn units.");
            return;
        }
        
        // Wait a frame to ensure the grid is fully initialized
        StartCoroutine(SpawnUnitsAfterFrame());
    }
    
    private System.Collections.IEnumerator SpawnUnitsAfterFrame()
    {
        yield return null; // Wait one frame
        SpawnUnits();
    }
    
    private void SpawnUnits()
    {
        // Spawn units of type 1
        for (int i = 0; i < unitsPerType; i++)
        {
            SpawnUnit(unitPlaceholder1Prefab, Creature.Player.Player1);
        }
        
        // Spawn units of type 2
        for (int i = 0; i < unitsPerType; i++)
        {
            SpawnUnit(unitPlaceholder2Prefab, Creature.Player.Player2);
        }
    }
    
    private void SpawnUnit(GameObject unitPrefab, Creature.Player player)
    {
        if (unitPrefab == null)
        {
            Debug.LogError($"Unit prefab is null for {player}!");
            return;
        }
        
        Vector2Int? gridPosition = GetRandomAvailablePosition();
        
        if (!gridPosition.HasValue)
        {
            Debug.LogWarning("No available positions to spawn unit!");
            return;
        }
        
        // Get the tile to find its actual height
        IsometricGroundTile tile = groundManager.GetTileAtPosition(gridPosition.Value);
        if (tile == null)
        {
            Debug.LogError($"No tile found at position {gridPosition.Value}!");
            return;
        }
        
        // Calculate world position based on tile's actual position and bounds
        Vector3 worldPosition = tile.transform.position;
        
        // Get the tile's renderer to find its actual height
        MeshRenderer tileRenderer = tile.GetComponent<MeshRenderer>();
        if (tileRenderer != null)
        {
            // Position unit on top of the tile
            float tileTopY = tileRenderer.bounds.max.y;
            worldPosition.y = tileTopY + unitHeightOffset;
        }
        else
        {
            // Fallback calculation if no renderer
            worldPosition.y = tile.transform.position.y + (tile.transform.localScale.y * 0.5f) + unitHeightOffset;
        }
        
        // Instantiate the unit
        GameObject unitObj = Instantiate(unitPrefab, worldPosition, Quaternion.identity);
        unitObj.name = $"{unitPrefab.name}_{player}_{occupiedPositions.Count}";
        
        // Configure the Creature component
        Creature creature = unitObj.GetComponent<Creature>();
        if (creature != null)
        {
            creature.player = player;
        }
        else
        {
            Debug.LogWarning($"Spawned unit {unitObj.name} does not have a Creature component!");
        }
        
        // Add UnitSelectable component if it doesn't exist
        UnitSelectable selectable = unitObj.GetComponent<UnitSelectable>();
        if (selectable == null)
        {
            selectable = unitObj.AddComponent<UnitSelectable>();
        }
        
        // Ensure the unit has a collider for mouse interaction
        Collider unitCollider = unitObj.GetComponent<Collider>();
        if (unitCollider == null)
        {
            // Add a box collider if none exists
            BoxCollider boxCollider = unitObj.AddComponent<BoxCollider>();
            
            // Try to size the collider based on the renderer bounds
            MeshRenderer renderer = unitObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                boxCollider.size = renderer.bounds.size;
                boxCollider.center = Vector3.zero;
            }
            else
            {
                // Default collider size
                boxCollider.size = Vector3.one;
            }
            
            Debug.Log($"Added BoxCollider to {unitObj.name} for selection");
        }
        
        // Mark position as occupied
        occupiedPositions.Add(gridPosition.Value);
        
        Debug.Log($"Spawned {unitObj.name} at grid position {gridPosition.Value}, world position {worldPosition}");
    }
    
    private Vector2Int? GetRandomAvailablePosition()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        
        // Check all grid positions
        for (int x = 0; x < groundManager.GridWidth; x++)
        {
            for (int y = 0; y < groundManager.GridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                // Check if position is not occupied
                if (!occupiedPositions.Contains(pos))
                {
                    // Check if tile exists and is walkable
                    IsometricGroundTile tile = groundManager.GetTileAtPosition(pos);
                    if (tile != null && tile.IsWalkable())
                    {
                        availablePositions.Add(pos);
                    }
                }
            }
        }
        
        // Return random position from available ones
        if (availablePositions.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            return availablePositions[randomIndex];
        }
        
        return null;
    }
    
    // Public method to check if a position is occupied by a unit
    public bool IsPositionOccupied(Vector2Int gridPosition)
    {
        return occupiedPositions.Contains(gridPosition);
    }
    
    // Public method to update unit position (for movement)
    public void UpdateUnitPosition(Vector2Int oldPosition, Vector2Int newPosition)
    {
        if (occupiedPositions.Contains(oldPosition))
        {
            occupiedPositions.Remove(oldPosition);
            occupiedPositions.Add(newPosition);
        }
    }
} 