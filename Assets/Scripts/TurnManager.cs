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
    [SerializeField] private bool debugTurns = true;
    
    // Current turn state
    private Unit.Player currentPlayer;
    private int currentMoveCount;
    private bool gameStarted = false;
    
    // Events for other systems to subscribe to
    public System.Action<Unit.Player> OnTurnStart;
    public System.Action<Unit.Player> OnTurnEnd;
    public System.Action<Unit.Player, int, int> OnMoveCountChanged; // player, current moves, max moves
    public System.Action<Unit.Player, int> OnMoveUsed; // player, remaining moves
    
    // Singleton instance
    private static TurnManager instance;
    public static TurnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TurnManager>();
                if (instance == null)
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
    public int RemainingMoves => maxMovesPerTurn - currentMoveCount;
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
    }
    
    private void Start()
    {
        InitializeTurnSystem();
    }
    
    private void Update()
    {
        // Check for early turn end (X key)
        if (gameStarted && Input.GetKeyDown(KeyCode.X))
        {
            EndTurnEarly();
        }
    }
    
    private void InitializeTurnSystem()
    {
        currentPlayer = startingPlayer;
        currentMoveCount = 0;
        gameStarted = true;
        
        if (debugTurns)
        {
            Debug.Log($"TurnManager: Game started! {currentPlayer}'s turn begins.");
        }
        
        // Notify systems that the first turn has started
        OnTurnStart?.Invoke(currentPlayer);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
    }
    
    /// <summary>
    /// Check if the specified player can make a move
    /// </summary>
    public bool CanPlayerMove(Unit.Player player)
    {
        if (!gameStarted)
            return false;
            
        return player == currentPlayer && currentMoveCount < maxMovesPerTurn;
    }
    
    /// <summary>
    /// Check if a specific unit can move based on current turn rules
    /// </summary>
    public bool CanUnitMove(Unit unit)
    {
        if (unit == null || !gameStarted)
            return false;
            
        return CanPlayerMove(unit.player);
    }
    
    /// <summary>
    /// Use a move for the current player
    /// </summary>
    public bool UseMove(Unit.Player player)
    {
        if (!CanPlayerMove(player))
        {
            if (debugTurns)
                Debug.Log($"TurnManager: {player} cannot make a move (current player: {currentPlayer}, moves: {currentMoveCount}/{maxMovesPerTurn})");
            return false;
        }
        
        currentMoveCount++;
        
        if (debugTurns)
        {
            Debug.Log($"TurnManager: {player} used move {currentMoveCount}/{maxMovesPerTurn}");
        }
        
        // Notify systems about move usage
        OnMoveUsed?.Invoke(currentPlayer, RemainingMoves);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
        
        // Check if turn should end
        if (currentMoveCount >= maxMovesPerTurn)
        {
            EndTurn();
        }
        
        return true;
    }
    
    /// <summary>
    /// End the current turn and switch to the next player
    /// </summary>
    public void EndTurn()
    {
        if (!gameStarted)
            return;
            
        Unit.Player previousPlayer = currentPlayer;
        
        // Notify about turn ending
        OnTurnEnd?.Invoke(previousPlayer);
        
        // Switch to next player
        currentPlayer = (currentPlayer == Unit.Player.Player1) ? Unit.Player.Player2 : Unit.Player.Player1;
        currentMoveCount = 0;
        
        if (debugTurns)
        {
            Debug.Log($"TurnManager: {previousPlayer}'s turn ended. {currentPlayer}'s turn begins.");
        }
        
        // Notify about new turn starting
        OnTurnStart?.Invoke(currentPlayer);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
    }
    
    /// <summary>
    /// End the current turn early (called when X key is pressed)
    /// </summary>
    public void EndTurnEarly()
    {
        if (!gameStarted)
            return;
            
        if (debugTurns)
        {
            Debug.Log($"TurnManager: {currentPlayer} ended their turn early ({currentMoveCount}/{maxMovesPerTurn} moves used)");
        }
        
        EndTurn();
    }
    
    /// <summary>
    /// Reset the game to initial state
    /// </summary>
    public void ResetGame()
    {
        currentPlayer = startingPlayer;
        currentMoveCount = 0;
        gameStarted = true;
        
        if (debugTurns)
        {
            Debug.Log($"TurnManager: Game reset. {currentPlayer}'s turn begins.");
        }
        
        OnTurnStart?.Invoke(currentPlayer);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
    }
    
    /// <summary>
    /// Get turn information as a formatted string
    /// </summary>
    public string GetTurnInfo()
    {
        if (!gameStarted)
            return "Game not started";
            
        return $"{currentPlayer} - Move {currentMoveCount + 1}/{maxMovesPerTurn}";
    }
    
    /// <summary>
    /// Set maximum moves per turn (for game configuration)
    /// </summary>
    public void SetMaxMovesPerTurn(int maxMoves)
    {
        maxMovesPerTurn = Mathf.Max(1, maxMoves);
        OnMoveCountChanged?.Invoke(currentPlayer, currentMoveCount, maxMovesPerTurn);
    }
    
    /// <summary>
    /// Enable or disable debug logging
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        debugTurns = enabled;
    }
} 