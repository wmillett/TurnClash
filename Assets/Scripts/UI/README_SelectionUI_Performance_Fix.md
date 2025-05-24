# SelectionInfoUI Performance Fix

## Issue Summary
The `SelectionInfoUI` component was causing performance problems and console spam due to several issues:

1. **Infinite Update Loop**: The `Update()` method was calling `UpdateUnitInfo()` every frame
2. **Division by Zero**: Health percentage calculation failed when `maxHealth = 0`, producing NaN values
3. **Debug Spam**: Debug logging was happening every frame, flooding the console
4. **Timing Issues**: UI tried to access unit stats before they were fully initialized

## Fixes Applied

### 1. Removed Infinite Update Loop
- **Problem**: `Update()` method called `UpdateUnitInfo()` every frame when a unit was selected
- **Solution**: Removed the `Update()` method entirely
- **Result**: UI now only updates when selection changes, not every frame

### 2. Added Health Validation
- **Problem**: Division by zero when `unit.maxHealth = 0` caused NaN values
- **Solution**: Added validation to check `maxHealth > 0` before calculations
- **Code**: 
```csharp
if (unit.maxHealth <= 0)
{
    Debug.LogWarning($"Unit {unit.UnitName} has invalid maxHealth: {unit.maxHealth}");
    return;
}
```

### 3. Reduced Debug Spam
- **Problem**: Debug logs printed every frame, flooding console
- **Solution**: 
  - Changed default `debugMode = false`
  - Added `!float.IsNaN()` checks before logging
  - Reduced frequency of debug messages
- **Result**: Much cleaner console output

### 4. Fixed Initialization Timing
- **Problem**: UI accessed unit stats before spawner finished initialization
- **Solution**: Added coroutine delay when unit is selected
- **Code**:
```csharp
private System.Collections.IEnumerator UpdateUIAfterFrame()
{
    yield return null; // Wait one frame
    UpdateUnitInfo();
}
```

### 5. Added Manual Refresh Method
- **Problem**: No way to update UI when unit stats change during gameplay
- **Solution**: Added `RefreshCurrentSelection()` method
- **Usage**: Call when unit takes damage, levels up, etc.

## Performance Improvements

### Before Fix:
- UI updated 60+ times per second when unit selected
- Console flooded with debug messages
- Potential NaN errors causing visual glitches
- Frame rate impact from excessive updates

### After Fix:
- UI updates only when selection changes
- Clean console output
- Robust error handling
- No performance impact from UI updates

## Usage Notes

### For Developers:
```csharp
// To refresh UI when unit stats change:
SelectionInfoUI uiComponent = FindObjectOfType<SelectionInfoUI>();
uiComponent.RefreshCurrentSelection();

// To enable debug mode temporarily:
uiComponent.debugMode = true;
```

### For Turn-Based Combat:
When implementing combat where unit health changes:
```csharp
void OnUnitTakesDamage(Unit unit)
{
    unit.TakeDamage(damage);
    
    // Refresh UI if this unit is currently selected
    var selectionUI = FindObjectOfType<SelectionInfoUI>();
    if (selectionUI != null)
    {
        selectionUI.RefreshCurrentSelection();
    }
}
```

## Technical Details

### Event-Driven Updates
The UI now relies entirely on the selection system's events:
- `OnUnitSelected`: Shows panel and updates info
- `OnSelectionCleared`: Hides panel or clears info
- Manual refresh when stats change during gameplay

### Error Handling
- Validates `maxHealth > 0` before calculations
- Uses `Mathf.Clamp01()` to ensure valid health percentages
- Graceful fallbacks for missing components

### Memory Optimization
- No more per-frame allocations from `Update()`
- Reduced string allocations from debug logging
- Event-based updates only when needed

## Migration Guide

If you have existing code that relied on real-time updates:

### Old (Problematic):
```csharp
// UI automatically updated every frame - caused performance issues
```

### New (Efficient):
```csharp
// Manually refresh when stats actually change
selectionUI.RefreshCurrentSelection();
```

This ensures the UI stays responsive while maintaining optimal performance. 