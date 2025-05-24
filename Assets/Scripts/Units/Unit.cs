using UnityEngine;

namespace TurnClash.Units
{
    /// <summary>
    /// Complete Unit component that combines all creature and unit functionality
    /// No longer inherits from Creature - all functionality is contained here
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [Header("Player Assignment")]
        public Player player;
        
        [Header("Combat Stats")]
        public int health;
        public int maxHealth = 100;
        public int attack = 15;
        public int defense = 5;
        
        [Header("Unit Properties")]
        [SerializeField] private string unitName = "Unit";
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Grid Movement")]
        private Vector2Int currentGridPosition;
        private IsometricGroundManager groundManager;
        private UnitSpawner unitSpawner;
        
        // Player enum moved here since it's no longer in Creature
        public enum Player { Player1, Player2 }
        
        // Public property to access unit name
        public string UnitName 
        { 
            get { return unitName; } 
            set { unitName = value; }
        }
        
        protected virtual void Start()
        {
            // Initialize health to max if not set
            if (health <= 0)
                health = maxHealth;
            
            // Find managers
            groundManager = FindObjectOfType<IsometricGroundManager>();
            unitSpawner = FindObjectOfType<UnitSpawner>();
            
            // Initialize grid position based on current world position
            if (groundManager != null)
            {
                currentGridPosition = groundManager.WorldToGridPosition(transform.position);
            }
            
            Debug.Log($"Unit {unitName} initialized with HP:{health}/{maxHealth}, ATK:{attack}, DEF:{defense}");
        }
        
        public void TakeDamage(int damage)
        {
            damage = Mathf.Max(0, damage - defense); // Defense reduces damage

            health -= damage;
            Debug.Log($"{unitName} took {damage} damage (after defense). Health: {health}/{maxHealth}");
            
            if (health <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            Debug.Log($"{unitName} has died!");
            
            // Clear our position from the spawner
            if (unitSpawner != null)
            {
                unitSpawner.UpdateUnitPosition(currentGridPosition, new Vector2Int(-1, -1));
            }
            
            Destroy(gameObject);
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
        
        // Utility methods for getting unit info
        public bool IsAlive() => health > 0;
        public float GetHealthPercentage() => maxHealth > 0 ? (float)health / maxHealth : 0f;
        public bool IsPlayerUnit(Player checkPlayer) => player == checkPlayer;
        
        // Method for healing
        public void Heal(int healAmount)
        {
            int oldHealth = health;
            health = Mathf.Min(health + healAmount, maxHealth);
            int actualHeal = health - oldHealth;
            
            if (actualHeal > 0)
            {
                Debug.Log($"{unitName} healed for {actualHeal} HP. Health: {health}/{maxHealth}");
            }
        }
        
        // Method for stat modifications
        public void ModifyStats(int healthMod, int attackMod, int defenseMod)
        {
            maxHealth = Mathf.Max(1, maxHealth + healthMod);
            attack = Mathf.Max(0, attack + attackMod);
            defense = Mathf.Max(0, defense + defenseMod);
            
            // Ensure current health doesn't exceed new max
            health = Mathf.Min(health, maxHealth);
            
            Debug.Log($"{unitName} stats modified. New stats - HP:{health}/{maxHealth}, ATK:{attack}, DEF:{defense}");
        }
    }
} 