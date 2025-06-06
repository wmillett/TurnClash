using UnityEngine;
using TurnClash.Units;

namespace TurnClash.Units
{
    /// <summary>
    /// Handles arrow key movement for selected units in the turn-based game
    /// </summary>
    public class UnitMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private bool enableArrowKeyMovement = true;
        [SerializeField] private bool enableDiagonalMovement = true; // Allow diagonal movement with key combinations
        [SerializeField] private float inputCooldown = 0.2f; // Prevent rapid movement
        [SerializeField] private bool debugMovement = false; // Re-disabled after fixing turn change integration
        [SerializeField] private bool respectTurnSystem = true; // Whether to check turn system before allowing movement
        [SerializeField] private bool workWithMovementPreview = true; // Allow both arrow keys and clickable tiles
        
        [Header("Combat Settings")]
        [SerializeField] private bool enableCombat = true; // Allow combat when moving to enemy tiles
        [SerializeField] private bool showCombatPreview = true; // Show damage preview in debug
        
        // Input timing
        private float lastInputTime;
        private bool[] keyPressed = new bool[4]; // Up, Down, Left, Right
        private float[] keyPressTime = new float[4];
        
        // Singleton instance
        private static UnitMovementController instance;
        private static bool isApplicationQuitting = false;
        
        public static UnitMovementController Instance
        {
            get
            {
                if (isApplicationQuitting)
                    return null;
                    
                if (instance == null)
                {
                    instance = FindObjectOfType<UnitMovementController>();
                    if (instance == null && !isApplicationQuitting)
                    {
                        GameObject go = new GameObject("UnitMovementController");
                        instance = go.AddComponent<UnitMovementController>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.Log("UnitMovementController: Destroying duplicate instance");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            isApplicationQuitting = false;
            Debug.Log("UnitMovementController: Instance created and initialized");
        }
        
        private void Start()
        {
            // Ensure the flag is cleared on scene start
            isApplicationQuitting = false;
            Debug.Log("UnitMovementController: Start() called, ready for operations");
        }
        
        private void Update()
        {
            if (isApplicationQuitting)
                return;
                
            if (enableArrowKeyMovement)
            {
                HandleArrowKeyInput();
            }
            
            // Handle defend action (G key)
            if (Input.GetKeyDown(KeyCode.G))
            {
                HandleDefendInput();
            }
        }
        
        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            if (debugMovement)
                Debug.Log("UnitMovementController: Application quitting, preventing new instance creation");
        }
        
        private void HandleArrowKeyInput()
        {
            // Check if game is over - don't allow arrow key movement when victory screen is active
            if (IsGameOver())
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: Cannot process arrow keys - game is over (victory screen active)");
                return;
            }
            
            // Note: Removed the check that disabled arrow keys when movement preview is active
            // Both systems (arrow keys and clickable tiles) should work together
            
            // Check each arrow key independently to allow diagonal movement
            Vector2Int direction = Vector2Int.zero;
            bool inputDetected = false;
            
            // Check vertical movement (Up/Down)
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction.y = 1; // Positive Y direction in grid space
                inputDetected = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction.y = -1; // Negative Y direction in grid space
                inputDetected = true;
            }
            
            // Check horizontal movement (Left/Right) - can combine with vertical for diagonal
            if (enableDiagonalMovement)
            {
                // Diagonal movement: allow both horizontal and vertical input simultaneously
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    direction.x = -1; // Negative X direction in grid space
                    inputDetected = true;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    direction.x = 1; // Positive X direction in grid space
                    inputDetected = true;
                }
            }
            else
            {
                // Non-diagonal movement: only allow horizontal if no vertical input
                if (direction.y == 0)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        direction.x = -1; // Negative X direction in grid space
                        inputDetected = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        direction.x = 1; // Positive X direction in grid space
                        inputDetected = true;
                    }
                }
            }
            
            // Process movement if input was detected and cooldown has passed
            if (inputDetected && Time.time - lastInputTime >= inputCooldown)
            {
                MoveSelectedUnit(direction);
                lastInputTime = Time.time;
            }
        }
        
        private void HandleDefendInput()
        {
            // Check if game is over - don't allow defending when victory screen is active
            if (IsGameOver())
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: Cannot defend - game is over (victory screen active)");
                return;
            }
            
            // Get the currently selected unit
            UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
            
            if (selectionManager == null || !selectionManager.HasSelection)
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: No unit selected for defend action");
                return;
            }
            
            UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
            if (selectedUnitSelectable == null)
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: Selected unit is null for defend action");
                return;
            }
            
            // Get the Unit component
            Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
            if (selectedUnit == null)
            {
                if (debugMovement)
                    Debug.LogError("UnitMovementController: Selected object doesn't have a Unit component for defend action");
                return;
            }
            
            // Check turn system restrictions
            if (respectTurnSystem)
            {
                TurnManager turnManager = TurnManager.Instance;
                if (turnManager != null && !turnManager.CanUnitMove(selectedUnit))
                {
                    if (debugMovement)
                    {
                        if (turnManager.CurrentPlayer != selectedUnit.player)
                        {
                            Debug.Log($"UnitMovementController: Cannot defend - it's {turnManager.CurrentPlayer}'s turn, not {selectedUnit.player}'s");
                        }
                        else
                        {
                            Debug.Log($"UnitMovementController: Cannot defend - no moves remaining this turn ({turnManager.CurrentMoveCount}/{turnManager.MaxMovesPerTurn})");
                        }
                    }
                    return;
                }
            }
            
            // Check if unit is already defending
            if (selectedUnit.IsDefending)
            {
                if (debugMovement)
                    Debug.Log($"UnitMovementController: {selectedUnit.UnitName} is already defending");
                return;
            }
            
            // Start defending
            selectedUnit.StartDefending();
            
            // Use a move in the turn system
            if (respectTurnSystem)
            {
                TurnManager turnManager = TurnManager.Instance;
                if (turnManager != null)
                {
                    turnManager.UseMove(selectedUnit.player);
                }
            }
            
            if (debugMovement)
                Debug.Log($"UnitMovementController: {selectedUnit.UnitName} is now defending!");
        }
        
        private void MoveSelectedUnit(Vector2Int direction)
        {
            if (isApplicationQuitting) return;
            
            // Get the currently selected unit
            UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
            
            if (selectionManager == null || !selectionManager.HasSelection)
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: No unit selected for movement");
                return;
            }
            
            UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
            if (selectedUnitSelectable == null)
            {
                if (debugMovement)
                    Debug.Log("UnitMovementController: Selected unit is null");
                return;
            }
            
            // Get the Unit component
            Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
            if (selectedUnit == null)
            {
                if (debugMovement)
                    Debug.LogError("UnitMovementController: Selected object doesn't have a Unit component");
                return;
            }
            
            // Check turn system restrictions
            if (respectTurnSystem)
            {
                TurnManager turnManager = TurnManager.Instance;
                if (turnManager != null && !turnManager.CanUnitMove(selectedUnit))
                {
                    if (debugMovement)
                    {
                        if (turnManager.CurrentPlayer != selectedUnit.player)
                        {
                            Debug.Log($"UnitMovementController: Cannot move {selectedUnit.player} unit - it's {turnManager.CurrentPlayer}'s turn");
                        }
                        else
                        {
                            Debug.Log($"UnitMovementController: Cannot move unit - no moves remaining this turn ({turnManager.CurrentMoveCount}/{turnManager.MaxMovesPerTurn})");
                        }
                    }
                    return;
                }
            }
            
            // Calculate target position
            Vector2Int currentPos = selectedUnit.GetGridPosition();
            Vector2Int targetPos = currentPos + direction;
            
            // Check if there's an enemy at the target position (for combat)
            Unit enemyAtTarget = null;
            if (enableCombat)
            {
                enemyAtTarget = selectedUnit.GetEnemyAtPosition(targetPos);
            }
            
            if (debugMovement)
            {
                string movementType = GetDirectionName(direction);
                if (enemyAtTarget != null)
                {
                    Debug.Log($"UnitMovementController: {selectedUnit.player} unit will attack {enemyAtTarget.UnitName} at {targetPos} ({movementType})");
                    if (showCombatPreview)
                    {
                        Debug.Log($"Combat Preview: {selectedUnit.GetCombatPreview(enemyAtTarget)}");
                    }
                }
                else
                {
                    Debug.Log($"UnitMovementController: Attempting to move {selectedUnit.player} unit {movementType} from {currentPos} to {targetPos}");
                }
            }
            
            // Try to move/attack using the enhanced movement method
            bool actionSuccessful = false;
            
            if (enableCombat)
            {
                // Use the new TryMoveToPosition method that handles both movement and combat
                actionSuccessful = selectedUnit.TryMoveToPosition(targetPos);
            }
            else
            {
                // Use old movement system (combat disabled)
                if (selectedUnit.CanMoveTo(targetPos))
                {
                    selectedUnit.SetGridPosition(targetPos);
                    actionSuccessful = true;
                }
            }
            
            if (actionSuccessful)
            {
                // Use a move in the turn system
                if (respectTurnSystem)
                {
                    TurnManager turnManager = TurnManager.Instance;
                    if (turnManager != null)
                    {
                        turnManager.UseMove(selectedUnit.player);
                    }
                }
                
                // Refresh movement preview after successful arrow key movement
                // This ensures the clickable tiles update to show new available moves from the new position
                if (workWithMovementPreview && MovementPreview.Instance != null)
                {
                    // Refresh preview whether we moved or attacked - the preview system will handle dead units
                    MovementPreview.Instance.RefreshCurrentUnitPreview();
                }
                
                if (debugMovement)
                {
                    string movementType = GetDirectionName(direction);
                    if (enemyAtTarget != null)
                    {
                        Debug.Log($"UnitMovementController: Combat completed - {selectedUnit.UnitName} attacked {enemyAtTarget.UnitName} ({movementType})");
                    }
                    else
                    {
                        Debug.Log($"UnitMovementController: Successfully moved {selectedUnit.player} unit {movementType} to {targetPos}");
                    }
                }
            }
            else
            {
                if (debugMovement)
                {
                    Debug.Log($"UnitMovementController: Action failed - cannot move/attack at {targetPos}");
                }
            }
        }
        
        /// <summary>
        /// Enable or disable arrow key movement
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            enableArrowKeyMovement = enabled;
            if (debugMovement)
                Debug.Log($"UnitMovementController: Arrow key movement {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set the input cooldown time
        /// </summary>
        public void SetInputCooldown(float cooldown)
        {
            inputCooldown = Mathf.Max(0f, cooldown);
        }
        
        /// <summary>
        /// Enable or disable turn system integration
        /// </summary>
        public void SetTurnSystemRespect(bool respect)
        {
            respectTurnSystem = respect;
            if (debugMovement)
                Debug.Log($"UnitMovementController: Turn system respect {(respect ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Enable or disable combat system
        /// </summary>
        public void SetCombatEnabled(bool enabled)
        {
            enableCombat = enabled;
            if (debugMovement)
                Debug.Log($"UnitMovementController: Combat system {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Enable or disable working with movement preview system
        /// </summary>
        public void SetWorkWithMovementPreview(bool enabled)
        {
            workWithMovementPreview = enabled;
            if (debugMovement)
                Debug.Log($"UnitMovementController: Movement preview cooperation {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Enable or disable diagonal movement
        /// </summary>
        public void SetDiagonalMovementEnabled(bool enabled)
        {
            enableDiagonalMovement = enabled;
            if (debugMovement)
                Debug.Log($"UnitMovementController: Diagonal movement {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Manual movement method for other systems to use
        /// </summary>
        public bool TryMoveSelectedUnit(Vector2Int direction)
        {
            if (isApplicationQuitting) return false;
            
            MoveSelectedUnit(direction);
            return true; // You could modify this to return actual success/failure
        }
        
        /// <summary>
        /// Check if the currently selected unit can move or attack
        /// </summary>
        public bool CanSelectedUnitMove()
        {
            if (isApplicationQuitting) return false;
            
            UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
            if (selectionManager == null || !selectionManager.HasSelection)
                return false;
                
            UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
            if (selectedUnitSelectable == null)
                return false;
                
            Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
            if (selectedUnit == null)
                return false;
                
            // Check turn system if enabled
            if (respectTurnSystem)
            {
                TurnManager turnManager = TurnManager.Instance;
                if (turnManager != null)
                {
                    return turnManager.CanUnitMove(selectedUnit);
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get information about what would happen if the selected unit moved in a direction
        /// Supports diagonal movement analysis
        /// </summary>
        public string GetMovePreview(Vector2Int direction)
        {
            if (isApplicationQuitting) return "Application quitting";
            
            UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
            if (selectionManager == null || !selectionManager.HasSelection)
                return "No unit selected";
                
            UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
            if (selectedUnitSelectable == null)
                return "No unit selected";
                
            Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
            if (selectedUnit == null)
                return "Invalid unit";
                
            Vector2Int currentPos = selectedUnit.GetGridPosition();
            Vector2Int targetPos = currentPos + direction;
            
            // Get direction description for better feedback
            string directionName = GetDirectionName(direction);
            
            Unit enemyAtTarget = selectedUnit.GetEnemyAtPosition(targetPos);
            if (enemyAtTarget != null)
            {
                return $"{directionName}: {selectedUnit.GetCombatPreview(enemyAtTarget)}";
            }
            else if (selectedUnit.CanMoveTo(targetPos))
            {
                return $"{directionName}: Move to {targetPos}";
            }
            else
            {
                return $"{directionName}: Cannot move there";
            }
        }
        
        /// <summary>
        /// Get a human-readable name for a movement direction
        /// </summary>
        private string GetDirectionName(Vector2Int direction)
        {
            if (direction.x == 0 && direction.y == 1) return "North";
            if (direction.x == 0 && direction.y == -1) return "South";
            if (direction.x == -1 && direction.y == 0) return "West";
            if (direction.x == 1 && direction.y == 0) return "East";
            if (direction.x == 1 && direction.y == 1) return "Northeast";
            if (direction.x == -1 && direction.y == 1) return "Northwest";
            if (direction.x == 1 && direction.y == -1) return "Southeast";
            if (direction.x == -1 && direction.y == -1) return "Southwest";
            return "Unknown";
        }
        
        /// <summary>
        /// Get movement direction based on arrow key input (for external use)
        /// Supports diagonal movement by detecting key combinations
        /// </summary>
        public Vector2Int GetMovementInput()
        {
            if (!enableArrowKeyMovement || isApplicationQuitting)
                return Vector2Int.zero;
                
            Vector2Int direction = Vector2Int.zero;
            
            // Check vertical input (Up/Down)
            if (Input.GetKey(KeyCode.UpArrow))
                direction.y = 1;
            else if (Input.GetKey(KeyCode.DownArrow))
                direction.y = -1;
                
            // Check horizontal input (Left/Right)
            if (enableDiagonalMovement)
            {
                // Diagonal movement: allow both horizontal and vertical input simultaneously
                if (Input.GetKey(KeyCode.LeftArrow))
                    direction.x = -1;
                else if (Input.GetKey(KeyCode.RightArrow))
                    direction.x = 1;
            }
            else
            {
                // Non-diagonal movement: only allow horizontal if no vertical input
                if (direction.y == 0)
                {
                    if (Input.GetKey(KeyCode.LeftArrow))
                        direction.x = -1;
                    else if (Input.GetKey(KeyCode.RightArrow))
                        direction.x = 1;
                }
            }
                
            return direction;
        }
        
        /// <summary>
        /// Check if the game is over by seeing if any player has been eliminated
        /// </summary>
        private bool IsGameOver()
        {
            // Try to find the CombatManager to check player elimination
            var combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
            {
                // Check if either player has been eliminated
                return combatManager.IsPlayerEliminated(Unit.Player.Player1) || 
                       combatManager.IsPlayerEliminated(Unit.Player.Player2);
            }
            
            return false; // If no combat manager found, assume game is still active
        }
        
        /// <summary>
        /// Check if movement preview is currently showing highlighted tiles
        /// Note: This no longer blocks arrow key input - both systems work together
        /// </summary>
        private bool HasMovementPreviewActive()
        {
            // Check if there's a selected unit that would have movement preview
            UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
            if (selectionManager == null || !selectionManager.HasSelection)
                return false;
                
            UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
            if (selectedUnitSelectable == null)
                return false;
                
            Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
            if (selectedUnit == null)
                return false;
                
            // Check if it's the current player's turn and they can move
            TurnManager turnManager = TurnManager.Instance;
            if (turnManager != null)
            {
                return turnManager.CurrentPlayer == selectedUnit.player && turnManager.CanUnitMove(selectedUnit);
            }
            
            return false;
        }
        
        private void OnDestroy()
        {
            if (debugMovement)
                Debug.Log("UnitMovementController: OnDestroy called");
                
            // Clear singleton reference
            if (instance == this)
            {
                instance = null;
            }
            
            // Only set quitting flag if we're actually quitting the application
            // Don't set it during scene changes or manual destroy
            if (debugMovement)
                Debug.Log("UnitMovementController: Cleanup complete, instance cleared");
        }
    }
} 