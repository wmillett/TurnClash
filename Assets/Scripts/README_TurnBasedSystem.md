# Turn-Based System

## Overview
The Turn-Based System implements a complete turn management solution for TurnClash, featuring player alternation, move limits, and seamless integration with the existing arrow key movement system. The system is built using event-driven architecture inspired by proven Unity turn-based frameworks.

## How It Works

### Game Flow
1. **Game Start**: Player1 begins with the first turn
2. **Player Turn**: Current player can move their units up to 4 times
3. **Turn End**: Turn automatically ends after 4 moves or when X key is pressed
4. **Player Switch**: Turn passes to the other player
5. **Repeat**: Process continues alternating between players

### Turn Rules
- **Move Limit**: Each player gets 4 moves per turn (configurable)
- **Early End**: Press **X key** to end your turn early
- **Unit Ownership**: Only units belonging to the current player can move
- **Free Selection**: Any unit can be selected regardless of whose turn it is
- **Movement Validation**: System checks player ownership before allowing movement

## Key Components

### TurnManager
The core singleton that manages all turn-based logic:

**Key Properties:**
- `CurrentPlayer`: Which player's turn it is currently
- `CurrentMoveCount`: Number of moves used this turn
- `RemainingMoves`: Moves left in current turn
- `MaxMovesPerTurn`: Total moves allowed per turn (default: 4)

**Key Methods:**
- `CanPlayerMove(player)`: Check if a player can make moves
- `CanUnitMove(unit)`: Check if a specific unit can move
- `UseMove(player)`: Consume a move for the current player
- `EndTurn()`: Switch to the next player
- `EndTurnEarly()`: End turn before using all moves

### UnitMovementController Integration
Enhanced to work with the turn system:

**New Features:**
- **Turn Validation**: Checks if unit belongs to current player before movement
- **Move Consumption**: Automatically uses a turn move when unit moves
- **Turn Respect Toggle**: Can disable turn restrictions for testing (`respectTurnSystem`)

### TurnDisplayUI
Optional UI component for showing turn information:

**Display Elements:**
- Current player's turn
- Moves remaining this turn
- Control instructions
- Color-coded player indicators

## Events System

The turn system uses events for loose coupling and extensibility:

### Available Events
```csharp
// Turn lifecycle events
TurnManager.OnTurnStart += (player) => { /* Handle turn start */ };
TurnManager.OnTurnEnd += (player) => { /* Handle turn end */ };

// Move tracking events
TurnManager.OnMoveUsed += (player, remainingMoves) => { /* Handle move usage */ };
TurnManager.OnMoveCountChanged += (player, current, max) => { /* Handle move count changes */ };
```

### Event Usage Examples
```csharp
// Subscribe to turn events in your scripts
private void Start()
{
    TurnManager.Instance.OnTurnStart += OnPlayerTurnStart;
    TurnManager.Instance.OnMoveUsed += OnPlayerMoveUsed;
}

private void OnPlayerTurnStart(Unit.Player player)
{
    Debug.Log($"{player}'s turn started!");
    // Enable/disable UI elements, play sounds, etc.
}

private void OnPlayerMoveUsed(Unit.Player player, int remainingMoves)
{
    if (remainingMoves == 0)
    {
        Debug.Log($"{player} has used all their moves!");
    }
}
```

## Configuration

### Inspector Settings

**TurnManager:**
- **Max Moves Per Turn**: Number of moves each player gets (default: 4)
- **Starting Player**: Which player goes first (default: Player1)
- **Debug Turns**: Enable console logging for turn events

**UnitMovementController:**
- **Respect Turn System**: Whether to enforce turn-based movement rules
- **Debug Movement**: Enable movement-specific logging

### Runtime Configuration
```csharp
// Access turn manager
TurnManager turnManager = TurnManager.Instance;

// Change max moves per turn
turnManager.SetMaxMovesPerTurn(6);

// Reset game to initial state
turnManager.ResetGame();

// Disable turn system temporarily
UnitMovementController.Instance.SetTurnSystemRespect(false);
```

## Integration Guide

### Existing System Compatibility
The turn system is designed to work seamlessly with existing TurnClash systems:

- **Unit Selection**: Uses existing `UnitSelectionManager` - no changes needed
- **Grid Movement**: Uses existing `Unit.CanMoveTo()` and `SetGridPosition()` methods
- **Input Handling**: Integrates with existing arrow key movement system
- **Player System**: Uses existing `Unit.Player` enum for player identification

### Adding Turn UI
1. Create a Canvas with TextMeshPro components
2. Add `TurnDisplayUI` component to a GameObject
3. Assign UI text elements in the inspector
4. The system will automatically connect and update the display

### Custom Turn Logic
Extend the system for custom game rules:

```csharp
// Custom turn ending conditions
public class CustomTurnRules : MonoBehaviour
{
    private void Start()
    {
        TurnManager.Instance.OnMoveUsed += CheckCustomEndConditions;
    }
    
    private void CheckCustomEndConditions(Unit.Player player, int remainingMoves)
    {
        // Example: End turn if specific condition met
        if (SomeCustomCondition())
        {
            TurnManager.Instance.EndTurn();
        }
    }
}
```

## Controls

| Input | Action |
|-------|--------|
| **Arrow Keys** | Move selected unit (if it belongs to current player) |
| **X Key** | End current turn early |
| **Mouse Click** | Select units (any player's units can be selected) |
| **Escape** | Clear selection |

## Debugging

### Console Output
With debug mode enabled, you'll see comprehensive logging:

```
TurnManager: Game started! Player1's turn begins.
UnitMovementController: Attempting to move Player1 unit from (2, 3) to (2, 4)
TurnManager: Player1 used move 1/4
Player1 has 3 moves remaining this turn
=== Player1's Turn Ended ===
=== Player2's Turn Started ===
UnitMovementController: Cannot move Player1 unit - it's Player2's turn
```

### Common Issues
1. **Unit Won't Move**: 
   - Check if it's the correct player's turn
   - Verify the unit belongs to the current player
   - Ensure moves are remaining this turn

2. **Turn Won't End**: 
   - Check if X key binding conflicts with other systems
   - Verify TurnManager is properly initialized

3. **Wrong Player Turn**: 
   - Check unit's `player` field is set correctly
   - Verify TurnManager starting player setting

## Architecture Benefits

### Event-Driven Design
- **Loose Coupling**: Systems communicate through events, not direct references
- **Extensibility**: Easy to add new systems that react to turn changes
- **Maintainability**: Clear separation of concerns between turn logic and game systems

### Singleton Pattern
- **Global Access**: Turn state available anywhere in the codebase
- **Single Source of Truth**: One authoritative turn manager
- **Automatic Initialization**: Self-creating singletons require no manual setup

### Integration Philosophy
Following patterns from established Unity turn-based systems:
- State-driven architecture (inspired by [Unity Turn-Based Toolkit](https://github.com/DeanAviv/unity-turn-base-toolkit))
- Event-driven messaging (inspired by [Unity3DTurnManager](https://github.com/samjudge/Unity3DTurnManager))
- Clean component separation for maintainability

## Future Enhancements
- **Turn Timer**: Automatic turn ending after time limit
- **Action Points**: Different moves cost different amounts
- **Turn History**: Track and replay previous turns
- **AI Integration**: AI players using the same turn system
- **Network Multiplayer**: Synchronize turns across network
- **Save/Load**: Persist turn state between game sessions 