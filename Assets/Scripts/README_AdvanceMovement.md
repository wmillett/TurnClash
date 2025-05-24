# TurnClash Advance Movement System

## Overview
The Advance Movement System automatically moves an attacking unit to the position of a defeated enemy unit at no additional movement cost. This rewards successful attacks and creates more dynamic, aggressive gameplay.

## Features

### Core Functionality
- **Automatic Advancement**: When a unit kills an enemy, it automatically moves to the victim's position
- **No Movement Cost**: Advance movements don't consume additional movement points
- **Smooth Animation**: Optional animated movement using Vector3.MoveTowards
- **Configurable**: Can be enabled/disabled per unit or globally
- **Event System**: Tracks and reports advance movements

### Visual Features
- **Movement Animation**: Smooth transition to new position
- **Configurable Speed**: Adjustable animation speed
- **Instant Movement**: Option to disable animation for instant movement
- **Combat Feedback**: Clear logging of advance movements

## Implementation

### Unit.cs Enhancements
**Location**: `Assets/Scripts/Units/Unit.cs`

**New Features**:
- `moveToVictimPosition` toggle for enabling/disabling the feature
- `AdvanceToPosition()` method for moving without movement cost
- Animation system using `Vector3.MoveTowards`
- `OnUnitAdvancedToPosition` event for tracking

**Inspector Settings**:
```csharp
[Header("Movement Animation")]
[SerializeField] private bool animateMovement = true;           // Enable animation
[SerializeField] private float movementAnimationSpeed = 10f;   // Animation speed
[SerializeField] private bool moveToVictimPosition = true;     // Enable advance feature
```

### Enhanced Attack System
The `AttackUnit()` method now:
1. Stores victim's position before attack
2. Checks if target was alive before attack
3. Applies damage to target
4. If target dies, triggers advance movement
5. Fires advance event for tracking

### CombatManager Integration
**Location**: `Assets/Scripts/CombatManager.cs`

**New Features**:
- Tracks advance movement statistics
- Logs advance movements
- Fires events for other systems
- Updated combat statistics display

**New Statistics**:
- `AdvanceMovements` count
- Enhanced combat statistics reporting

## Usage Guide

### Basic Setup
The advance movement system is automatically enabled for all units by default. No additional setup required.

### Configuring Per Unit
```csharp
// In Unit inspector
moveToVictimPosition = true;  // Enable advance movement
animateMovement = true;       // Enable smooth animation
movementAnimationSpeed = 10f; // Animation speed
```

### Disabling for Specific Units
```csharp
// To disable advance movement for a unit
unit.moveToVictimPosition = false;
```

### Animation Settings
```csharp
// Fast movement
movementAnimationSpeed = 20f;

// Instant movement (no animation)
animateMovement = false;
```

## Technical Details

### Movement Point Economy
- **Attack**: Consumes 1 movement point (normal cost)
- **Advance**: Costs 0 movement points (reward for successful kill)
- **Net Cost**: 1 movement point for attack + advance combination

### Animation System
Uses Unity's [`Vector3.MoveTowards`](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/Vector3.MoveTowards.html) for smooth movement:

```csharp
private void Update()
{
    if (isAnimating && animateMovement)
    {
        float maxDistance = movementAnimationSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, animationTargetPos, maxDistance);
    }
}
```

### Event Flow
1. **Attack Initiated**: `AttackUnit()` called
2. **Position Stored**: Victim's position saved
3. **Damage Applied**: Normal combat damage calculation
4. **Death Check**: Verify if victim died from this attack
5. **Advance Triggered**: `AdvanceToPosition()` called if kill occurred
6. **Position Updated**: Grid position tracking updated
7. **Animation Started**: Visual movement begins (if enabled)
8. **Event Fired**: `OnUnitAdvancedToPosition` event triggered

## Events and Integration

### New Events
```csharp
// In Unit.cs
public System.Action<Unit, Vector2Int> OnUnitAdvancedToPosition;

// In CombatManager.cs
public System.Action<Unit, Vector2Int> OnUnitAdvanced;
```

### Event Subscription
The CombatManager automatically subscribes to unit advance events:
```csharp
unit.OnUnitAdvancedToPosition += OnUnitAdvancedToPosition;
```

### Statistics Tracking
- **Advance Count**: Total number of advance movements
- **Combat Integration**: Advance movements included in combat statistics
- **Per-Player Tracking**: Can determine which player benefits more from advances

## Gameplay Impact

### Strategic Benefits
- **Aggressive Play**: Rewards attacking over defensive positioning
- **Board Control**: Successful attacks gain territory
- **Chain Attacks**: Advanced units may be in position for follow-up attacks
- **Risk/Reward**: Encourages calculated aggression

### Tactical Considerations
- **Positioning**: Units ending up in enemy territory after advances
- **Vulnerability**: Advanced units may be isolated
- **Planning**: Need to consider post-advance positioning
- **Multi-Kill Potential**: Advanced units may threaten additional enemies

## Configuration Examples

### Aggressive Setup
```csharp
moveToVictimPosition = true;   // Always advance
animateMovement = true;        // Show dramatic movement
movementAnimationSpeed = 15f;  // Fast, exciting animation
```

### Conservative Setup
```csharp
moveToVictimPosition = false;  // No automatic advances
// Or selectively enable for specific unit types
```

### Performance Setup
```csharp
moveToVictimPosition = true;   // Keep feature enabled
animateMovement = false;       // Instant movement for performance
```

## Testing and Debugging

### Debug Output
When `debugCombat = true`, the system logs:
- Attack damage calculations
- Kill confirmations
- Advance movement triggers
- Final position updates

### Testing Scenarios
1. **Basic Advance**: Unit A attacks and kills Unit B, advances to B's position
2. **Failed Kill**: Unit A attacks Unit B but doesn't kill, no advance occurs
3. **Animation Test**: Verify smooth movement animation
4. **Statistics Test**: Check advance count in combat statistics
5. **Chain Attack**: Advanced unit attacks another enemy

### Verification
```csharp
// Check advance movement statistics
CombatManager.Instance.AdvanceMovements; // Total advance count

// Verify unit position after advance
unit.GetGridPosition(); // Should match victim's former position
```

## Future Enhancements

Potential improvements:
- **Unit Type Restrictions**: Some units don't advance (e.g., ranged units)
- **Terrain Considerations**: Can't advance to certain tile types
- **Advance Distance Limits**: Multi-tile advances for powerful units
- **Conditional Advances**: Based on unit health, stats, or abilities
- **Animation Varieties**: Different animation styles per unit type

## Performance Considerations

### Optimization Notes
- Animation system uses `Time.deltaTime` for frame-rate independence
- Position updates happen immediately for game logic
- Visual animation is separate from position tracking
- Event system allows efficient notification without polling

### Memory Usage
- Minimal additional memory overhead
- Animation state variables only active during movement
- Events use standard C# Action delegates

## Related Components
- `Unit.cs` - Core advance movement implementation
- `CombatManager.cs` - Statistics and event tracking
- `UnitMovementController.cs` - Normal movement system (unchanged)
- `TurnManager.cs` - Movement point management
- `IsometricGroundManager.cs` - Position validation

## References
- [Unity Vector3.MoveTowards Documentation](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/Vector3.MoveTowards.html)
- [Unity Time.deltaTime Documentation](https://docs.unity3d.com/ScriptReference/Time-deltaTime.html)
- Turn-based strategy game design principles 