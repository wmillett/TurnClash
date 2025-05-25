# Movement Preview System

## Overview
The Movement Preview system adds visual feedback for unit movement by showing glowing tiles around selected units that indicate where they can move or attack.

## Features

### ✨ **Visual Movement Indicators**
- **Green glow**: Tiles where the unit can move
- **Red glow**: Tiles where the unit can attack enemies
- **Automatic preview**: Shows immediately when selecting a friendly unit
- **Smart filtering**: Only shows for units that can actually move

### 🎮 **Click-to-Move**
- Click any highlighted tile to move the unit there
- Works for both movement and combat
- Integrates with turn system (consumes moves)
- Prevents invalid moves automatically

### 🎯 **Smart Behavior**
- **Turn-based integration**: Only shows for current player's units
- **Move validation**: Respects remaining moves per turn
- **Game state aware**: Disabled when game is over
- **Arrow key integration**: Arrow keys disabled when preview is active

## Components

### MovementPreview (Singleton)
- **Location**: `Assets/Scripts/Units/MovementPreview.cs`
- **Auto-initialized**: Created by GameManager
- **Subscribes to**: UnitSelectionManager events

### MovementTileClick
- **Dynamic component**: Added/removed from tiles as needed
- **Handles**: Mouse click detection on highlighted tiles
- **Cleanup**: Automatically removed when preview ends

## Usage

### Automatic Usage
The system works automatically:
1. Select a friendly unit (during your turn)
2. See glowing tiles around the unit
3. Click any glowing tile to move there
4. Preview disappears after movement

### Manual Control
```csharp
// Enable/disable movement preview
MovementPreview.Instance.SetMovementPreviewEnabled(false);

// Change movement range (default: 1 tile)
MovementPreview.Instance.SetMovementRange(2);
```

## Configuration

### In Inspector (MovementPreview component):
- **Enable Movement Preview**: Toggle the entire system
- **Movement Tile Color**: Color for movement tiles (default: green)
- **Attack Tile Color**: Color for attack tiles (default: red)
- **Movement Range**: How many tiles away to show (default: 1)
- **Debug Preview**: Enable debug logging

## Integration Points

### With Existing Systems:
- **UnitSelectionManager**: Listens for selection events
- **TurnManager**: Checks current player and remaining moves
- **CombatManager**: Checks for game over state
- **UnitMovementController**: Disables arrow keys when preview active
- **Unit.CanMoveTo()**: Uses existing movement validation

### Event Flow:
1. Unit selected → MovementPreview.OnUnitSelected()
2. Show valid tiles → Create highlighting and click handlers
3. Tile clicked → MovementPreview.OnTileClicked()
4. Move unit → Unit.TryMoveToPosition()
5. Hide preview → Clean up highlighting

## Troubleshooting

### Preview Not Showing
- Check if it's the current player's turn
- Verify unit has remaining moves
- Ensure MovementPreview is enabled
- Check if game is over

### Clicks Not Working
- Verify tiles have colliders
- Check MovementTileClick components are added
- Ensure no UI elements are blocking clicks

### Performance Issues
- Movement range affects number of highlighted tiles
- Consider reducing range for large grids
- Debug mode adds logging overhead

## Technical Notes

### Material Management
- Original materials are stored and restored
- New materials created for highlighting
- Automatic cleanup prevents memory leaks

### Collision Detection
- Uses OnMouseDown() for tile clicks
- Requires colliders on tiles
- Automatically adds colliders if missing

### Singleton Pattern
- Follows same pattern as other managers
- Auto-creates instance when needed
- Proper cleanup on application quit

## Future Enhancements

### Possible Additions:
- **Multi-tile movement**: Path planning for longer moves
- **Movement costs**: Different terrain types
- **Area of effect**: Show spell/ability ranges
- **Path visualization**: Show movement path
- **Undo movement**: Allow taking back moves 