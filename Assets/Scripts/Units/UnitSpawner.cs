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
    
    [Header("Unit Templates")]
    [SerializeField] private UnitTemplate chillCubeTemplate;
    [SerializeField] private UnitTemplate meanBallTemplate;
    
    [System.Serializable]
    public class UnitTemplate
    {
        public string unitName;
        public int maxHealth = 100;
        public int attack = 15;
        public int defense = 5;
    }
    
    private IsometricGroundManager groundManager;
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    
    private void Awake()
    {
        // Initialize default templates if not set OR if they have empty names
        if (chillCubeTemplate == null || string.IsNullOrEmpty(chillCubeTemplate.unitName))
        {
            chillCubeTemplate = new UnitTemplate
            {
                unitName = "Chill Cube",
                maxHealth = 80,
                attack = 12,
                defense = 8
            };
            Debug.Log("UnitSpawner: Created default Chill Cube template");
        }
        
        if (meanBallTemplate == null || string.IsNullOrEmpty(meanBallTemplate.unitName))
        {
            meanBallTemplate = new UnitTemplate
            {
                unitName = "Mean Ball",
                maxHealth = 120,
                attack = 18,
                defense = 3
            };
            Debug.Log("UnitSpawner: Created default Mean Ball template");
        }
        
        // Validate templates have different stats
        Debug.Log($"UnitSpawner: Chill Cube template - Name:'{chillCubeTemplate.unitName}', HP:{chillCubeTemplate.maxHealth}, ATK:{chillCubeTemplate.attack}, DEF:{chillCubeTemplate.defense}");
        Debug.Log($"UnitSpawner: Mean Ball template - Name:'{meanBallTemplate.unitName}', HP:{meanBallTemplate.maxHealth}, ATK:{meanBallTemplate.attack}, DEF:{meanBallTemplate.defense}");
    }
    
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
        // Spawn units of type 1 (Chill Cube)
        for (int i = 0; i < unitsPerType; i++)
        {
            SpawnUnit(unitPlaceholder1Prefab, Unit.Player.Player1, chillCubeTemplate);
        }
        
        // Spawn units of type 2 (Mean Ball)
        for (int i = 0; i < unitsPerType; i++)
        {
            SpawnUnit(unitPlaceholder2Prefab, Unit.Player.Player2, meanBallTemplate);
        }
    }
    
    private void SpawnUnit(GameObject unitPrefab, Unit.Player player, UnitTemplate template)
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
        unitObj.name = $"{template.unitName}_{player}_{occupiedPositions.Count}";
        
        // Configure the Unit component with template stats (Unit extends Creature)
        Unit unit = unitObj.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogWarning($"No Unit component found on {unitObj.name}, adding one...");
            unit = unitObj.AddComponent<Unit>();
        }
        
        if (unit != null)
        {
            unit.player = player;
            unit.maxHealth = template.maxHealth;
            unit.health = template.maxHealth; // Start with full health
            unit.attack = template.attack;
            unit.defense = template.defense;
            
            // Set the unit name using the public property - with validation
            string unitNameToSet = !string.IsNullOrEmpty(template.unitName) ? template.unitName : $"Unit_{player}";
            unit.UnitName = unitNameToSet;
            
            Debug.Log($"✅ Stats set on Unit component for {unitObj.name}: HP={unit.health}/{unit.maxHealth}, ATK={unit.attack}, DEF={unit.defense}");
            Debug.Log($"✅ Unit name set to: '{unit.UnitName}' (from template: '{template.unitName}')");
        }
        else
        {
            Debug.LogError($"❌ Failed to create Unit component on {unitObj.name}!");
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
        
        Debug.Log($"Spawned {template.unitName} ({unitObj.name}) at grid position {gridPosition.Value} with stats: HP={template.maxHealth}, ATK={template.attack}, DEF={template.defense}");
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