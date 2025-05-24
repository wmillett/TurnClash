using UnityEngine;
using TurnClash.Units;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private IsometricGroundManager groundManager;
    [SerializeField] private UnitSpawner unitSpawner;
    [SerializeField] private UnitMovementController movementController;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CombatManager combatManager;
    
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Ensure managers are found
        if (groundManager == null)
            groundManager = FindObjectOfType<IsometricGroundManager>();
            
        if (unitSpawner == null)
            unitSpawner = FindObjectOfType<UnitSpawner>();
            
        // Initialize movement controller if not assigned
        if (movementController == null)
        {
            // The UnitMovementController will create itself as a singleton
            movementController = UnitMovementController.Instance;
        }
        
        // Initialize turn manager if not assigned
        if (turnManager == null)
        {
            // The TurnManager will create itself as a singleton
            turnManager = TurnManager.Instance;
        }
        
        // Initialize combat manager if not assigned
        if (combatManager == null)
        {
            // The CombatManager will create itself as a singleton
            combatManager = CombatManager.Instance;
        }
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
        Debug.Log("Controls: Arrow keys to move/attack, X key to end turn early");
        
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
    
    private void OnDestroy()
    {
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
    /// Reset the entire game to initial state
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("Resetting game...");
        
        if (turnManager != null)
            turnManager.ResetGame();
            
        if (combatManager != null)
            combatManager.ResetStatistics();
            
        Debug.Log("Game reset complete!");
    }
} 