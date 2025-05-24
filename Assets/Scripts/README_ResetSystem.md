# TurnClash Reset System

## Overview
The Reset System allows players to quickly restart the game by pressing the 'R' key. This provides instant game restart functionality without needing to reload the scene manually.

## Features

### Core Functionality
- **R Key Reset**: Press 'R' to instantly restart the game
- **Scene Reload**: Complete game state reset by reloading the current scene
- **Configurable**: Reset key and behavior can be customized in inspector
- **Clean Shutdown**: Proper event unsubscription before scene reload

### UI Integration
- **Instructions Display**: Shows reset controls in game instructions
- **Game Over Integration**: Optional reset instructions during game over
- **Customizable Text**: Reset message can be customized
- **Visual Effects**: Optional flashing text effect for emphasis

## Components

### GameManager (Enhanced)
**Location**: `Assets/Scripts/GameManager.cs`

**New Features**:
- Input detection for reset key
- Scene reloading functionality
- Configurable reset settings
- Clean event unsubscription

**Inspector Settings**:
```csharp
[Header("Reset Settings")]
[SerializeField] private bool allowReset = true;           // Enable/disable reset
[SerializeField] private KeyCode resetKey = KeyCode.R;     // Reset key
[SerializeField] private bool showResetInstructions = true; // Show in controls
```

### ResetInstructionUI (New)
**Location**: `Assets/Scripts/UI/ResetInstructionUI.cs`

**Purpose**: Displays reset instructions to players, especially during game over

**Features**:
- Auto-detects game over events
- Configurable display settings
- Optional text flashing effect
- Can show permanently or only on game over

**Inspector Settings**:
```csharp
[Header("Display Settings")]
[SerializeField] private bool showOnGameOver = true;      // Show during game over
[SerializeField] private bool showPermanently = false;   // Always visible
[SerializeField] private bool autoFindGameManager = true;
[SerializeField] private bool autoFindCombatManager = true;

[Header("Text Settings")]
[SerializeField] private string resetMessage = "Press 'R' to Reset Game";
[SerializeField] private Color textColor = Color.white;
[SerializeField] private bool flashText = false;         // Flashing effect
[SerializeField] private float flashSpeed = 2f;
```

### TurnDisplayUI (Enhanced)
**Location**: `Assets/Scripts/UI/TurnDisplayUI.cs`

**Changes**: Updated instructions text to include reset functionality
- Old: `"Arrow Keys: Move Unit | X: End Turn"`
- New: `"Arrow Keys: Move Unit | X: End Turn | R: Reset Game"`

## Usage Guide

### Basic Setup
1. The reset system is automatically enabled when GameManager is present
2. Default reset key is 'R' (configurable in inspector)
3. Instructions are automatically shown in debug logs and UI

### Customizing Reset Key
```csharp
// In GameManager inspector
resetKey = KeyCode.F5;  // Change to F5
```

### Adding Reset Instructions to UI
1. Create a UI Text element in your Canvas
2. Add `ResetInstructionUI` component
3. Assign the text element to `resetInstructionText`
4. Configure display settings as needed

### Showing Reset on Game Over
```csharp
// ResetInstructionUI automatically detects game over if:
showOnGameOver = true;  // Enabled in inspector
```

### Permanent Reset Instructions
```csharp
// To always show reset instructions:
showPermanently = true;  // In ResetInstructionUI inspector
```

## Events and Integration

### GameManager Integration
- Listens for R key input in `Update()`
- Calls enhanced `ResetGame()` method
- Properly unsubscribes from events before scene reload

### Event Cleanup
Before scene reload, the system automatically:
1. Unsubscribes from TurnManager events
2. Unsubscribes from CombatManager events  
3. Clears unit selections
4. Logs reset action

### Scene Reload Process
```csharp
public void ResetGame()
{
    // 1. Unsubscribe from all events
    // 2. Clear selections
    // 3. Get current scene name
    // 4. Reload scene using SceneManager.LoadScene()
}
```

## Best Practices

### When to Use Reset
- During game over situations
- For quick testing during development
- When players want to restart without menu navigation
- After completing a match

### UI Placement
- Place reset instructions near other controls
- Consider showing only during game over to reduce UI clutter
- Use flashing effect sparingly to avoid distraction

### Testing Reset Functionality
1. Start a game
2. Make some moves/changes
3. Press 'R' key
4. Verify complete state reset

## Implementation Notes

### Scene Reload vs Manual Reset
The system uses `SceneManager.LoadScene()` instead of manually resetting individual systems because:
- **Complete Reset**: Ensures all objects return to initial state
- **Memory Management**: Properly cleans up all references
- **Simplicity**: Single call handles all reset logic
- **Reliability**: Prevents edge cases with partial resets

### Event Safety
Before scene reload:
```csharp
// Unsubscribe to prevent null reference errors
turnManager.OnTurnStart -= OnPlayerTurnStart;
combatManager.OnPlayerEliminationCheck -= OnPlayerEliminationCheck;
UnitSelectionManager.Instance.ClearSelection();
```

## Future Enhancements

Potential improvements:
- Confirmation dialog for accidental resets
- Reset counter/statistics
- Different reset types (soft vs hard reset)
- Save/load system integration
- Multiple scene support

## Related Components
- `GameManager.cs` - Core reset functionality
- `TurnManager.cs` - Provides ResetGame() method
- `CombatManager.cs` - Provides ResetStatistics() method
- `UnitSelectionManager.cs` - Selection clearing
- All UI components - Updated instructions

## References
- [Unity SceneManagement Documentation](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html)
- [Press R to Reset Tutorial](https://medium.com/@russsmith482/press-r-to-reset-a3b4e15e81f9)
- [Unity Input System](https://docs.unity3d.com/ScriptReference/Input.html) 