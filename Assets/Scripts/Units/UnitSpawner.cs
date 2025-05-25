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
    
    [Header("Player Colors")]
    [SerializeField] private Color player1Color = Color.blue;
    [SerializeField] private Color player2Color = Color.red;
    [SerializeField] private Color player3Color = Color.green;
    [SerializeField] private Color player4Color = Color.yellow;
    [SerializeField] private float colorIntensity = 0.8f; // How strong the color tint is
    
    [Header("Unit Templates")]
    [SerializeField] private UnitTemplate chillCubeTemplate;
    [SerializeField] private UnitTemplate meanBallTemplate;
    
    [System.Serializable]
    public class UnitTemplate
    {
        public string unitName;
        public int maxHealth = 100;
        public int attack = 15;
        public int defence = 5;
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
                defence = 8
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
                defence = 3
            };
            Debug.Log("UnitSpawner: Created default Mean Ball template");
        }
        
        // Validate templates have different stats
        Debug.Log($"UnitSpawner: Chill Cube template - Name:'{chillCubeTemplate.unitName}', HP:{chillCubeTemplate.maxHealth}, ATK:{chillCubeTemplate.attack}, DEF:{chillCubeTemplate.defence}");
        Debug.Log($"UnitSpawner: Mean Ball template - Name:'{meanBallTemplate.unitName}', HP:{meanBallTemplate.maxHealth}, ATK:{meanBallTemplate.attack}, DEF:{meanBallTemplate.defence}");
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
        Debug.Log("UnitSpawner: Starting balanced unit spawning - each player gets one of each type");
        
        // Spawn units for Player 1 (one of each type)
        SpawnUnit(unitPlaceholder1Prefab, Unit.Player.Player1, chillCubeTemplate, player1Color, "Chill Cube");
        SpawnUnit(unitPlaceholder2Prefab, Unit.Player.Player1, meanBallTemplate, player1Color, "Mean Ball");
        
        // Spawn units for Player 2 (one of each type)  
        SpawnUnit(unitPlaceholder1Prefab, Unit.Player.Player2, chillCubeTemplate, player2Color, "Chill Cube");
        SpawnUnit(unitPlaceholder2Prefab, Unit.Player.Player2, meanBallTemplate, player2Color, "Mean Ball");
        
        Debug.Log($"UnitSpawner: Spawned 4 units total - 2 per player, each player has both unit types");
    }
    
    private void SpawnUnit(GameObject unitPrefab, Unit.Player player, UnitTemplate template, Color playerColor, string unitTypeName)
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
        
        // Instantiate the unit with a descriptive name
        GameObject unitObj = Instantiate(unitPrefab, worldPosition, Quaternion.identity);
        unitObj.name = $"{unitTypeName}_{player}_{occupiedPositions.Count}";
        
        // Configure the Unit component with template stats
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
            unit.defence = template.defence;
            
            // Set the unit name using the unitTypeName parameter
            unit.UnitName = unitTypeName;
            
            Debug.Log($"✅ Stats set on Unit component for {unitObj.name}: HP={unit.health}/{unit.maxHealth}, ATK={unit.attack}, DEF={unit.defence}");
            Debug.Log($"✅ Unit name set to: '{unit.UnitName}'");
        }
        else
        {
            Debug.LogError($"❌ Failed to create Unit component on {unitObj.name}!");
        }
        
        // Apply player color to the unit
        ApplyPlayerColor(unitObj, playerColor, player, unitTypeName);
        
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
        
        Debug.Log($"Spawned {unitTypeName} ({unitObj.name}) for {player} at grid position {gridPosition.Value} with stats: HP={template.maxHealth}, ATK={template.attack}, DEF={template.defence}");
    }
    
    private void ApplyPlayerColor(GameObject unitObj, Color playerColor, Unit.Player player, string unitTypeName)
    {
        // Get the unit's MeshRenderer
        MeshRenderer renderer = unitObj.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"No MeshRenderer found on {unitObj.name}, cannot apply player color");
            return;
        }
        
        // Create a new material instance to avoid affecting the original prefab
        Material originalMaterial = renderer.material;
        Material playerMaterial = new Material(originalMaterial);
        
        // Apply player color as a tint
        // Multiply the original color with the player color for a tinted effect
        Color originalColor = originalMaterial.color;
        Color tintedColor = originalColor * playerColor * colorIntensity + originalColor * (1f - colorIntensity);
        tintedColor.a = originalColor.a; // Preserve original alpha
        
        playerMaterial.color = tintedColor;
        
        // Apply the new material
        renderer.material = playerMaterial;
        
        string colorName = player == Unit.Player.Player1 ? "Blue" : "Red";
        Debug.Log($"Applied {colorName} color to {unitTypeName} for {player}");
    }
    
    // Public helper method to get player colors for other systems
    public Color GetPlayerColor(Unit.Player player)
    {
        switch (player)
        {
            case Unit.Player.Player1:
                return player1Color;
            case Unit.Player.Player2:
                return player2Color;
            // Future-ready for when Player enum is expanded:
            // case Unit.Player.Player3:
            //     return player3Color;
            // case Unit.Player.Player4:
            //     return player4Color;
            default:
                return Color.white; // Default fallback color
        }
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