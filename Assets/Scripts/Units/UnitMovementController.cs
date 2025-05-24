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
        [SerializeField] private float inputCooldown = 0.2f; // Prevent rapid movement
        [SerializeField] private bool debugMovement = true;
        [SerializeField] private bool respectTurnSystem = true; // Whether to check turn system before allowing movement
        
        // Input timing
        private float lastInputTime;
        private bool[] keyPressed = new bool[4]; // Up, Down, Left, Right
        private float[] keyPressTime = new float[4];
        
        // Singleton instance
        private static UnitMovementController instance;
        public static UnitMovementController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UnitMovementController>();
                    if (instance == null)
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
        }
        
        private void Update()
        {
            if (!enableArrowKeyMovement)
                return;
                
            HandleArrowKeyInput();
        }
        
        private void HandleArrowKeyInput()
        {
            // Check each arrow key
            Vector2Int direction = Vector2Int.zero;
            bool inputDetected = false;
            
            // Up Arrow (moves in positive Y direction in grid space)
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = new Vector2Int(0, 1);
                inputDetected = true;
            }
            // Down Arrow (moves in negative Y direction in grid space)
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = new Vector2Int(0, -1);
                inputDetected = true;
            }
            // Left Arrow (moves in negative X direction in grid space)
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction = new Vector2Int(-1, 0);
                inputDetected = true;
            }
            // Right Arrow (moves in positive X direction in grid space)
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction = new Vector2Int(1, 0);
                inputDetected = true;
            }
            
            // Process movement if input was detected and cooldown has passed
            if (inputDetected && Time.time - lastInputTime >= inputCooldown)
            {
                MoveSelectedUnit(direction);
                lastInputTime = Time.time;
            }
        }
        
        private void MoveSelectedUnit(Vector2Int direction)
        {
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
            
            if (debugMovement)
            {
                Debug.Log($"UnitMovementController: Attempting to move {selectedUnit.player} unit from {currentPos} to {targetPos}");
            }
            
            // Check if the move is valid (position, collision, etc.)
            if (selectedUnit.CanMoveTo(targetPos))
            {
                // Perform the movement
                selectedUnit.SetGridPosition(targetPos);
                
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
                {
                    Debug.Log($"UnitMovementController: Successfully moved {selectedUnit.player} unit to {targetPos}");
                }
            }
            else
            {
                if (debugMovement)
                {
                    Debug.Log($"UnitMovementController: Cannot move to {targetPos} - invalid position");
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
        /// Manual movement method for other systems to use
        /// </summary>
        public bool TryMoveSelectedUnit(Vector2Int direction)
        {
            MoveSelectedUnit(direction);
            return true; // You could modify this to return actual success/failure
        }
        
        /// <summary>
        /// Check if the currently selected unit can move
        /// </summary>
        public bool CanSelectedUnitMove()
        {
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
        /// Get movement direction based on arrow key input (for external use)
        /// </summary>
        public Vector2Int GetMovementInput()
        {
            if (!enableArrowKeyMovement)
                return Vector2Int.zero;
                
            Vector2Int direction = Vector2Int.zero;
            
            if (Input.GetKey(KeyCode.UpArrow))
                direction.y = 1;
            else if (Input.GetKey(KeyCode.DownArrow))
                direction.y = -1;
                
            if (Input.GetKey(KeyCode.LeftArrow))
                direction.x = -1;
            else if (Input.GetKey(KeyCode.RightArrow))
                direction.x = 1;
                
            return direction;
        }
    }
} 