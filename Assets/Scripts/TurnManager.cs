using UnityEngine;
using TurnClash.Units;

/// <summary>
/// Manages turn-based gameplay with player alternation and move limits
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Turn Settings")]
    [SerializeField] private int maxMovesPerTurn = 4;
    [SerializeField] private Unit.Player startingPlayer = Unit.Player.Player1;
    // Debug turns now controlled by DebugManager
    
    // Current turn state
    private Unit.Player currentPlayer;
    private int currentMoveCount;
    private int currentTurnNumber = 1; // Track overall turn number
    private bool gameStarted = false;
    
    // Events for other systems to subscribe to
    public System.Action<Unit.Player> OnTurnStart;
    public System.Action<Unit.Player> OnTurnEnd;
    public System.Action<Unit.Player, int, int> OnMoveCountChanged; // player, current moves, max moves
    public System.Action<Unit.Player, int> OnMoveUsed; // player, remaining moves
    public System.Action<int> OnTurnNumberChanged; // current turn number
    
    // Singleton instance
    private static TurnManager instance;
    private static bool isApplicationQuitting = false;
    
    public static TurnManager Instance
    {
        get
        {
            if (isApplicationQuitting)
                return null;
                
            if (instance == null)
            {
                instance = FindObjectOfType<TurnManager>();
                if (instance == null && !isApplicationQuitting)
                {
                    GameObject go = new GameObject("TurnManager");
                    instance = go.AddComponent<TurnManager>();
                }
            }
            return instance;
        }
    }
    
    // Public properties
    public Unit.Player CurrentPlayer => currentPlayer;
    public int CurrentMoveCount => currentMoveCount;
    public int MaxMovesPerTurn => maxMovesPerTurn;
    public int CurrentTurnNumber => currentTurnNumber;
    public bool IsGameStarted => gameStarted;
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Debug.Log("TurnManager: Destroying duplicate instance");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        isApplicationQuitting = false;
        Debug.Log("TurnManager: Instance created and initialized");
    }
    
    private void Start()
    {
        // Ensure the flag is cleared on scene start
        isApplicationQuitting = false;
        Debug.Log("TurnManager: Start() called, ready for operations");
        StartGame();
    }
    
    private void Update()
    {
        if (isApplicationQuitting || !gameStarted)
            return;
            
        // Check for early turn end input (X key) - but only if game is still active
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Check if game is over by seeing if any player has been eliminated
            if (IsGameOver())
            {
                // Don't allow turn skipping when game is over
                return;
            }
            
            EndTurnEarly();
        }
    }
    
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
#if DEBUG_TURNS
        Debug.Log("TurnManager: Application quitting, preventing new instance creation");
