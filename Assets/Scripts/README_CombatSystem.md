# Combat System

## Overview
The Combat System adds tactical combat to TurnClash where units can attack enemy units by moving into their tiles. The system follows classic turn-based RPG mechanics with attack vs defense calculations, similar to those found in [proven turn-based systems](https://pavcreations.com/turn-based-battle-and-transition-from-a-game-world-unity/).

## How Combat Works

### Basic Combat Mechanics
- **Attack Trigger**: Moving into an enemy unit's tile triggers an attack
- **Damage Calculation**: `Damage = Attacker's Attack - Defender's Defense` (minimum 1 damage)
- **Turn Integration**: Combat actions consume moves from your turn allowance
- **Death Handling**: Units with 0 or less health are destroyed
- **Player Elimination**: A player loses when all their units are destroyed

### Combat Flow
1. **Select Unit**: Click to select one of your units
2. **Target Enemy**: Use arrow keys to move toward an enemy unit
3. **Attack Resolution**: Moving into the enemy tile triggers combat
4. **Damage Calculation**: System calculates and applies damage
5. **Victory Check**: Game checks for player elimination

## Combat Components

### 🎯 **Unit.cs Enhancements**
```csharp
// New combat methods
public void AttackUnit(Unit target)
public Unit GetEnemyAtPosition(Vector2Int targetPosition)
public bool TryMoveToPosition(Vector2Int targetPosition)
public bool WouldAttackEnemy(Vector2Int targetPosition)
public string GetCombatPreview(Unit target)
```

**Key Features:**
- **AttackUnit()**: Handles damage calculation and application
- **GetEnemyAtPosition()**: Detects enemy units at specific locations
- **TryMoveToPosition()**: Combined movement/combat action
- **Combat Events**: Fires events when attacks occur or units die

### ⚔️ **CombatManager.cs**
```csharp
// Combat tracking and statistics
public void SubscribeToUnit(Unit unit)
public string GetCombatStatistics()
public int GetRemainingUnitsForPlayer(Unit.Player player)
public bool IsPlayerEliminated(Unit.Player player)
```

**Key Features:**
- **Combat Logging**: Detailed combat information in console
- **Statistics Tracking**: Tracks attacks, damage, and eliminations
- **Event Management**: Centralized combat event handling
- **Player Elimination**: Automatically detects when a player loses

### 🎮 **UnitMovementController.cs Updates**
```csharp
// Enhanced movement with combat
[SerializeField] private bool enableCombat = true;
[SerializeField] private bool showCombatPreview = true;
```

**Key Features:**
- **Combat Integration**: Seamlessly handles both movement and combat
- **Combat Preview**: Shows damage calculation before attacking
- **Turn System Respect**: Combat actions consume turn moves
- **Debug Information**: Detailed logging of combat actions

## Combat Examples

### Example 1: Basic Attack
```
Scenario:
- Player1 unit (ATK: 15, DEF: 5) attacks Player2 unit (ATK: 10, DEF: 3)
- Player2 unit has 20/20 HP

Combat Calculation:
- Damage = 15 (P1 ATK) - 3 (P2 DEF) = 12 damage
- P2 unit health: 20 - 12 = 8/20 HP

Result: P2 unit survives with 8 HP
```

### Example 2: Unit Destruction
```
Scenario:
- Player1 unit (ATK: 20) attacks Player2 unit (DEF: 2, HP: 5)

Combat Calculation:
- Damage = 20 - 2 = 18 damage
- P2 unit health: 5 - 18 = -13 HP

Result: P2 unit is destroyed (eliminated from game)
```

### Example 3: High Defense
```
Scenario:
- Player1 unit (ATK: 10) attacks Player2 unit (DEF: 12)

Combat Calculation:
- Damage = 10 - 12 = -2, but minimum damage is 1
- P2 unit takes 1 damage

Result: Even high defense units take at least 1 damage
```

## Turn System Integration

### Movement + Combat Rules
- **Turn Consumption**: Each attack uses one move from your turn allowance
- **Player Restriction**: Only units belonging to the current player can attack
- **Move Limit**: Standard 4 moves per turn applies to both movement and combat
- **Early Turn End**: Press X to end turn early after combat

### Turn Flow with Combat
1. **Player's Turn Starts**: Can move/attack with their units
2. **Combat Actions**: Each attack consumes a move
3. **Move Tracking**: System tracks remaining moves
4. **Turn End**: Automatic after 4 moves or manual with X key
5. **Player Switch**: Turn passes to other player

## Combat Statistics

### Tracked Metrics
- **Total Combats**: Number of attacks performed
- **Player Attacks**: Individual attack counts per player
- **Total Damage**: Cumulative damage dealt
- **Units Destroyed**: Number of units eliminated
- **Remaining Units**: Live unit count per player

### Accessing Statistics
```csharp
// Get formatted statistics
string stats = CombatManager.Instance.GetCombatStatistics();

// Check player elimination
bool eliminated = CombatManager.Instance.IsPlayerEliminated(Unit.Player.Player1);

// Get remaining units
int remaining = CombatManager.Instance.GetRemainingUnitsForPlayer(Unit.Player.Player2);
```

## Configuration Options

### Unit Combat Settings
```csharp
[Header("Combat Stats")]
public int attack = 15;      // Damage dealt
public int defense = 5;      // Damage reduction
public int health = 100;     // Hit points
public int maxHealth = 100;  // Maximum health

[Header("Combat Settings")]
[SerializeField] private bool debugCombat = true;
```

### Movement Controller Settings
```csharp
[Header("Combat Settings")]
[SerializeField] private bool enableCombat = true;           // Enable/disable combat
[SerializeField] private bool showCombatPreview = true;      // Show damage preview
```

### Combat Manager Settings
```csharp
[Header("Combat Settings")]
[SerializeField] private bool enableCombatLogging = true;       // Enable combat logs
[SerializeField] private bool enableDetailedCombatLog = true;   // Detailed logging
[SerializeField] private bool enableCombatStatistics = true;    // Track statistics
```

## Combat Events

### Available Events
```csharp
// Unit-level events
public System.Action<Unit, Unit, int> OnAttackPerformed;  // attacker, defender, damage
public System.Action<Unit> OnUnitDestroyed;               // destroyed unit

// Combat Manager events
public System.Action<Unit, Unit, int> OnCombatOccurred;   // attacker, defender, damage
public System.Action<Unit, Unit> OnUnitKilled;            // attacker, victim
public System.Action<Unit.Player> OnPlayerEliminationCheck; // eliminated player
```

### Event Usage Example
```csharp
void Start()
{
    // Subscribe to combat events
    CombatManager.Instance.OnUnitKilled += OnUnitKilled;
    CombatManager.Instance.OnPlayerEliminationCheck += OnGameOver;
}

private void OnUnitKilled(Unit attacker, Unit victim)
{
    Debug.Log($"{victim.UnitName} was killed by {attacker.UnitName}!");
    // Add visual effects, sounds, etc.
}

private void OnGameOver(Unit.Player eliminatedPlayer)
{
    Debug.Log($"Game Over! {eliminatedPlayer} has been eliminated!");
    // Show victory screen, stop game, etc.
}
```

## Debugging and Logging

### Combat Logs
The system provides comprehensive logging:
```
=== COMBAT ===
Attacker: Warrior (Player1) [ATK: 15]
Defender: Knight (Player2) [DEF: 5]
Damage Dealt: 10
Defender Health: 15/25
=============
```

### Debug Commands
```csharp
// Enable/disable combat logging
CombatManager.Instance.SetCombatLogging(true);

// Enable/disable detailed logging
CombatManager.Instance.SetDetailedLogging(true);

// Get combat preview
string preview = selectedUnit.GetCombatPreview(targetUnit);
```

## Integration with UI

### TurnPanelController Integration
The combat system works seamlessly with the existing turn display:
- Shows current player's turn
- Updates move count after combat
- Displays turn information during combat

### Combat Feedback
- **Console Logging**: Detailed combat information
- **Debug Previews**: Damage calculations before attack
- **Statistics Display**: Real-time combat metrics
- **Event System**: Extensible for UI integration

## Future Enhancements

### Possible Additions
- **Range Attacks**: Units that can attack from distance
- **Special Abilities**: Unique unit powers and skills
- **Terrain Effects**: Different tile types affecting combat
- **Equipment System**: Items that modify stats
- **Experience/Leveling**: Units that grow stronger
- **Formation Bonuses**: Adjacent unit benefits

### Advanced Combat Features
- **Critical Hits**: Random damage multipliers
- **Status Effects**: Poison, stun, buffs, debuffs
- **Counter Attacks**: Defending units striking back
- **Opportunity Attacks**: Penalties for moving past enemies
- **Area of Effect**: Attacks hitting multiple targets

## Troubleshooting

### Common Issues

**Units Not Attacking:**
- Check that `enableCombat = true` in UnitMovementController
- Verify the target tile contains an enemy unit
- Ensure it's the attacking player's turn

**No Damage Dealt:**
- Verify attack > 0 on attacking unit
- Check that minimum damage (1) is being applied
- Confirm TakeDamage method is being called

**Turn System Not Working:**
- Ensure TurnManager is initialized
- Check that `respectTurnSystem = true` in movement controller
- Verify turn events are being fired

**Missing Combat Logs:**
- Enable `enableCombatLogging = true` in CombatManager
- Check console window for debug output
- Verify Debug.Log is not filtered out

## Best Practices

### Unit Design
- **Balanced Stats**: Ensure no unit is overpowered
- **Clear Roles**: Different units for different strategies
- **Visual Distinction**: Easy identification of unit types and players

### Combat Design
- **Risk vs Reward**: Attacking should have consequences
- **Strategic Depth**: Multiple viable strategies
- **Clear Feedback**: Players should understand what happened

### Performance
- **Event Cleanup**: Always unsubscribe from events
- **Efficient Queries**: Cache frequently accessed data
- **Minimal FindObjectOfType**: Use singletons and references

The combat system is now fully integrated with your turn-based movement system, providing tactical depth while maintaining the simplicity of the core mechanics! 