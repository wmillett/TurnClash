# Unit Selection System Guide

## Overview

The unit selection system allows players to click on units to select them with a visual glow effect. This system is designed following RTS game patterns and provides both visual feedback and programmatic access to selected units.

## Components

### 1. UnitSelectable
- **Purpose**: Makes individual units clickable and handles glow effects
- **Requirements**: Unit component, Collider component
- **Features**: Emission-based glow, material caching, event system

### 2. UnitSelectionManager
- **Purpose**: Global selection state management
- **Pattern**: Singleton
- **Features**: Single/multi-selection, player filtering, input handling

### 3. SelectionInfoUI (Optional)
- **Purpose**: Displays information about selected units
- **Features**: Health display, player color coding, position info

## Automatic Setup

The system is designed to work automatically:

1. **UnitSpawner** automatically adds `UnitSelectable` components to spawned units
2. **Colliders** are automatically added if missing
3. **UnitSelectionManager** auto-creates itself as a singleton

## Manual Setup

### For Unit Prefabs:

1. **Add Components:**
   ```
   - Unit (from Creature)
   - UnitSelectable
   - Collider (BoxCollider, SphereCollider, etc.)
   - MeshRenderer (for glow effect)
   ```

2. **Configure UnitSelectable:**
   - **Glow Color**: Default golden (1, 0.8, 0, 1)
   - **Glow Intensity**: Default 2.0
   - **Use Emission**: Keep enabled for glow effect

### For Scene Setup:

1. **UnitSelectionManager:**
   - Automatically created as singleton
   - Access via `UnitSelectionManager.Instance`
   - Configure in inspector if manually added

2. **SelectionInfoUI (Optional):**
   - Add to Canvas
   - Assign UI text references
   - Will automatically subscribe to selection events

## Usage Examples

### Basic Selection:
```csharp
// Get the selection manager
var selectionManager = UnitSelectionManager.Instance;

// Check if units are selected
if (selectionManager.HasSelection)
{
    var selectedUnit = selectionManager.FirstSelectedUnit;
    Debug.Log($"Selected: {selectedUnit.name}");
}
```

### Subscribe to Selection Events:
```csharp
void Start()
{
    UnitSelectionManager.Instance.OnUnitSelected += OnUnitSelected;
    UnitSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
}

void OnUnitSelected(UnitSelectable unit)
{
    Debug.Log($"Unit selected: {unit.GetUnit().name}");
}

void OnSelectionCleared()
{
    Debug.Log("Selection cleared");
}
```

### Select Units by Player:
```csharp
// Select all Player1 units
UnitSelectionManager.Instance.SelectUnitsOfPlayer(Creature.Player.Player1);

// Get selected units of specific player
var player1Units = UnitSelectionManager.Instance.GetSelectedUnitsOfPlayer(Creature.Player.Player1);
```

## Input Controls

### Mouse Controls:
- **Left Click**: Select unit
- **Left Click + Ctrl**: Multi-select (if enabled)
- **Left Click on Empty Space**: Clear selection

### Keyboard Controls:
- **Escape**: Clear selection
- **Ctrl** (Hold): Multi-select modifier

## Configuration Options

### UnitSelectionManager Settings:
- **Allow Multiple Selection**: Enable/disable multi-select
- **Multi Select Key**: Key for multi-selection (default: Left Ctrl)
- **Debug Selection**: Enable console logging

### UnitSelectable Settings:
- **Glow Color**: Selection highlight color
- **Glow Intensity**: Brightness of the glow
- **Use Emission**: Toggle glow effect on/off

## Turn-Based Game Considerations

For turn-based games like yours:
- **Single Selection**: Usually preferred (`allowMultipleSelection = false`)
- **Player Turn Validation**: Check if it's the unit's player's turn before allowing selection
- **Action Feedback**: Use selection events to show available actions

## Events Available

### UnitSelectionManager Events:
- `OnUnitSelected(UnitSelectable unit)`: When a unit is selected
- `OnUnitDeselected(UnitSelectable unit)`: When a unit is deselected  
- `OnSelectionChanged(List<UnitSelectable> units)`: When selection changes
- `OnSelectionCleared()`: When all selections are cleared

### UnitSelectable Events:
- `OnSelected(UnitSelectable unit)`: When this specific unit is selected
- `OnDeselected(UnitSelectable unit)`: When this specific unit is deselected

## Troubleshooting

### Units Not Clickable:
- Ensure units have colliders
- Check that colliders are not set as triggers
- Verify UnitSelectable component is attached

### Glow Effect Not Working:
- Check that materials support emission (`_EmissionColor` property)
- Ensure URP/HDRP materials are used if in those render pipelines
- Verify `Use Emission` is enabled in UnitSelectable

### Selection Manager Not Found:
- Manager auto-creates itself on first access
- Check for script compilation errors
- Ensure TurnClash.Units namespace is accessible

### Multiple Selection Not Working:
- Enable `Allow Multiple Selection` in UnitSelectionManager
- Check multi-select key configuration
- Verify input system compatibility

## Performance Considerations

- **Material Caching**: UnitSelectable caches materials to avoid repeated lookups
- **Event-Driven**: UI updates only when selection changes
- **Singleton Pattern**: Single manager instance prevents duplication
- **Collider Optimization**: Uses simple colliders for click detection

## Extension Ideas

- **Hover Effects**: Add highlighting when mouse hovers over units
- **Selection Boxes**: Visual boxes around selected units
- **Group Selection**: Drag to select multiple units
- **Animation**: Animated selection effects instead of static glow
- **Sound Effects**: Audio feedback for selection/deselection 