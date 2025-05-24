# SelectionInfoUI Setup Guide

## Quick Setup (Automated) - Using Existing Panel

### Method 1: Using Existing SelectionPanel

1. **Prepare Your Existing Panel:**
   - Make sure you have a SelectionPanel already created in your Canvas
   - The panel can be empty or have existing children (they will be cleared)

2. **Create the Setup GameObject:**
   - Create an empty GameObject in your scene
   - Name it "UISetup"
   - Add the `SelectionUISetup` component

3. **Configure for Existing Panel:**
   - **Use Existing Panel**: ✓ (checked)
   - **Existing Selection Panel**: Drag your SelectionPanel from the Canvas
   - **Target Canvas**: Will be auto-detected from the panel
   - Panel Size and Position are ignored when using existing panel

4. **Run Setup:**
   - Right-click on the SelectionUISetup component → "Setup Selection UI"
   - The existing panel will be populated with:
     - Unit Name text (top)
     - Health Bar (slider)
     - Health Text showing "35/35" format
     - Attack and defence stats side by side

5. **Clean Up:**
   - Delete the UISetup GameObject after setup

### Method 2: Auto-Setup on Game Start

1. **Automatic Creation:**
   - Add `SelectionUISetup` to any GameObject
   - Check "Use Existing Panel" and assign your SelectionPanel
   - Check "Create UI On Start"
   - Run the game - UI will be setup automatically

---

## What Gets Created

The new setup creates these UI elements in your existing panel:

### Layout Structure:
```
SelectionPanel
├── UnitNameText          ("Unit Name" - 18pt, Bold)
├── HealthBar            (Slider with fill and background)
├── HealthText           ("35/35" format - 12pt)
├── AttackText           ("ATK: 15" - 14pt, left side)
└── DefenseText          ("DEF: 10" - 14pt, right side)
```

### Health Bar Features:
- **Visual Health Bar**: Shows health percentage as a filled slider
- **Color Coding**: 
  - Green (>60% health)
  - Yellow (30-60% health) 
  - Red (<30% health)
- **Numeric Display**: "Current/Max" format (e.g., "35/50")
- **Non-Interactive**: Players cannot adjust the slider

### Stats Display:
- **Attack**: Shows unit's attack value
- **defence**: Shows unit's defence value
- **Side-by-Side Layout**: Both stats on same row to save space

---

## Manual Setup (If Needed)

If you prefer manual setup, here's what to create in your existing SelectionPanel:

### 1. Unit Name Text
```
Type: TextMeshPro - Text (UI)
Name: "UnitNameText"
Position: Top of panel
Font: 18pt, Bold
Color: White
```

### 2. Health Bar
```
Type: UI → Slider
Name: "HealthBar"
Position: Below unit name
Size: Full width, ~20px height
Settings:
- Interactable: Off
- Transition: None
- Min Value: 0, Max Value: 1
- Background: Dark gray
- Fill: Green color, Horizontal fill
```

### 3. Health Text
```
Type: TextMeshPro - Text (UI)
Name: "HealthText"
Position: Below health bar
Font: 12pt, Normal
Color: White (will change based on health)
Text: "Health: --/--"
```

### 4. Attack Text
```
Type: TextMeshPro - Text (UI)
Name: "AttackText"
Position: Bottom left
Font: 14pt, Normal
Width: Half panel width
Text: "ATK: --"
```

### 5. defence Text
```
Type: TextMeshPro - Text (UI)
Name: "DefenseText"
Position: Bottom right
Font: 14pt, Normal
Width: Half panel width
Text: "DEF: --"
```

### 6. Add SelectionInfoUI Component
- Add the SelectionInfoUI component to your SelectionPanel
- Assign all the created UI elements to their respective fields

---

## Health Bar Color Customization

You can customize health bar colors in the SelectionInfoUI component:

### Inspector Settings:
- **Health Color High**: Color when health > 60% (default: Green)
- **Health Color Mid**: Color when health 30-60% (default: Yellow)
- **Health Color Low**: Color when health < 30% (default: Red)
- **Mid Health Threshold**: Percentage for yellow color (default: 0.6)
- **Low Health Threshold**: Percentage for red color (default: 0.3)

### Code Customization:
```csharp
// Get the SelectionInfoUI component
SelectionInfoUI uiComponent = selectionPanel.GetComponent<SelectionInfoUI>();

// Set custom colors
uiComponent.SetHealthBarColors(
    Color.cyan,    // High health
    Color.orange,  // Mid health  
    Color.magenta  // Low health
);
```

---

## Testing Your Setup

### Verification Checklist:

1. **Panel Setup:**
   - [ ] Existing SelectionPanel is properly configured
   - [ ] All UI elements are created and positioned correctly
   - [ ] SelectionInfoUI component is attached with references assigned

2. **Health Bar Test:**
   - [ ] Health bar fills correctly based on unit health
   - [ ] Health bar color changes with health percentage
   - [ ] Health text shows "current/max" format
   - [ ] Health text color matches health bar color

3. **Stats Display:**
   - [ ] Attack value displays correctly
   - [ ] defence value displays correctly
   - [ ] Both stats fit side by side

4. **Functionality:**
   - [ ] UI hides when no unit selected
   - [ ] UI appears when unit is selected
   - [ ] UI updates in real-time if health changes
   - [ ] Right-click clears selection

### Common Issues:

**Health Bar Not Filling:**
- Check that health bar's fillRect is properly assigned
- Verify unit's health and maxHealth values are valid
- Make sure health bar min/max values are 0 and 1

**Colors Not Changing:**
- Check health percentage calculation
- Verify fillRect has an Image component
- Ensure color thresholds are set correctly

**Stats Not Showing:**
- Verify unit has valid attack/defence values
- Check that text components are properly assigned
- Look for null reference errors in console

---

## Integration with Damage System

To make the health bar update when units take damage:

### Option 1: Event-Based Updates
```csharp
// In your Unit/Creature class, add an event
public System.Action<Unit> OnHealthChanged;

// In TakeDamage method:
public void TakeDamage(int damage)
{
    // ... existing damage logic ...
    OnHealthChanged?.Invoke(this);
}
```

### Option 2: Automatic Updates
The current system updates health every frame when a unit is selected, so damage changes will be reflected immediately without additional code.

---

## Input Controls

The selection system supports:

### Mouse Controls:
- **Left Click**: Select unit
- **Left Click + Ctrl**: Multi-select (if enabled)
- **Left Click on Empty Space**: Clear selection
- **Right Click**: Clear selection

### Keyboard Controls:
- **Escape**: Clear selection
- **Ctrl** (Hold): Multi-select modifier

---

## Performance Notes

The system is optimized for turn-based games:
- Health updates only occur when unit is selected
- Color changes are calculated efficiently
- No unnecessary UI updates when no selection

For real-time games with frequently changing health, consider limiting update frequency to avoid performance issues. 