#endif
    }
    
    public void StartGame()
    {
        if (isApplicationQuitting) return;
        
        currentPlayer = startingPlayer;
        currentMoveCount = 0;
        currentTurnNumber = 1; // Reset turn counter
        gameStarted = true;
        
#if DEBUG_TURNS
        Debug.Log($"TurnManager: Game started! {currentPlayer} goes first. Turn {currentTurnNumber}");
#endif
            
        // Fire turn start event
        OnTurnStart?.Invoke(currentPlayer);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
        OnTurnNumberChanged?.Invoke(currentTurnNumber);
    }
    
    public void UseMove(Unit.Player player)
    {
        if (isApplicationQuitting || !gameStarted) return;
        
        // Verify it's the correct player's turn
        if (player != currentPlayer)
        {
            #if DEBUG_TURNS
        Debug.LogWarning($"TurnManager: {player} tried to use a move, but it's {currentPlayer}'s turn!");
#endif
            return;
        }
        
        // Increment move count
        currentMoveCount++;
        
#if DEBUG_TURNS
        Debug.Log($"TurnManager: {player} used move {currentMoveCount}/{maxMovesPerTurn}");
#endif
        
        // Fire move used event
        OnMoveUsed?.Invoke(currentPlayer, maxMovesPerTurn - currentMoveCount);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
        
        // Check if turn should end
        if (currentMoveCount >= maxMovesPerTurn)
        {
            EndTurn();
        }
    }
    
    public void EndTurnEarly()
    {
        if (isApplicationQuitting || !gameStarted) return;
        
#if DEBUG_TURNS
        Debug.Log($"TurnManager: {currentPlayer} ended their turn early ({currentMoveCount}/{maxMovesPerTurn} moves used)");
#endif
            
        EndTurn();
    }
    
    private void EndTurn()
    {
        if (isApplicationQuitting || !gameStarted) return;
        
        Unit.Player previousPlayer = currentPlayer;
        
        // Fire turn end event
        OnTurnEnd?.Invoke(currentPlayer);
        
        // Switch to other player
        currentPlayer = (currentPlayer == Unit.Player.Player1) ? Unit.Player.Player2 : Unit.Player.Player1;
        currentMoveCount = 0;
        
        // Increment turn number when switching players
        currentTurnNumber++;
        
#if DEBUG_TURNS
        Debug.Log($"TurnManager: Turn ended. {previousPlayer} -> {currentPlayer}. Now Turn {currentTurnNumber}");
#endif
        
        // Reset defending status for the current player's units
        ResetDefendingStatusForPlayer(currentPlayer);
        
        // Fire new turn start event
        OnTurnStart?.Invoke(currentPlayer);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
        OnTurnNumberChanged?.Invoke(currentTurnNumber);
    }
    
    public bool CanUnitMove(Unit unit)
    {
        if (isApplicationQuitting || !gameStarted || unit == null) return false;
        
        // Check if it's this player's turn
        if (unit.player != currentPlayer)
            return false;
            
        // Check if player has moves remaining
        return currentMoveCount < maxMovesPerTurn;
    }
    
    public int GetRemainingMoves()
    {
        return Mathf.Max(0, maxMovesPerTurn - currentMoveCount);
    }
    
    public string GetTurnInfo()
    {
        if (!gameStarted)
            return "Game not started";
            
        return $"Turn {currentTurnNumber} - {currentPlayer}'s Turn - Move {currentMoveCount + 1}/{maxMovesPerTurn}";
    }
    
    public void ResetGame()
    {
        if (isApplicationQuitting) return;
        
        currentPlayer = startingPlayer;
        currentMoveCount = 0;
        currentTurnNumber = 1; // Reset turn number
        gameStarted = false;
        
#if DEBUG_TURNS
        Debug.Log("TurnManager: Game reset");
#endif
            
        // Restart the game
        StartGame();
    }
    
    public void SetMaxMovesPerTurn(int moves)
    {
        maxMovesPerTurn = Mathf.Max(1, moves);
#if DEBUG_TURNS
        Debug.Log($"TurnManager: Max moves per turn set to {maxMovesPerTurn}");
#endif
    }
    
    public void SetStartingPlayer(Unit.Player player)
    {
        startingPlayer = player;
#if DEBUG_TURNS
        Debug.Log($"TurnManager: Starting player set to {startingPlayer}");
#endif
    }
    
    private void OnDestroy()
    {
#if DEBUG_TURNS
        Debug.Log("TurnManager: OnDestroy called");
#endif
            
        // Clear all event subscriptions
        OnTurnStart = null;
        OnTurnEnd = null;
        OnMoveCountChanged = null;
        OnMoveUsed = null;
        OnTurnNumberChanged = null;
        
        // Clear singleton reference
        if (instance == this)
        {
            instance = null;
        }
        
        // Only set quitting flag if we're actually quitting the application
        // Don't set it during scene changes or manual destroy
#if DEBUG_TURNS
        Debug.Log("TurnManager: Cleanup complete, instance cleared");
#endif
    }
    
    /// <summary>
    /// Check if the game is over by seeing if any player has been eliminated
    /// </summary>
    private bool IsGameOver()
    {
        // Try to find the CombatManager to check player elimination
        CombatManager combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            // Check if either player has been eliminated
            return combatManager.IsPlayerEliminated(Unit.Player.Player1) || 
                   combatManager.IsPlayerEliminated(Unit.Player.Player2);
        }
        
        return false; // If no combat manager found, assume game is still active
    }
    
    /// <summary>
    /// Reset defending status for all units of the specified player
    /// </summary>
    private void ResetDefendingStatusForPlayer(Unit.Player player)
    {
        if (isApplicationQuitting) return;
        
        // Find all units of the specified player and reset their defending status
        Unit[] allUnits = FindObjectsOfType<Unit>();
        int unitsReset = 0;
        
        foreach (Unit unit in allUnits)
        {
            if (unit.player == player && unit.IsDefending)
            {
                unit.StopDefending();
                unitsReset++;
                
#if DEBUG_TURNS
                Debug.Log($"TurnManager: Reset defending status for {unit.UnitName}");
#endif
            }
        }
        
#if DEBUG_TURNS
        if (unitsReset > 0)
            Debug.Log($"TurnManager: Reset defending status for {unitsReset} {player} units");
#endif
    }
} 