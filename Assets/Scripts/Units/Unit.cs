using UnityEngine;

namespace TurnClash.Units
{
    public class Unit : Creature
    {
        [Header("Unit Properties")]
        [SerializeField] private string unitName = "Unit";
        [SerializeField] private float moveSpeed = 5f;
        
        private Vector2Int currentGridPosition;
        private IsometricGroundManager groundManager;
        private UnitSpawner unitSpawner;
        
        // Public property to access unit name
        public string UnitName 
        { 
            get { return unitName; } 
            set { unitName = value; }
        }
        
        protected override void Start()
        {
            base.Start();
            
            // Find managers
            groundManager = FindObjectOfType<IsometricGroundManager>();
            unitSpawner = FindObjectOfType<UnitSpawner>();
            
            // Initialize grid position based on current world position
            if (groundManager != null)
            {
                currentGridPosition = groundManager.WorldToGridPosition(transform.position);
            }
        }
        
        public Vector2Int GetGridPosition()
        {
            return currentGridPosition;
        }
        
        public void SetGridPosition(Vector2Int newPosition)
        {
            if (unitSpawner != null && groundManager != null)
            {
                // Update the spawner's tracking
                unitSpawner.UpdateUnitPosition(currentGridPosition, newPosition);
                
                // Update our position
                currentGridPosition = newPosition;
                
                // Get the tile to find its actual height
                IsometricGroundTile tile = groundManager.GetTileAtPosition(newPosition);
                if (tile != null)
                {
                    Vector3 worldPos = tile.transform.position;
                    
                    // Get the tile's renderer to find its actual height
                    MeshRenderer tileRenderer = tile.GetComponent<MeshRenderer>();
                    if (tileRenderer != null)
                    {
                        // Position unit on top of the tile
                        float tileTopY = tileRenderer.bounds.max.y;
                        worldPos.y = tileTopY + 0.5f; // Same offset as spawner
                    }
                    else
                    {
                        // Fallback calculation if no renderer
                        worldPos.y = tile.transform.position.y + (tile.transform.localScale.y * 0.5f) + 0.5f;
                    }
                    
                    transform.position = worldPos;
                }
            }
        }
        
        public bool CanMoveTo(Vector2Int targetPosition)
        {
            if (groundManager == null || unitSpawner == null)
                return false;
                
            // Check if position is within grid bounds
            if (targetPosition.x < 0 || targetPosition.x >= groundManager.GridWidth ||
                targetPosition.y < 0 || targetPosition.y >= groundManager.GridHeight)
                return false;
                
            // Check if tile is walkable
            IsometricGroundTile tile = groundManager.GetTileAtPosition(targetPosition);
            if (tile == null || !tile.IsWalkable())
                return false;
                
            // Check if position is occupied by another unit
            if (unitSpawner.IsPositionOccupied(targetPosition))
                return false;
                
            return true;
        }
        
        public override void Die()
        {
            // Clear our position from the spawner
            if (unitSpawner != null)
            {
                unitSpawner.UpdateUnitPosition(currentGridPosition, new Vector2Int(-1, -1));
            }
            
            base.Die();
        }
    }
} 