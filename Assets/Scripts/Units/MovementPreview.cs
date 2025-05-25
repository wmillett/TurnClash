using UnityEngine;
using System.Collections.Generic;
using TurnClash.Units;

namespace TurnClash.Units
{
    /// <summary>
    /// Shows movement preview tiles around selected units and handles tile click movement
    /// Only shows for friendly units that can move
    /// </summary>
    public class MovementPreview : MonoBehaviour
    {
        [Header("Movement Preview Settings")]
        [SerializeField] private bool enableMovementPreview = true;
        [SerializeField] private Color movementTileColor = new Color(0.2f, 1f, 0.2f, 0.7f); // Green glow
        [SerializeField] private Color attackTileColor = new Color(1f, 0.2f, 0.2f, 0.7f); // Red glow for attack
        [SerializeField] private int movementRange = 1; // How many tiles away can the unit move
        [SerializeField] private bool debugPreview = false;
        
        // Singleton instance
        private static MovementPreview instance;
        private static bool isApplicationQuitting = false;
        
        public static MovementPreview Instance
        {
            get
            {
                if (isApplicationQuitting)
                    return null;
                    
                if (instance == null)
                {
                    instance = FindObjectOfType<MovementPreview>();
                    if (instance == null && !isApplicationQuitting)
                    {
                        GameObject go = new GameObject("MovementPreview");
                        instance = go.AddComponent<MovementPreview>();
                    }
                }
                return instance;
            }
        }
        
        // Components
        private IsometricGroundManager groundManager;
        private TurnManager turnManager;
        
        // Preview state
        private Unit currentPreviewUnit;
        private List<IsometricGroundTile> highlightedTiles = new List<IsometricGroundTile>();
        private Dictionary<IsometricGroundTile, Material> originalMaterials = new Dictionary<IsometricGroundTile, Material>();
        
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.Log("MovementPreview: Destroying duplicate instance");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            isApplicationQuitting = false;
            Debug.Log("MovementPreview: Instance created and initialized");
        }
        
        private void Start()
        {
            // Find managers
            groundManager = FindObjectOfType<IsometricGroundManager>();
            turnManager = TurnManager.Instance;
            
            // Subscribe to selection events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected += OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
                Debug.Log("MovementPreview: Subscribed to selection events");
            }
            
