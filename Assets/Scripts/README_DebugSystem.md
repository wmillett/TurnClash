# TurnClash Debug Management System

A centralized debug management system for the TurnClash Unity project, inspired by tools like [Unity Define Inspector](https://github.com/haydenjameslee/unity-define-inspector) and [Script Define Window](https://github.com/becksebenius/script-define-window).

## Overview

This system allows you to easily toggle different debug features across the entire project from a single inspector interface, using Unity's scripting define symbols for optimal performance and code cleanliness.

## Features

- **Centralized Control**: Manage all debug settings from one place
- **Performance Optimized**: Uses conditional compilation to remove debug code from builds
- **Category-Based**: Organized debug features by system (UI, Combat, Movement, etc.)
- **Easy Integration**: Simple to add to existing scripts
- **Runtime Control**: Option for immediate runtime changes (with recompilation)
- **Visual Inspector**: Beautiful Unity Editor window with color-coded status

## Debug Categories

### Main Categories
- **UI Debugging**: Selection UI, tooltips, and other UI systems
- **Turn System Debugging**: Turn management and player actions
- **Combat Debugging**: Basic combat logging and events
- **Movement Debugging**: Unit movement and movement preview
- **Unit Debugging**: Unit actions, defending, and stat changes
- **Visual Effects Debugging**: Visual effects and highlighting
- **Hover Tooltip Debugging**: Hover tooltip system
- **Codex Debugging**: Codex system
- **Performance Debugging**: Performance monitoring and optimization

### Detailed Options
- **Detailed Combat Logging**: Comprehensive combat logging with damage calculations
- **Combat Statistics**: Combat statistics tracking and display
- **Victory Panel Debugging**: Victory panel and game over states

## How to Use

### 1. Open the Debug Manager

- Go to **Window → TurnClash → Debug Manager**
- Or select a DebugSettings asset and click "Open Debug Manager Window"

### 2. Configure Debug Settings

1. Toggle the debug categories you want to enable
2. Click **"Apply Settings"** to set the scripting define symbols
3. Unity will recompile with the new settings

### 3. Quick Actions

- **Apply Settings**: Apply current settings and recompile
- **Reset All**: Reset all debug settings to default (disabled)
- **Enable All Debug**: Enable all debug features
- **Essential Debug Only**: Enable only essential debugging (Combat + Victory Panel)

## Integration Guide

### Method 1: Conditional Compilation (Recommended)

This is the most performance-efficient method as debug code is completely removed from builds:

```csharp
using TurnClash.Debug;

public class YourScript : MonoBehaviour
{
    private void SomeMethod()
    {
        // This code only exists when DEBUG_UI is defined
#if DEBUG_UI
        Debug.Log("UI operation completed");
#endif

        // Multi-level debugging
#if DEBUG_COMBAT
        Debug.Log("Basic combat info");
        
#if DEBUG_DETAILED_COMBAT
        Debug.Log("Detailed combat calculations");
#endif
#endif
    }
}
```

### Method 2: Runtime Checks

Use this when you need dynamic control without recompilation:

```csharp
using TurnClash.Debug;

public class YourScript : MonoBehaviour
{
    private void Start()
    {
        var debugManager = DebugManager.Instance;
        if (debugManager?.UIDebugging == true)
        {
            Debug.Log("UI debugging is enabled");
        }
    }
}
```

### Available Debug Defines

- `DEBUG_UI`
- `DEBUG_TURNS`
- `DEBUG_COMBAT`
- `DEBUG_MOVEMENT`
- `DEBUG_UNITS`
- `DEBUG_VISUAL_EFFECTS`
- `DEBUG_HOVER_TOOLTIP`
- `DEBUG_CODEX`
- `DEBUG_PERFORMANCE`
- `DEBUG_VICTORY_PANEL`
- `DEBUG_DETAILED_COMBAT`
- `DEBUG_COMBAT_STATS`

## Converting Existing Debug Code

### Before (Old System)
```csharp
[SerializeField] private bool debugMode = false;

private void SomeMethod()
{
    if (debugMode)
    {
        Debug.Log("Debug message");
    }
}
```

### After (New System)
```csharp
// Remove the SerializeField boolean

private void SomeMethod()
{
#if DEBUG_UI  // Use appropriate category
    Debug.Log("Debug message");
#endif
}
```

## Best Practices

### 1. Choose Appropriate Categories
- Use `DEBUG_UI` for UI-related debugging
- Use `DEBUG_COMBAT` for combat system debugging  
- Use `DEBUG_PERFORMANCE` for performance monitoring
- Use `DEBUG_DETAILED_COMBAT` for verbose combat logs

### 2. Performance Considerations
- Prefer conditional compilation (`#if`) over runtime checks
- Use runtime checks only when you need dynamic toggling
- Avoid expensive operations inside debug blocks

### 3. Code Organization
```csharp
public class ExampleScript : MonoBehaviour
{
    private void Update()
    {
        DoWork();
        
#if DEBUG_PERFORMANCE
        // Performance monitoring code here
#endif
    }
    
    private void DoWork()
    {
        // Main functionality
        
#if DEBUG_UI
        Debug.Log("Work completed");
#endif
    }
}
```

### 4. Category-Specific Logging
```csharp
private void LogByCategory(string category, string message)
{
    switch (category.ToUpper())
    {
        case "UI":
#if DEBUG_UI
            Debug.Log($"[UI] {message}");
#endif
            break;
            
        case "COMBAT":
#if DEBUG_COMBAT
            Debug.Log($"[COMBAT] {message}");
#endif
            break;
    }
}
```

## File Structure

```
Assets/Scripts/
├── DebugManager.cs              # Main debug settings ScriptableObject
├── Editor/
│   └── DebugManagerEditor.cs    # Editor window and inspector
├── DebugExample.cs              # Example implementation
└── README_DebugSystem.md        # This file
```

## Creating the Debug Settings Asset

1. Right-click in Project window
2. Create → TurnClash → Debug Settings
3. Name it "DebugSettings" and place in `Assets/Resources/`
4. Configure your preferred default settings

## Scripting Define Symbols

The system automatically manages Unity's scripting define symbols. You can view current symbols in:
- **Edit → Project Settings → Player → Configuration → Scripting Define Symbols**
- **Debug Manager Window → Current Scripting Define Symbols**

## Troubleshooting

### Debug Settings Not Found
- Ensure DebugSettings.asset exists in `Assets/Resources/`
- Use the Debug Manager window to create one automatically

### Changes Not Taking Effect
- Click "Apply Settings" in the Debug Manager window
- Wait for Unity to recompile
- Check that the correct define symbols are set

### Performance Issues
- Avoid expensive operations in debug blocks
- Use conditional compilation instead of runtime checks
- Monitor frame rate with DEBUG_PERFORMANCE enabled

## Examples

See `DebugExample.cs` for comprehensive usage examples, including:
- Basic conditional compilation
- Runtime debug checking
- Performance monitoring
- Category-specific logging
- Testing all debug categories

## Migration from Old Debug System

1. Remove `[SerializeField] private bool debug*` fields
2. Replace `if (debugVariable)` with `#if DEBUG_CATEGORY`
3. Add appropriate using statements
4. Test with Debug Manager window

This debug system provides professional-level debugging capabilities while maintaining clean, performant code that's easy to manage across the entire project. 