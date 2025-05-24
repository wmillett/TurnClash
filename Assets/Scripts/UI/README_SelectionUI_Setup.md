# SelectionInfoUI Setup Guide

## Quick Setup (Automated)

### Method 1: Using SelectionUISetup Helper

1. **Create the Setup GameObject:**
   - Create an empty GameObject in your scene
   - Name it "UISetup"
   - Add the `SelectionUISetup` component

2. **Configure and Run:**
   - In the inspector, you can adjust:
     - **Panel Size**: Default (300, 150)
     - **Panel Position**: Default (-10, 10) from bottom-left
     - **Target Canvas**: Leave empty to auto-create
   - Right-click on the component → "Create Selection UI"
   - The UI will be automatically created and configured!

3. **Clean Up:**
   - Delete the UISetup GameObject after creation (it's no longer needed)

### Method 2: Using createUIOnStart

1. **Automatic Creation:**
   - Add `SelectionUISetup` to any GameObject
   - Check "Create UI On Start"
   - Run the game - UI will be created automatically

---

## Manual Setup (Step by Step)

### Step 1: Create Canvas Structure

1. **Main Canvas:**
   ```
   Right-click Hierarchy → UI → Canvas
   Name: "GameUI"
   Canvas Scaler: Scale With Screen Size
   Reference Resolution: 1920x1080
   ```

2. **Selection Panel:**
   ```
   Right-click Canvas → UI → Panel
   Name: "SelectionPanel"
   Position: Bottom-left corner
   Size: 300x150 (adjust as needed)
   Background: Semi-transparent black (0,0,0,0.7)
   ```

### Step 2: Add Text Elements

Create these as children of SelectionPanel:

1. **UnitNameText:**
   ```
   UI → Text - TextMeshPro
   Text: "Unit Name"
   Font Size: 18, Bold
   Color: White
   Position: Top of panel
   ```

2. **HealthText:**
   ```
   UI → Text - TextMeshPro  
   Text: "Health: --/--"
   Font Size: 14, Normal
   Color: White (will change based on health)
   ```

3. **PlayerText:**
   ```
   UI → Text - TextMeshPro
   Text: "Player: --"
   Font Size: 14, Normal
   Color: White (will change based on player)
   ```

4. **PositionText:**
   ```
   UI → Text - TextMeshPro
   Text: "Position: (--, --)"
   Font Size: 14, Normal
   Color: White
   ```

### Step 3: Configure SelectionInfoUI Component

1. **Add Component:**
   - Select the SelectionPanel
   - Add Component → Scripts → SelectionInfoUI

2. **Assign References:**
   - **Selection Panel**: The SelectionPanel itself
   - **Unit Name Text**: UnitNameText component
   - **Health Text**: HealthText component  
   - **Player Text**: PlayerText component
   - **Position Text**: PositionText component
   - **Hide When No Selection**: ✓ (checked)

---

## Layout Suggestions

### Bottom-Left Panel (Recommended)
```
Position: Anchored to bottom-left
Offset: 10px from edges
Size: 300x150
Good for: Most games, doesn't block main view
```

### Top-Right Panel
```
Position: Anchored to top-right
Offset: 10px from edges  
Size: 280x120
Good for: Strategy games, out of action area
```

### Bottom-Center Panel
```
Position: Anchored to bottom-center
Offset: 0px from bottom, centered
Size: 400x100
Good for: RPGs, character-focused games
```

---

## Styling Options

### Color Schemes

**Dark Theme (Default):**
```
Panel Background: (0, 0, 0, 0.7)
Text Color: White
Health Colors: Green/Yellow/Red
Player1 Color: Blue
Player2 Color: Red
```

**Light Theme:**
```
Panel Background: (1, 1, 1, 0.8)
Text Color: Black
Health Colors: Dark Green/Orange/Dark Red
Player1 Color: Dark Blue
Player2 Color: Dark Red
```

### Font Sizes
```
Unit Name: 16-20pt (Bold)
Info Text: 12-16pt (Normal)
Small Details: 10-12pt (Normal)
```

---

## Input Controls Update

The selection system now supports:

### Mouse Controls:
- **Left Click**: Select unit
- **Left Click + Ctrl**: Multi-select (if enabled)
- **Left Click on Empty Space**: Clear selection
- **Right Click**: Clear selection (NEW!)

### Keyboard Controls:
- **Escape**: Clear selection
- **Ctrl** (Hold): Multi-select modifier

---

## Testing Your Setup

### Verification Checklist:

1. **UI Creation:**
   - [ ] Canvas exists with proper scaling
   - [ ] SelectionPanel is positioned correctly
   - [ ] All text elements are present and visible

2. **Component Setup:**
   - [ ] SelectionInfoUI component attached to panel
   - [ ] All text references assigned in inspector
   - [ ] Hide When No Selection is checked

3. **Functionality Test:**
   - [ ] UI is hidden on game start
   - [ ] UI appears when unit is selected
   - [ ] UI shows correct unit information
   - [ ] UI hides when selection is cleared
   - [ ] Right-click clears selection

### Common Issues:

**UI Not Appearing:**
- Check that Canvas has GraphicRaycaster
- Verify SelectionInfoUI references are assigned
- Ensure units have UnitSelectable components

**Text Not Updating:**
- Check console for errors
- Verify unit has proper Unit component
- Make sure text references aren't null

**Right-Click Not Working:**
- Check that UnitSelectionManager is active
- Verify debug logs show right-click detection
- Ensure no UI elements are blocking clicks

---

## Customization Examples

### Adding Attack/Defense Display:
```csharp
// In SelectionInfoUI, add new text field
[SerializeField] private TextMeshProUGUI statsText;

// In UpdateUnitInfo():
if (statsText != null)
{
    statsText.text = $"ATK: {unit.attack} | DEF: {unit.defense}";
}
```

### Adding Unit Portrait:
```csharp
// Add Image field for unit portrait
[SerializeField] private Image unitPortrait;

// Assign sprite based on unit type
if (unitPortrait != null && unit.unitSprite != null)
{
    unitPortrait.sprite = unit.unitSprite;
}
```

### Animated Panel Transitions:
```csharp
// Use DOTween or similar for smooth show/hide
selectionPanel.transform.DOScale(Vector3.one, 0.3f);
```

---

## Integration with Turn System

For turn-based games, consider:

1. **Show Available Actions:**
   - Display movement/attack options when unit is selected
   - Gray out unavailable actions

2. **Turn Indicators:**
   - Show whose turn it is
   - Highlight if selected unit can act

3. **Action Feedback:**
   - Update UI after unit performs actions
   - Show cooldowns or charges remaining 