# Selection System Issues & Fixes

## Issues Identified

### 1. UnitSelectionManager Cleanup Issue
**Problem**: UnitSelectionManager GameObject persists after scene closure, causing Unity warning about objects not being cleaned up.

**Root Cause**: The manager was using `DontDestroyOnLoad()` which prevented proper scene cleanup.

### 2. UI Values Not Updating
**Problem**: Attack, health, and defense values not displaying in SelectionPanel UI.

**Root Cause**: Missing or incorrectly assigned UI component references in SelectionInfoUI.

## Solutions Applied

### 1. Fixed UnitSelectionManager Cleanup

**Changes Made**:
- Removed `DontDestroyOnLoad()` from UnitSelectionManager.Awake()
- Enhanced OnDestroy() method with proper selection clearing
- Added null unit cleanup validation

**Code Changes** (in `UnitSelectionManager.cs`):
```csharp
private void Awake()
{
    // Singleton setup
    if (instance != null && instance != this)
    {
        Destroy(gameObject);
        return;
    }
    
    instance = this;
    // Removed DontDestroyOnLoad to allow proper scene cleanup
    // DontDestroyOnLoad(gameObject);
}

private void OnDestroy()
{
    if (debugSelection)
        Debug.Log("UnitSelectionManager: OnDestroy called");
        
    // Clear all selections before destroying
    ClearSelection(false); // Don't trigger events during cleanup
    
    if (instance == this)
    {
        instance = null;
    }
}
```

### 2. Fixed UI Update Issues

**Changes Made**:
- Added UI reference validation in SelectionInfoUI.Start()
- Enhanced debug logging to identify missing references
- Added comprehensive error reporting

**Code Changes** (in `SelectionInfoUI.cs`):
```csharp
private void ValidateUIReferences()
{
    bool allReferencesValid = true;
    
    if (selectionPanel == null)
    {
        Debug.LogError("SelectionInfoUI: selectionPanel is null! UI updates will fail.");
        allReferencesValid = false;
    }
    
    if (unitNameText == null)
    {
        Debug.LogError("SelectionInfoUI: unitNameText is null! Unit name won't update.");
        allReferencesValid = false;
    }
    
    // ... similar checks for all UI components
    
    if (allReferencesValid)
    {
        Debug.Log("SelectionInfoUI: All UI references are properly assigned ✓");
    }
    else
    {
        Debug.LogError("SelectionInfoUI: Some UI references are missing! Use SelectionUISetup to fix this.");
    }
}
```

## Diagnostic Tool: SelectionUIFixer

A new utility script `SelectionUIFixer.cs` has been created to help diagnose and automatically fix these issues.

### How to Use SelectionUIFixer:

1. **Add to Scene**:
   - Create empty GameObject
   - Add `SelectionUIFixer` component
   - Configure options in inspector

2. **Automatic Diagnosis**:
   - Runs automatically on Start if `runDiagnosticsOnStart = true`
   - Checks UnitSelectionManager, SelectionInfoUI, and Units
   - Reports all issues in console

3. **Manual Fixes**:
   - Right-click component → "Run Full Diagnostics"
   - Right-click component → "Test Selection Events"
   - Right-click component → "Force Update Current Selection"

### Inspector Options:
- **Run Diagnostics On Start**: Automatically check for issues when scene loads
- **Enable Debug Logs**: Show detailed diagnostic information
- **Auto Fix UI References**: Attempt to automatically fix missing UI references
- **Ensure Manager Cleanup**: Fix DontDestroyOnLoad issues

## Step-by-Step Fix Guide

### If UI Values Are Not Updating:

1. **Add SelectionUIFixer** to your scene
2. **Run diagnostics** (automatic on play or manual via context menu)
3. **Check console output** for missing UI references
4. **If references are missing**:
   - Find/add `SelectionUISetup` component to scene
   - Configure existing SelectionPanel in inspector
   - Click "Setup Selection UI" in context menu

### If Manager Cleanup Issues Persist:

1. **Check console** for "DontDestroyOnLoad" warnings
2. **Run SelectionUIFixer diagnostics**
3. **Enable "Ensure Manager Cleanup"** in SelectionUIFixer
4. **The fixer will automatically** move manager to scene hierarchy

## Manual UI Setup (If Automatic Fails)

### Using SelectionUISetup:

1. **Add SelectionUISetup component** to any GameObject
2. **Configure in inspector**:
   - Target Canvas: Your UI Canvas
   - Existing Selection Panel: Your SelectionPanel GameObject  
   - Use Existing Panel: ✓ (checked)
3. **Right-click component** → "Setup Selection UI"

### Manual Reference Assignment:

If automatic setup fails, manually assign references in SelectionInfoUI:
- **Selection Panel**: Your SelectionPanel GameObject
- **Unit Name Text**: TextMeshPro component for unit name
- **Health Text**: TextMeshPro component for health display
- **Health Bar**: Slider component for health bar
- **Attack Text**: TextMeshPro component for attack value
- **Defense Text**: TextMeshPro component for defense value

## Verification Steps

After applying fixes:

1. **Play the scene**
2. **Check console** for "All UI references are properly assigned ✓"
3. **Select a unit** by clicking on it
4. **Verify UI updates**:
   - Unit name appears in panel
   - Health bar shows unit health
   - Health text shows "X/Y" format
   - Attack shows "ATK: X"
   - Defense shows "DEF: X"
5. **Stop playing and reload scene**
6. **Check console** - should NOT see cleanup warnings

## Testing Commands

Use these console commands for testing:

```csharp
// Force UI update
FindObjectOfType<SelectionInfoUI>().ForceUpdateUI();

// Test selection events
FindObjectOfType<SelectionUIFixer>().TestSelectionEvents();

// Run full diagnostics
FindObjectOfType<SelectionUIFixer>().RunFullDiagnostics();
```

## Expected Console Output (Success)

When everything works correctly:
```
SelectionInfoUI: All UI references are properly assigned ✓
UnitSelectionManager: UnitSelectionManager found: UnitSelectionManager
SelectionInfoUI: Subscribed to selection events
UnitSelectionManager: Firing OnUnitSelected for Unit_Player1_1
SelectionInfoUI: Updating UI for UnitPlaceholder1 - HP:35/35, ATK:10, DEF:5
```

## Troubleshooting

### UI Still Not Updating:
- Ensure SelectionInfoUI has all references assigned
- Check that UnitSelectionManager events are firing
- Verify units have UnitSelectable components
- Make sure SelectionInfoUI is subscribed to events

### Manager Still Not Cleaning Up:
- Ensure DontDestroyOnLoad is removed
- Check that OnDestroy is being called
- Verify manager is child of scene objects, not DontDestroyOnLoad

### Units Not Selectable:
- Check units have Collider components
- Verify UnitSelectable components are attached
- Ensure colliders are not set as triggers
- Check that units are not on ignored layers

This comprehensive fix should resolve both the cleanup issue and UI update problems in your turn-based game selection system. 