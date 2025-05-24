# Arrow Key Movement System

## Overview
The Arrow Key Movement System allows players to move selected units using the arrow keys on their keyboard. This feature is designed for turn-based gameplay where precise, tile-by-tile movement is required.

## How It Works

### Basic Usage
1. **Select a Unit**: Click on any unit to select it (handled by the existing UnitSelectionManager)
2. **Move with Arrow Keys**: Use the arrow keys to move the selected unit one tile at a time:
   - **↑ (Up Arrow)**: Move one tile north (positive Y direction)
   - **↓ (Down Arrow)**: Move one tile south (negative Y direction)
   - **← (Left Arrow)**: Move one tile west (negative X direction)
   - **→ (Right Arrow)**: Move one tile east (positive X direction)

### Features
- **Input Cooldown**: Prevents rapid-fire movement by enforcing a 0.2-second cooldown between moves
- **Collision Detection**: Units cannot move to occupied tiles or invalid positions
- **Grid-Based Movement**: Movement is constrained to the isometric grid system
- **Visual Feedback**: Movement is immediately reflected in the game world

## Technical Implementation

### Core Components
- **UnitMovementController**: Main singleton that handles input and movement logic
- **UnitSelectionManager**: Existing system that tracks which units are selected
- **Unit**: Contains grid position and movement validation methods
- **IsometricGroundManager**: Provides grid coordinate system and tile validation

### Input Handling
The system uses Unity's legacy Input System for compatibility:
```csharp
// Example: Check for up arrow key press
if (Input.GetKeyDown(KeyCode.UpArrow))
{
    direction = new Vector2Int(0, 1);
    inputDetected = true;
}
```

### Movement Validation
Before moving, the system checks:
1. **Grid Bounds**: Target position is within the grid
2. **Tile Walkability**: Target tile allows unit placement
3. **Occupancy**: No other unit is already at the target position

## Configuration Options

### Inspector Settings (UnitMovementController)
- **Enable Arrow Key Movement**: Toggle the entire system on/off
- **Input Cooldown**: Time between allowed movements (default: 0.2 seconds)
- **Debug Movement**: Enable console logging for movement events

### Runtime Control
```csharp
// Get the movement controller instance
UnitMovementController controller = UnitMovementController.Instance;

// Disable movement temporarily
controller.SetMovementEnabled(false);

// Change input cooldown
controller.SetInputCooldown(0.1f);

// Manually move selected unit
controller.TryMoveSelectedUnit(new Vector2Int(1, 0)); // Move right
```

## Integration

### Setup
1. The `UnitMovementController` is automatically created as a singleton
2. It's initialized through the `GameManager` on game start
3. No manual setup is required - it works with existing selection and grid systems

### Events and Callbacks
The movement system integrates with existing systems:
- Uses `UnitSelectionManager.Instance` to get the currently selected unit
- Calls `Unit.CanMoveTo()` for movement validation
- Uses `Unit.SetGridPosition()` to perform the actual movement

## Debugging

### Console Output
When debug mode is enabled, you'll see logs like:
```
UnitMovementController: Attempting to move unit from (2, 3) to (2, 4)
UnitMovementController: Successfully moved unit to (2, 4)
```

### Common Issues
1. **Unit Not Moving**: Check if a unit is selected and the target tile is valid
2. **Movement Too Fast**: Adjust the input cooldown value
3. **Wrong Direction**: Verify grid coordinate system (Y+ is "up" in grid space)

## Future Enhancements
- Support for diagonal movement
- Animation smoothing between tiles
- Turn-based movement restrictions
- Multiple unit movement
- Customizable key bindings 