using UnityEngine;
using TurnClash.Units;

/// <summary>
/// Manages combat events and provides combat-related functionality
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private bool enableCombatLogging = true;
    [SerializeField] private bool enableDetailedCombatLog = true;
    [SerializeField] private bool enableCombatStatistics = true;
    
    [Header("Combat Statistics")]
    [SerializeField] private int totalCombats = 0;
    [SerializeField] private int player1Attacks = 0;
    [SerializeField] private int player2Attacks = 0;
    [SerializeField] private int totalDamageDealt = 0;
    [SerializeField] private int unitsDestroyed = 0;
    [SerializeField] private int advanceMovements = 0; // New statistic for advance movements
    
    // Singleton instance
    private static CombatManager instance;
    private static bool isApplicationQuitting = false;
    
    public static CombatManager Instance
    {
        get
        {
            if (isApplicationQuitting)
                return null;
                
            if (instance == null)
            {
                instance = FindObjectOfType<CombatManager>();
                if (instance == null && !isApplicationQuitting)
                {
                    GameObject go = new GameObject("CombatManager");
                    instance = go.AddComponent<CombatManager>();
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to subscribe to
    public System.Action<Unit, Unit, int> OnCombatOccurred; // attacker, defender, damage
    public System.Action<Unit, Unit> OnUnitKilled; // attacker, victim
    public System.Action<Unit.Player> OnPlayerEliminationCheck; // player that might be eliminated
    public System.Action<Unit, Vector2Int> OnUnitAdvanced; // New event for advance movements
    
    // Combat statistics properties
    public int TotalCombats => totalCombats;
    public int Player1Attacks => player1Attacks;
    public int Player2Attacks => player2Attacks;
    public int TotalDamageDealt => totalDamageDealt;
    public int UnitsDestroyed => unitsDestroyed;
    public int AdvanceMovements => advanceMovements; // New property
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Debug.Log("CombatManager: Destroying duplicate instance");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        isApplicationQuitting = false;
        Debug.Log("CombatManager: Instance created and initialized");
    }
    
    private void Start()
    {
        // Ensure the flag is cleared on scene start
        isApplicationQuitting = false;
        Debug.Log("CombatManager: Start() called, ready for operations");
        
        // Subscribe to all existing units' combat events
        SubscribeToExistingUnits();
        
        if (enableCombatLogging)
        {
            Debug.Log("CombatManager: Initialized and ready to track combat");
        }
    }
    
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        if (enableCombatLogging)
            Debug.Log("CombatManager: Application quitting, preventing new instance creation");
    }
    
    private void SubscribeToExistingUnits()
    {
        if (isApplicationQuitting) return;
        
        // Find all existing units and subscribe to their events
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            SubscribeToUnit(unit);
        }
    }
    
    /// <summary>
    /// Subscribe to a unit's combat events
    /// </summary>
    public void SubscribeToUnit(Unit unit)
    {
        if (unit == null || isApplicationQuitting) return;
        
        unit.OnAttackPerformed += OnUnitAttackPerformed;
        unit.OnUnitDestroyed += OnUnitDestroyed;
        unit.OnUnitAdvancedToPosition += OnUnitAdvancedToPosition;
    }
    
    /// <summary>
    /// Unsubscribe from a unit's combat events
    /// </summary>
    public void UnsubscribeFromUnit(Unit unit)
    {
        if (unit == null || isApplicationQuitting) return;
        
        unit.OnAttackPerformed -= OnUnitAttackPerformed;
        unit.OnUnitDestroyed -= OnUnitDestroyed;
        unit.OnUnitAdvancedToPosition -= OnUnitAdvancedToPosition;
    }
    
    private void OnUnitAttackPerformed(Unit attacker, Unit defender, int damage)
    {
        if (isApplicationQuitting) return;
        
        // Update statistics
        if (enableCombatStatistics)
        {
            totalCombats++;
            totalDamageDealt += damage;
            
            if (attacker.player == Unit.Player.Player1)
                player1Attacks++;
            else
                player2Attacks++;
        }
        
        // Combat logging
        if (enableCombatLogging)
        {
            if (enableDetailedCombatLog)
            {
                Debug.Log($"=== COMBAT ===");
                Debug.Log($"Attacker: {attacker.UnitName} ({attacker.player}) [ATK: {attacker.attack}]");
                Debug.Log($"Defender: {defender.UnitName} ({defender.player}) [DEF: {defender.defence}]");
                Debug.Log($"Damage Dealt: {damage}");
                Debug.Log($"Defender Health: {defender.health}/{defender.maxHealth}");
                Debug.Log($"=============");
            }
            else
            {
                Debug.Log($"COMBAT: {attacker.UnitName} dealt {damage} damage to {defender.UnitName}");
            }
        }
        
        // Check if defender was killed
        if (!defender.IsAlive())
        {
            HandleUnitKilled(attacker, defender);
        }
        
        // Fire event for other systems
        OnCombatOccurred?.Invoke(attacker, defender, damage);
    }
    
    private void OnUnitDestroyed(Unit unit)
    {
        if (isApplicationQuitting) return;
        
        // Update statistics
        if (enableCombatStatistics)
        {
            unitsDestroyed++;
        }
        
        // Unsubscribe from the destroyed unit
        UnsubscribeFromUnit(unit);
        
        // Check if this player has any remaining units
        CheckForPlayerElimination(unit.player);
        
        if (enableCombatLogging)
        {
            Debug.Log($"Unit destroyed: {unit.UnitName} ({unit.player})");
        }
    }
    
    private void OnUnitAdvancedToPosition(Unit unit, Vector2Int newPosition)
    {
        if (isApplicationQuitting) return;
        
        // Update statistics
        if (enableCombatStatistics)
        {
            advanceMovements++;
        }
        
        if (enableCombatLogging)
        {
            Debug.Log($"⚔️ {unit.UnitName} ({unit.player}) advances to position {newPosition} after successful kill!");
        }
        
        // Fire event for other systems
        OnUnitAdvanced?.Invoke(unit, newPosition);
    }
    
    private void HandleUnitKilled(Unit attacker, Unit victim)
    {
        if (isApplicationQuitting) return;
        
        if (enableCombatLogging)
        {
            Debug.Log($"💀 {victim.UnitName} ({victim.player}) was killed by {attacker.UnitName} ({attacker.player})!");
        }
        
        // Fire event
        OnUnitKilled?.Invoke(attacker, victim);
    }
    
    private void CheckForPlayerElimination(Unit.Player player)
    {
        if (isApplicationQuitting) return;
        
        // Count remaining units for this player
        Unit[] allUnits = FindObjectsOfType<Unit>();
        int remainingUnits = 0;
        
        foreach (Unit unit in allUnits)
        {
            if (unit.player == player && unit.IsAlive())
            {
                remainingUnits++;
            }
        }
        
        if (remainingUnits == 0)
        {
            if (enableCombatLogging)
            {
                Debug.Log($"🚨 {player} has been eliminated! No remaining units.");
            }
            
            OnPlayerEliminationCheck?.Invoke(player);
        }
        else if (enableCombatLogging)
        {
            Debug.Log($"{player} has {remainingUnits} units remaining");
        }
    }
    
    /// <summary>
    /// Get combat statistics as a formatted string
    /// </summary>
    public string GetCombatStatistics()
    {
        return $"Combat Statistics:\n" +
               $"Total Combats: {totalCombats}\n" +
               $"Player 1 Attacks: {player1Attacks}\n" +
               $"Player 2 Attacks: {player2Attacks}\n" +
               $"Total Damage: {totalDamageDealt}\n" +
               $"Units Destroyed: {unitsDestroyed}\n" +
               $"Advance Movements: {advanceMovements}";
    }
    
    /// <summary>
    /// Reset all combat statistics
    /// </summary>
    public void ResetStatistics()
    {
        if (isApplicationQuitting) return;
        
        totalCombats = 0;
        player1Attacks = 0;
        player2Attacks = 0;
        totalDamageDealt = 0;
        unitsDestroyed = 0;
        advanceMovements = 0;
        
        if (enableCombatLogging)
            Debug.Log("Combat statistics reset");
    }
    
    /// <summary>
    /// Get the count of remaining units for a player
    /// </summary>
    public int GetRemainingUnitsForPlayer(Unit.Player player)
    {
        if (isApplicationQuitting) return 0;
        
        Unit[] allUnits = FindObjectsOfType<Unit>();
        int count = 0;
        
        foreach (Unit unit in allUnits)
        {
            if (unit.player == player && unit.IsAlive())
            {
                count++;
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// Check if a player has been eliminated
    /// </summary>
    public bool IsPlayerEliminated(Unit.Player player)
    {
        return GetRemainingUnitsForPlayer(player) == 0;
    }
    
    /// <summary>
    /// Enable or disable combat logging
    /// </summary>
    public void SetCombatLogging(bool enabled)
    {
        enableCombatLogging = enabled;
    }
    
    /// <summary>
    /// Enable or disable detailed combat logging
    /// </summary>
    public void SetDetailedLogging(bool enabled)
    {
        enableDetailedCombatLog = enabled;
    }
    
    private void OnDestroy()
    {
        if (enableCombatLogging)
            Debug.Log("CombatManager: OnDestroy called");
            
        // Clear all event subscriptions
        OnCombatOccurred = null;
        OnUnitKilled = null;
        OnPlayerEliminationCheck = null;
        OnUnitAdvanced = null;
        
        // Clear singleton reference
        if (instance == this)
        {
            instance = null;
        }
        
        // Only set quitting flag if we're actually quitting the application
        // Don't set it during scene changes or manual destroy
        if (enableCombatLogging)
            Debug.Log("CombatManager: Cleanup complete, instance cleared");
    }
} 