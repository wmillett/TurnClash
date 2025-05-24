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
        
        [Header("Combat Settings")]
        [SerializeField] private bool debugCombat = true;
        
        [Header("Grid Movement")]
        private Vector2Int currentGridPosition;
        private IsometricGroundManager groundManager;
        private UnitSpawner unitSpawner;
        
        // Player enum moved here since it's no longer in Creature
        public enum Player { Player1, Player2 }
        
        // Events for combat system
        public System.Action<Unit, Unit, int> OnAttackPerformed; // attacker, defender, damage
        public System.Action<Unit> OnUnitDestroyed;
        
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
        
        /// <summary>
        /// Attack another unit with damage calculation
        /// </summary>
        public void AttackUnit(Unit target)
        {
            if (target == null || !target.IsAlive())
            {
                if (debugCombat)
                    Debug.LogWarning($"{unitName} tried to attack null or dead target");
                return;
            }
            
            if (target.player == this.player)
            {
                if (debugCombat)
                    Debug.LogWarning($"{unitName} tried to attack friendly unit {target.unitName}");
                return;
            }
            
            // Calculate damage: Attacker's attack - Defender's defense
            int baseDamage = this.attack;
            int finalDamage = Mathf.Max(1, baseDamage - target.defense); // Minimum 1 damage
            
            if (debugCombat)
            {
                Debug.Log($"COMBAT: {unitName} attacks {target.unitName}!");
                Debug.Log($"Damage calculation: {baseDamage} (ATK) - {target.defense} (DEF) = {finalDamage} damage");
            }
            
            // Apply damage to target
            target.TakeDamage(baseDamage); // TakeDamage already handles defense calculation
            
            // Fire combat event
            OnAttackPerformed?.Invoke(this, target, finalDamage);
        }

        public virtual void Die()
        {
            Debug.Log($"{unitName} has died!");
            
            // Fire destruction event
            OnUnitDestroyed?.Invoke(this);
            
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
        
        /// <summary>
        /// Get the unit at a specific grid position
        /// </summary>
        public Unit GetUnitAtPosition(Vector2Int position)
        {
            if (unitSpawner == null)
                return null;
                
            // Find all units and check their positions
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (Unit unit in allUnits)
            {
                if (unit.GetGridPosition() == position)
                {
                    return unit;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if there's an enemy unit at the target position
        /// </summary>
        public Unit GetEnemyAtPosition(Vector2Int targetPosition)
        {
            Unit unitAtPosition = GetUnitAtPosition(targetPosition);
            
            if (unitAtPosition != null && unitAtPosition.player != this.player)
            {
                return unitAtPosition;
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if movement to target position would result in combat
        /// </summary>
        public bool WouldAttackEnemy(Vector2Int targetPosition)
        {
            return GetEnemyAtPosition(targetPosition) != null;
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
        
        /// <summary>
        /// Enhanced movement method that handles combat
        /// </summary>
        public bool TryMoveToPosition(Vector2Int targetPosition)
        {
            // Check if there's an enemy at the target position
            Unit enemyAtTarget = GetEnemyAtPosition(targetPosition);
            
            if (enemyAtTarget != null)
            {
                // Attack the enemy instead of moving
                AttackUnit(enemyAtTarget);
                return true; // Combat occurred, so the action was successful
            }
            
            // No enemy, check if we can move normally
            if (CanMoveTo(targetPosition))
            {
                SetGridPosition(targetPosition);
                return true;
            }
            
            return false; // Movement failed
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
            
            // Check if there's an enemy unit (combat is allowed)
            Unit enemyAtPosition = GetEnemyAtPosition(targetPosition);
            if (enemyAtPosition != null)
            {
                return true; // We can "move" here to attack
            }
                
            // Check if position is occupied by a friendly unit
            if (unitSpawner.IsPositionOccupied(targetPosition))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Check if this unit can attack the target position
        /// </summary>
        public bool CanAttackPosition(Vector2Int targetPosition)
        {
            // Check bounds
            if (targetPosition.x < 0 || targetPosition.x >= groundManager.GridWidth ||
                targetPosition.y < 0 || targetPosition.y >= groundManager.GridHeight)
                return false;
            
            // Check if there's an enemy there
            Unit enemyAtPosition = GetEnemyAtPosition(targetPosition);
            return enemyAtPosition != null && enemyAtPosition.IsAlive();
        }
        
        // Utility methods for getting unit info
        public bool IsAlive() => health > 0;
        public float GetHealthPercentage() => maxHealth > 0 ? (float)health / maxHealth : 0f;
        public bool IsPlayerUnit(Player checkPlayer) => player == checkPlayer;
        public bool IsEnemyOf(Unit otherUnit) => otherUnit != null && otherUnit.player != this.player;
        
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
        
        /// <summary>
        /// Get combat preview information
        /// </summary>
        public string GetCombatPreview(Unit target)
        {
            if (target == null)
                return "No target";
                
            int damage = Mathf.Max(1, this.attack - target.defense);
            return $"{unitName} -> {target.unitName}: {damage} damage";
        }
    }
} 