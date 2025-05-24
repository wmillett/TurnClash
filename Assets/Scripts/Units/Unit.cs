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
        public int defence = 5;
        
        [Header("Unit Properties")]
        [SerializeField] private string unitName = "Unit";
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Combat Settings")]
        [SerializeField] private bool debugCombat = true;
        
        [Header("Grid Movement")]
        private Vector2Int currentGridPosition;
        private IsometricGroundManager groundManager;
        private UnitSpawner unitSpawner;
        
        [Header("Movement Animation")]
        [SerializeField] private bool animateMovement = true;
        [SerializeField] private float movementAnimationSpeed = 10f;
        [SerializeField] private bool moveToVictimPosition = true; // New feature toggle
        
        // Movement animation state
        private bool isAnimating = false;
        private Vector3 animationStartPos;
        private Vector3 animationTargetPos;
        private float animationProgress = 0f;
        
        // Player enum moved here since it's no longer in Creature
        public enum Player { Player1, Player2 }
        
        // Events for combat system
        public System.Action<Unit, Unit, int> OnAttackPerformed; // attacker, defender, damage
        public System.Action<Unit> OnUnitDestroyed;
        public System.Action<Unit, Vector2Int> OnUnitAdvancedToPosition; // New event for advance movement
        
        // Public property to access unit name
        public string UnitName 
        { 
            get { return unitName; } 
            set { unitName = value; }
        }
        
        protected virtual void Start()
        {
            Debug.Log($"=== {unitName} Starting Initialization ===");
            
            // Find references
            groundManager = FindObjectOfType<IsometricGroundManager>();
            unitSpawner = FindObjectOfType<UnitSpawner>();
            
            if (groundManager == null)
                Debug.LogError($"Unit {unitName}: IsometricGroundManager not found!");
            else
                Debug.Log($"Unit {unitName}: Found GroundManager with {groundManager.GridWidth}x{groundManager.GridHeight} grid");
                
            if (unitSpawner == null)
                Debug.LogError($"Unit {unitName}: UnitSpawner not found!");
            else
                Debug.Log($"Unit {unitName}: Found UnitSpawner");
                
            // Initialize current health to max health
            health = maxHealth;
            
            // CRITICAL: Initialize grid position based on current world position
            if (groundManager != null)
            {
                Vector3 currentWorldPos = transform.position;
                currentGridPosition = groundManager.WorldToGridPosition(currentWorldPos);
                Debug.Log($"Unit {unitName} at world position {currentWorldPos} converted to grid position {currentGridPosition}");
                
                // Verify the position is valid
                if (currentGridPosition.x < 0 || currentGridPosition.x >= groundManager.GridWidth ||
                    currentGridPosition.y < 0 || currentGridPosition.y >= groundManager.GridHeight)
                {
                    Debug.LogWarning($"Unit {unitName} initialized to INVALID grid position {currentGridPosition}! Grid bounds: {groundManager.GridWidth}x{groundManager.GridHeight}");
                }
                else
                {
                    Debug.Log($"Unit {unitName} successfully initialized at VALID grid position {currentGridPosition}");
                }
            }
            else
            {
                Debug.LogError($"Unit {unitName}: Cannot initialize grid position - no GroundManager!");
            }
            
            Debug.Log($"Unit {unitName} initialized with {health}/{maxHealth} HP, {attack} ATK, {defence} DEF");
            Debug.Log($"=== {unitName} Initialization Complete ===");
        }
        
        private void Update()
        {
            // Handle movement animation ONLY for advance movements
            // Normal movements via SetGridPosition() are immediate
            if (isAnimating && animateMovement)
            {
                animationProgress += Time.deltaTime * movementAnimationSpeed;
                
                if (animationProgress >= 1f)
                {
                    // Animation complete
                    transform.position = animationTargetPos;
                    isAnimating = false;
                    animationProgress = 0f;
                    
                    if (debugCombat)
                        Debug.Log($"{unitName} animation completed at {animationTargetPos}");
                }
                else
                {
                    // Use Vector3.MoveTowards for smooth movement
                    float maxDistance = movementAnimationSpeed * Time.deltaTime;
                    Vector3 newPos = Vector3.MoveTowards(transform.position, animationTargetPos, maxDistance);
                    transform.position = newPos;
                }
            }
        }
        
        public void TakeDamage(int damage)
        {
            damage = Mathf.Max(0, damage - defence); // defence reduces damage

            health -= damage;
            Debug.Log($"{unitName} took {damage} damage (after defence). Health: {health}/{maxHealth}");
            
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
            
            // Store the target's position before the attack
            Vector2Int victimPosition = target.GetGridPosition();
            bool wasTargetAlive = target.IsAlive();
            
            // Calculate damage: Attacker's attack - Defender's defence
            int baseDamage = this.attack;
            int finalDamage = Mathf.Max(1, baseDamage - target.defence); // Minimum 1 damage
            
            if (debugCombat)
            {
                Debug.Log($"COMBAT: {unitName} attacks {target.unitName}!");
                Debug.Log($"Damage calculation: {baseDamage} (ATK) - {target.defence} (DEF) = {finalDamage} damage");
            }
            
            // Apply damage to target
            target.TakeDamage(baseDamage); // TakeDamage already handles defence calculation
            
            // Check if the target died from this attack
            bool targetDiedFromAttack = wasTargetAlive && !target.IsAlive();
            
            // Fire combat event
            OnAttackPerformed?.Invoke(this, target, finalDamage);
            
            // If the target died and we have the "move to victim position" feature enabled
            if (targetDiedFromAttack && moveToVictimPosition)
            {
                if (debugCombat)
                    Debug.Log($"{unitName} advances to {target.unitName}'s position at {victimPosition}!");
                
                // Move to the victim's position without consuming movement points
                AdvanceToPosition(victimPosition);
                
                // Fire advance event
                OnUnitAdvancedToPosition?.Invoke(this, victimPosition);
            }
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
            Debug.Log($"=== {unitName} SetGridPosition called: {currentGridPosition} -> {newPosition} ===");
            
            if (unitSpawner != null && groundManager != null)
            {
                // Update the spawner's tracking
                Debug.Log($"{unitName}: Updating spawner position tracking");
                unitSpawner.UpdateUnitPosition(currentGridPosition, newPosition);
                
                // Update our position
                Vector2Int oldPosition = currentGridPosition;
                currentGridPosition = newPosition;
                Debug.Log($"{unitName}: Grid position updated from {oldPosition} to {currentGridPosition}");
                
                // Get the tile to find its actual height
                IsometricGroundTile tile = groundManager.GetTileAtPosition(newPosition);
                if (tile != null)
                {
                    Vector3 worldPos = tile.transform.position;
                    Debug.Log($"{unitName}: Found tile at {newPosition}, tile world position: {worldPos}");
                    
                    // Get the tile's renderer to find its actual height
                    MeshRenderer tileRenderer = tile.GetComponent<MeshRenderer>();
                    if (tileRenderer != null)
                    {
                        // Position unit on top of the tile
                        float tileTopY = tileRenderer.bounds.max.y;
                        worldPos.y = tileTopY + 0.5f; // Same offset as spawner
                        Debug.Log($"{unitName}: Using renderer bounds, calculated world position: {worldPos}");
                    }
                    else
                    {
                        // Fallback calculation if no renderer
                        worldPos.y = tile.transform.position.y + (tile.transform.localScale.y * 0.5f) + 0.5f;
                        Debug.Log($"{unitName}: Using fallback calculation, calculated world position: {worldPos}");
                    }
                    
                    // IMPORTANT: Normal movements are immediate - no animation
                    // Animation is ONLY used for advance movements via AdvanceToPosition()
                    Vector3 oldWorldPos = transform.position;
                    transform.position = worldPos;
                    Debug.Log($"{unitName}: World position updated from {oldWorldPos} to {worldPos}");
                    
                    // Stop any ongoing animation since this is a direct position set
                    if (isAnimating)
                    {
                        Debug.Log($"{unitName}: Stopping ongoing animation for direct position set");
                        isAnimating = false;
                        animationProgress = 0f;
                    }
                }
                else
                {
                    Debug.LogError($"{unitName}: No tile found at position {newPosition}!");
                }
            }
            else
            {
                Debug.LogError($"{unitName}: SetGridPosition failed - missing unitSpawner ({unitSpawner != null}) or groundManager ({groundManager != null})");
            }
            
            Debug.Log($"=== {unitName} SetGridPosition complete ===");
        }
        
        /// <summary>
        /// Enhanced movement method that handles combat
        /// </summary>
        public bool TryMoveToPosition(Vector2Int targetPosition)
        {
            Debug.Log($"=== {unitName} TryMoveToPosition: {GetGridPosition()} -> {targetPosition} ===");
            
            // Check if there's an enemy at the target position
            Unit enemyAtTarget = GetEnemyAtPosition(targetPosition);
            
            if (enemyAtTarget != null)
            {
                // Attack the enemy instead of moving
                Debug.Log($"{unitName}: Found enemy {enemyAtTarget.UnitName} at {targetPosition}, attacking");
                AttackUnit(enemyAtTarget);
                return true; // Combat occurred, so the action was successful
            }
            
            // No enemy, check if we can move normally
            Debug.Log($"{unitName}: No enemy at {targetPosition}, checking if can move");
            bool canMove = CanMoveTo(targetPosition);
            Debug.Log($"{unitName}: CanMoveTo result: {canMove}");
            
            if (canMove)
            {
                Debug.Log($"{unitName}: Moving to {targetPosition}");
                SetGridPosition(targetPosition);
                Debug.Log($"{unitName}: Movement complete, new position: {GetGridPosition()}");
                return true;
            }
            
            Debug.Log($"{unitName}: Cannot move to {targetPosition}");
            return false; // Movement failed
        }
        
        public bool CanMoveTo(Vector2Int targetPosition)
        {
            Debug.Log($"=== {unitName} CanMoveTo checking {targetPosition} ===");
            
            if (groundManager == null || unitSpawner == null)
            {
                Debug.LogError($"{unitName}: CanMoveTo failed - missing managers (groundManager: {groundManager != null}, unitSpawner: {unitSpawner != null})");
                return false;
            }
                
            // Check if position is within grid bounds
            if (targetPosition.x < 0 || targetPosition.x >= groundManager.GridWidth ||
                targetPosition.y < 0 || targetPosition.y >= groundManager.GridHeight)
            {
                Debug.Log($"{unitName}: CanMoveTo failed - position {targetPosition} is outside grid bounds (0-{groundManager.GridWidth-1}, 0-{groundManager.GridHeight-1})");
                return false;
            }
                
            // Check if tile is walkable
            IsometricGroundTile tile = groundManager.GetTileAtPosition(targetPosition);
            if (tile == null)
            {
                Debug.LogError($"{unitName}: CanMoveTo failed - no tile found at position {targetPosition}");
                return false;
            }
            
            if (!tile.IsWalkable())
            {
                Debug.Log($"{unitName}: CanMoveTo failed - tile at {targetPosition} is not walkable");
                return false;
            }
            
            // Check if there's an enemy unit (combat is allowed)
            Unit enemyAtPosition = GetEnemyAtPosition(targetPosition);
            if (enemyAtPosition != null)
            {
                Debug.Log($"{unitName}: CanMoveTo true - enemy {enemyAtPosition.UnitName} at {targetPosition} (combat allowed)");
                return true; // We can "move" here to attack
            }
                
            // Check if position is occupied by a friendly unit
            bool isOccupied = unitSpawner.IsPositionOccupied(targetPosition);
            if (isOccupied)
            {
                Debug.Log($"{unitName}: CanMoveTo failed - position {targetPosition} is occupied by friendly unit");
                return false;
            }
            
            Debug.Log($"{unitName}: CanMoveTo true - position {targetPosition} is available");
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
            defence = Mathf.Max(0, defence + defenseMod);
            
            // Ensure current health doesn't exceed new max
            health = Mathf.Min(health, maxHealth);
            
            Debug.Log($"{unitName} stats modified. New stats - HP:{health}/{maxHealth}, ATK:{attack}, DEF:{defence}");
        }
        
        /// <summary>
        /// Get combat preview information
        /// </summary>
        public string GetCombatPreview(Unit target)
        {
            if (target == null)
                return "No target";
                
            int damage = Mathf.Max(1, this.attack - target.defence);
            return $"{unitName} -> {target.unitName}: {damage} damage";
        }
        
        /// <summary>
        /// Move to a position without consuming movement points (used for advancing after kills)
        /// This is the ONLY method that should use animation - normal movements are immediate
        /// </summary>
        public void AdvanceToPosition(Vector2Int targetPosition)
        {
            if (groundManager == null || unitSpawner == null)
            {
                Debug.LogError($"{unitName}: Cannot advance - missing managers");
                return;
            }
            
            // Check if target position is valid
            if (targetPosition.x < 0 || targetPosition.x >= groundManager.GridWidth ||
                targetPosition.y < 0 || targetPosition.y >= groundManager.GridHeight)
            {
                Debug.LogError($"{unitName}: Cannot advance to invalid position {targetPosition}");
                return;
            }
            
            // Get the target tile position
            IsometricGroundTile targetTile = groundManager.GetTileAtPosition(targetPosition);
            if (targetTile == null)
            {
                Debug.LogError($"{unitName}: Cannot advance - no tile at position {targetPosition}");
                return;
            }
            
            // Calculate target world position
            Vector3 targetWorldPos = targetTile.transform.position;
            MeshRenderer tileRenderer = targetTile.GetComponent<MeshRenderer>();
            if (tileRenderer != null)
            {
                float tileTopY = tileRenderer.bounds.max.y;
                targetWorldPos.y = tileTopY + 0.5f;
            }
            else
            {
                targetWorldPos.y = targetTile.transform.position.y + (targetTile.transform.localScale.y * 0.5f) + 0.5f;
            }
            
            // Update position tracking first
            if (unitSpawner != null)
            {
                unitSpawner.UpdateUnitPosition(currentGridPosition, targetPosition);
            }
            currentGridPosition = targetPosition;
            
            // Handle movement animation ONLY for advance movements
            if (animateMovement && !isAnimating)
            {
                // Start animation for advance movement
                animationStartPos = transform.position;
                animationTargetPos = targetWorldPos;
                animationProgress = 0f;
                isAnimating = true;
                
                if (debugCombat)
                    Debug.Log($"{unitName} starting ADVANCE animation from {animationStartPos} to {targetPosition}");
            }
            else
            {
                // Instant movement (animation disabled or already animating)
                transform.position = targetWorldPos;
                if (debugCombat)
                    Debug.Log($"{unitName} instantly advanced to {targetPosition} (animation disabled)");
            }
        }
    }
} 