            isApplicationQuitting = false;
            Debug.Log("MovementPreview: Start() called, ready for operations");
        }
        
        private void Update()
        {
            if (isApplicationQuitting) return;
            
            // Handle tile clicks
            if (Input.GetMouseButtonDown(0) && currentPreviewUnit != null)
            {
                HandleTileClick();
            }
        }
        
        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            if (debugPreview)
                Debug.Log("MovementPreview: Application quitting, preventing new instance creation");
        }
        
        private void OnUnitSelected(UnitSelectable selectedUnit)
        {
            if (!enableMovementPreview || selectedUnit == null) return;
            
            Unit unit = selectedUnit.GetUnit();
            if (unit == null) return;
            
            // Only show preview for friendly units that can move
            if (CanShowPreviewForUnit(unit))
            {
                ShowMovementPreview(unit);
            }
            else
            {
                HideMovementPreview();
            }
        }
        
        private void OnSelectionCleared()
        {
            HideMovementPreview();
        }
        
        private bool CanShowPreviewForUnit(Unit unit)
        {
            if (unit == null) return false;
            
            // Check if it's the current player's turn
            if (turnManager != null && turnManager.CurrentPlayer != unit.player)
            {
                if (debugPreview)
                    Debug.Log($"MovementPreview: Not showing preview - wrong player turn ({unit.player} vs {turnManager.CurrentPlayer})");
                return false;
            }
            
            // Check if unit can move (has moves remaining)
            if (turnManager != null && !turnManager.CanUnitMove(unit))
            {
                if (debugPreview)
                    Debug.Log($"MovementPreview: Not showing preview - unit {unit.UnitName} cannot move (no moves remaining)");
                return false;
            }
            
            // Check if game is over
            if (IsGameOver())
            {
                if (debugPreview)
                    Debug.Log("MovementPreview: Not showing preview - game is over");
                return false;
            }
            
            return true;
        }
        
        private void ShowMovementPreview(Unit unit)
        {
            if (groundManager == null || unit == null) return;
            
            // Clear any existing preview
            HideMovementPreview();
            
            currentPreviewUnit = unit;
            Vector2Int unitPosition = unit.GetGridPosition();
            
            if (debugPreview)
                Debug.Log($"MovementPreview: Showing preview for {unit.UnitName} at {unitPosition}");
            
            // Get adjacent tiles within movement range
            for (int x = -movementRange; x <= movementRange; x++)
            {
                for (int y = -movementRange; y <= movementRange; y++)
                {
                    // Skip the unit's current position
                    if (x == 0 && y == 0) continue;
                    
                    Vector2Int targetPos = unitPosition + new Vector2Int(x, y);
                    
                    // Check if this position is valid for movement or attack
                    if (IsValidMovementPosition(unit, targetPos))
                    {
                        HighlightTile(targetPos, unit);
                    }
                }
            }
            
            if (debugPreview)
                Debug.Log($"MovementPreview: Highlighted {highlightedTiles.Count} tiles");
        }
        
        private bool IsValidMovementPosition(Unit unit, Vector2Int position)
        {
            if (groundManager == null) return false;
            
            // Check bounds
            if (position.x < 0 || position.x >= groundManager.GridWidth ||
                position.y < 0 || position.y >= groundManager.GridHeight)
                return false;
            
            // Check if tile exists and is walkable
            IsometricGroundTile tile = groundManager.GetTileAtPosition(position);
            if (tile == null || !tile.IsWalkable())
                return false;
            
            // Check if we can move there (includes attack positions)
            return unit.CanMoveTo(position);
        }
        
        private void HighlightTile(Vector2Int position, Unit unit)
        {
            IsometricGroundTile tile = groundManager.GetTileAtPosition(position);
            if (tile == null) return;
            
            // Store original material
            MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
            if (renderer != null && !originalMaterials.ContainsKey(tile))
            {
                originalMaterials[tile] = renderer.material;
            }
            
            // Determine highlight color based on whether there's an enemy
            Color highlightColor = movementTileColor;
            Unit enemyAtPosition = unit.GetEnemyAtPosition(position);
            if (enemyAtPosition != null)
            {
                highlightColor = attackTileColor; // Red for attack tiles
            }
            
            // Create highlight material
            Material highlightMaterial = new Material(originalMaterials[tile]);
            highlightMaterial.color = highlightColor;
            highlightMaterial.EnableKeyword("_EMISSION");
            highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.5f);
            
            // Apply highlight
            renderer.material = highlightMaterial;
            highlightedTiles.Add(tile);
            
            // Add click detection component if it doesn't exist
            MovementTileClick clickComponent = tile.GetComponent<MovementTileClick>();
            if (clickComponent == null)
            {
                clickComponent = tile.gameObject.AddComponent<MovementTileClick>();
            }
            clickComponent.Initialize(position, this);
        }
        
        private void HideMovementPreview()
        {
            if (debugPreview && highlightedTiles.Count > 0)
                Debug.Log($"MovementPreview: Hiding preview ({highlightedTiles.Count} tiles)");
            
            // Restore original materials
            foreach (var tile in highlightedTiles)
            {
                if (tile != null && originalMaterials.ContainsKey(tile))
                {
                    MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = originalMaterials[tile];
                    }
                }
                
                // Remove click detection component
                MovementTileClick clickComponent = tile?.GetComponent<MovementTileClick>();
                if (clickComponent != null)
                {
                    Destroy(clickComponent);
                }
            }
            
            highlightedTiles.Clear();
            originalMaterials.Clear();
            currentPreviewUnit = null;
        }
        
        private void HandleTileClick()
        {
            // This method is called when a tile is clicked
            // The actual click handling is done by MovementTileClick components
        }
        
        /// <summary>
        /// Called by MovementTileClick when a highlighted tile is clicked
        /// </summary>
        public void OnTileClicked(Vector2Int tilePosition)
        {
            if (currentPreviewUnit == null)
            {
                if (debugPreview)
                    Debug.Log("MovementPreview: Tile clicked but no unit selected");
                return;
            }
            
            // Check if game is over
            if (IsGameOver())
            {
                if (debugPreview)
                    Debug.Log("MovementPreview: Tile click ignored - game is over");
                return;
            }
            
            // Check if unit can still move
            if (!CanShowPreviewForUnit(currentPreviewUnit))
            {
                if (debugPreview)
                    Debug.Log("MovementPreview: Tile click ignored - unit can no longer move");
                HideMovementPreview();
                return;
            }
            
            if (debugPreview)
                Debug.Log($"MovementPreview: Tile clicked at {tilePosition}, moving {currentPreviewUnit.UnitName}");
            
            // Attempt to move the unit
            bool moveSuccessful = currentPreviewUnit.TryMoveToPosition(tilePosition);
            
            if (moveSuccessful)
            {
                // Consume a move in the turn system
                if (turnManager != null)
                {
                    turnManager.UseMove(currentPreviewUnit.player);
                }
                
                // Hide preview after successful move
                HideMovementPreview();
                
                if (debugPreview)
                    Debug.Log($"MovementPreview: Successfully moved {currentPreviewUnit.UnitName} to {tilePosition}");
            }
            else
            {
                if (debugPreview)
                    Debug.Log($"MovementPreview: Failed to move {currentPreviewUnit.UnitName} to {tilePosition}");
            }
        }
        
        /// <summary>
        /// Check if the game is over
        /// </summary>
        private bool IsGameOver()
        {
            var combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
            {
                return combatManager.IsPlayerEliminated(Unit.Player.Player1) || 
                       combatManager.IsPlayerEliminated(Unit.Player.Player2);
            }
            return false;
        }
        
        /// <summary>
        /// Enable or disable movement preview
        /// </summary>
        public void SetMovementPreviewEnabled(bool enabled)
        {
            enableMovementPreview = enabled;
            if (!enabled)
            {
                HideMovementPreview();
            }
        }
        
        /// <summary>
        /// Set movement range
        /// </summary>
        public void SetMovementRange(int range)
        {
            movementRange = Mathf.Max(1, range);
        }
        
        private void OnDestroy()
        {
            if (debugPreview)
                Debug.Log("MovementPreview: OnDestroy called");
            
            // Clean up preview
            HideMovementPreview();
            
            // Unsubscribe from events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected -= OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared -= OnSelectionCleared;
            }
            
            // Clear singleton reference
            if (instance == this)
            {
                instance = null;
            }
            
            if (debugPreview)
                Debug.Log("MovementPreview: Cleanup complete, instance cleared");
        }
    }
}

/// <summary>
/// Component added to tiles to handle click detection
/// </summary>
public class MovementTileClick : MonoBehaviour
{
    private Vector2Int tilePosition;
    private MovementPreview movementPreview;
    
    public void Initialize(Vector2Int position, MovementPreview preview)
    {
        tilePosition = position;
        movementPreview = preview;
        
        // Ensure there's a collider for click detection
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
    }
    
    private void OnMouseDown()
    {
        if (movementPreview != null)
        {
            movementPreview.OnTileClicked(tilePosition);
        }
    }
} 