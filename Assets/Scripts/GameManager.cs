using UnityEngine;
using UnityEngine.SceneManagement;
using TurnClash.Units;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private IsometricGroundManager groundManager;
    [SerializeField] private UnitSpawner unitSpawner;
    [SerializeField] private UnitMovementController movementController;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CombatManager combatManager;
    
    [Header("Reset Settings")]
    [SerializeField] private bool allowReset = true;
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private bool showResetInstructions = true;
    
    private static GameManager instance;
    private static bool isApplicationQuitting = false;
    
    public static GameManager Instance
    {
        get 
        { 
            if (isApplicationQuitting)
                return null;
            return instance; 
        }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        isApplicationQuitting = false;
        
        Debug.Log("GameManager: Initializing all singleton systems...");
        
        // Force initialization of all singleton systems early to prevent timeout errors
        InitializeSingletonSystems();
        
        Debug.Log("GameManager: All singleton systems initialized");
    }
    
    private void InitializeSingletonSystems()
    {
        // Reset all singleton flags to ensure they can be created
        Debug.Log("GameManager: Resetting singleton flags for new scene...");
        UnitSelectionManager.ResetForNewScene();
        
        // Ensure managers are found
        if (groundManager == null)
            groundManager = FindObjectOfType<IsometricGroundManager>();
            
        if (unitSpawner == null)
            unitSpawner = FindObjectOfType<UnitSpawner>();
            
        // Initialize movement controller if not assigned
        if (movementController == null)
        {
            // Force singleton creation
            Debug.Log("GameManager: Initializing UnitMovementController...");
            movementController = UnitMovementController.Instance;
        }
        
        // Initialize turn manager if not assigned
        if (turnManager == null)
        {
            // Force singleton creation
            Debug.Log("GameManager: Initializing TurnManager...");
            turnManager = TurnManager.Instance;
        }
        
        // Initialize combat manager if not assigned
        if (combatManager == null)
        {
            // Force singleton creation
            Debug.Log("GameManager: Initializing CombatManager...");
            combatManager = CombatManager.Instance;
        }
        
        // Initialize UnitSelectionManager (often needed by UI)
        Debug.Log("GameManager: Initializing UnitSelectionManager...");
        var selectionManager = UnitSelectionManager.Instance;
        
        Debug.Log("GameManager: All singletons initialized and ready");
    }
    
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        Debug.Log("GameManager: Application quitting, preventing new instance creation");
    }
    
    private void Start()
    {
        // The ground manager will create the grid in its Start method
        // The unit spawner will spawn units in its Start method (with a frame delay)
        // The turn manager will initialize the turn system in its Start method
        // The combat manager will initialize combat tracking in its Start method
        
        Debug.Log("Game Manager initialized. Ground and units will be set up automatically.");
        Debug.Log("Arrow key movement is now available for selected units.");
        Debug.Log("Turn-based system is active - Player1 starts first!");
        Debug.Log("Combat system is active - move into enemy units to attack!");
        
        // Show controls including reset
        string controls = "Controls: Arrow keys to move/attack, X key to end turn early";
        if (allowReset && showResetInstructions)
        {
            controls += $", {resetKey} key to reset game";
        }
        Debug.Log(controls);
        
        // Subscribe to turn events for additional logging
        if (turnManager != null)
        {
            turnManager.OnTurnStart += OnPlayerTurnStart;
            turnManager.OnTurnEnd += OnPlayerTurnEnd;
            turnManager.OnMoveUsed += OnPlayerMoveUsed;
        }
        
        // Subscribe to combat events for game-level responses
        if (combatManager != null)
        {
            combatManager.OnPlayerEliminationCheck += OnPlayerEliminationCheck;
            combatManager.OnUnitKilled += OnUnitKilled;
        }
    }
    
    private void Update()
    {
        // Handle reset input
        if (allowReset && Input.GetKeyDown(resetKey))
        {
            Debug.Log($"Reset key ({resetKey}) pressed - restarting game...");
            ResetGame();
        }
    }
    
    private void OnDestroy()
    {
        Debug.Log("GameManager: OnDestroy called");
        
        // Unsubscribe from events to prevent memory leaks
        if (turnManager != null)
        {
            turnManager.OnTurnStart -= OnPlayerTurnStart;
            turnManager.OnTurnEnd -= OnPlayerTurnEnd;
            turnManager.OnMoveUsed -= OnPlayerMoveUsed;
        }
        
        if (combatManager != null)
        {
            combatManager.OnPlayerEliminationCheck -= OnPlayerEliminationCheck;
            combatManager.OnUnitKilled -= OnUnitKilled;
        }
        
        // Clear singleton reference
        if (instance == this)
        {
            instance = null;
        }
        
        Debug.Log("GameManager: Cleanup complete, instance cleared");
    }
    
    private void OnPlayerTurnStart(Unit.Player player)
    {
        Debug.Log($"=== {player}'s Turn Started ===");
    }
    
    private void OnPlayerTurnEnd(Unit.Player player)
    {
        Debug.Log($"=== {player}'s Turn Ended ===");
    }
    
    private void OnPlayerMoveUsed(Unit.Player player, int remainingMoves)
    {
        Debug.Log($"{player} has {remainingMoves} moves remaining this turn");
    }
    
    private void OnPlayerEliminationCheck(Unit.Player eliminatedPlayer)
    {
        Debug.Log($"🏆 GAME OVER: {eliminatedPlayer} has been eliminated!");
        
        // Determine winner
        Unit.Player winner = (eliminatedPlayer == Unit.Player.Player1) ? Unit.Player.Player2 : Unit.Player.Player1;
        Debug.Log($"🎉 {winner} wins the game!");
        
        // You could add game over logic here, such as:
        // - Show victory screen
        // - Stop turn system
        // - Display final statistics
        
        // For now, just show combat statistics
        if (combatManager != null)
        {
            Debug.Log($"\n{combatManager.GetCombatStatistics()}");
        }
    }
    
    private void OnUnitKilled(Unit attacker, Unit victim)
    {
        // Game-level response to unit kills
        // You could add effects, sounds, or other responses here
        Debug.Log($"Game Event: {victim.UnitName} eliminated by {attacker.UnitName}");
    }
    
    public IsometricGroundManager GetGroundManager()
    {
        return groundManager;
    }
    
    public UnitSpawner GetUnitSpawner()
    {
        return unitSpawner;
    }
    
    public UnitMovementController GetMovementController()
    {
        return movementController;
    }
    
    public TurnManager GetTurnManager()
    {
        return turnManager;
    }
    
    public CombatManager GetCombatManager()
    {
        return combatManager;
    }
    
    /// <summary>
    /// Get current game status information
    /// </summary>
    public string GetGameStatus()
    {
        if (turnManager == null || combatManager == null)
            return "Managers not initialized";
            
        string status = $"=== GAME STATUS ===\n";
        status += $"{turnManager.GetTurnInfo()}\n";
        status += $"Player 1 Units: {combatManager.GetRemainingUnitsForPlayer(Unit.Player.Player1)}\n";
        status += $"Player 2 Units: {combatManager.GetRemainingUnitsForPlayer(Unit.Player.Player2)}\n";
        status += $"==================";
        
        return status;
    }
    
    /// <summary>
    /// Reset the entire game to initial state by reloading the current scene
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("🔄 Resetting game - reloading scene...");
        
        // Mark that scene is unloading to prevent singleton creation during cleanup
        UnitSelectionManager.MarkSceneUnloading();
        
        // Unsubscribe from events before scene reload to prevent issues
        if (turnManager != null)
        {
            turnManager.OnTurnStart -= OnPlayerTurnStart;
            turnManager.OnTurnEnd -= OnPlayerTurnEnd;
            turnManager.OnMoveUsed -= OnPlayerMoveUsed;
        }
        
        if (combatManager != null)
        {
            combatManager.OnPlayerEliminationCheck -= OnPlayerEliminationCheck;
            combatManager.OnUnitKilled -= OnUnitKilled;
        }
        
        // Clear selection to prevent issues during reload
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.ClearSelection();
        }
        
        // Get current scene name and reload it
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Reloading scene: {currentSceneName}");
        
        // Reload the current scene
        SceneManager.LoadScene(currentSceneName);
    }
} 