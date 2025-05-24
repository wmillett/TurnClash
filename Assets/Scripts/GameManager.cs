using UnityEngine;
using TurnClash.Units;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private IsometricGroundManager groundManager;
    [SerializeField] private UnitSpawner unitSpawner;
    [SerializeField] private UnitMovementController movementController;
    [SerializeField] private TurnManager turnManager;
    
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
    }
    
    private void Start()
    {
        // The ground manager will create the grid in its Start method
        // The unit spawner will spawn units in its Start method (with a frame delay)
        // The turn manager will initialize the turn system in its Start method
        
        Debug.Log("Game Manager initialized. Ground and units will be set up automatically.");
        Debug.Log("Arrow key movement is now available for selected units.");
        Debug.Log("Turn-based system is active - Player1 starts first!");
        Debug.Log("Controls: Arrow keys to move selected unit, X key to end turn early");
        
        // Subscribe to turn events for additional logging
        if (turnManager != null)
        {
            turnManager.OnTurnStart += OnPlayerTurnStart;
            turnManager.OnTurnEnd += OnPlayerTurnEnd;
            turnManager.OnMoveUsed += OnPlayerMoveUsed;
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
} 