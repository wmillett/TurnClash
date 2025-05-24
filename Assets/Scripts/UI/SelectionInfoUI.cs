using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;

namespace TurnClash.UI
{
    public class SelectionInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        
        [Header("Health Bar Settings")]
        [SerializeField] private Color healthColorHigh = Color.green;
        [SerializeField] private Color healthColorMid = Color.yellow;
        [SerializeField] private Color healthColorLow = Color.red;
        [SerializeField] private float midHealthThreshold = 0.6f;
        [SerializeField] private float lowHealthThreshold = 0.3f;
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenNoSelection = true;
        [SerializeField] private bool debugMode = true;
        
        private UnitSelectable currentSelectedUnit;
        
        private void Start()
        {
            if (debugMode)
            {
                Debug.Log("SelectionInfoUI: Starting up...");
                LogUIReferences();
            }
            
            // Subscribe to selection events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected += OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
                if (debugMode) Debug.Log("SelectionInfoUI: Subscribed to selection events");
            }
            else
            {
                if (debugMode) Debug.LogWarning("SelectionInfoUI: UnitSelectionManager.Instance is null!");
            }
            
            // Initially hide the panel
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                if (debugMode) Debug.Log("SelectionInfoUI: Panel hidden on start");
            }
        }
        
        private void LogUIReferences()
        {
            Debug.Log($"SelectionInfoUI UI References:");
            Debug.Log($"  - selectionPanel: {(selectionPanel != null ? "✓" : "✗")}");
            Debug.Log($"  - unitNameText: {(unitNameText != null ? "✓" : "✗")}");
            Debug.Log($"  - healthText: {(healthText != null ? "✓" : "✗")}");
            Debug.Log($"  - healthBar: {(healthBar != null ? "✓" : "✗")}");
            Debug.Log($"  - attackText: {(attackText != null ? "✓" : "✗")}");
            Debug.Log($"  - defenseText: {(defenseText != null ? "✓" : "✗")}");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected -= OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared -= OnSelectionCleared;
            }
        }
        
        private void Update()
        {
            // Update UI if we have a selected unit
            if (currentSelectedUnit != null)
            {
                UpdateUnitInfo();
            }
        }
        
        private void OnUnitSelected(UnitSelectable selectedUnit)
        {
            if (debugMode)
                Debug.Log($"SelectionInfoUI: Unit selected - {selectedUnit?.name}");
            
            // For single selection, show the selected unit
            // For multiple selection, show the first/last selected unit
            currentSelectedUnit = selectedUnit;
            
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                if (debugMode) Debug.Log("SelectionInfoUI: Panel shown");
            }
            
            // Force immediate update
            UpdateUnitInfo();
        }
        
        private void OnSelectionCleared()
        {
            if (debugMode)
                Debug.Log("SelectionInfoUI: Selection cleared");
            
            currentSelectedUnit = null;
            
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                if (debugMode) Debug.Log("SelectionInfoUI: Panel hidden");
            }
            else
            {
                ClearUnitInfo();
            }
        }
        
        private void UpdateUnitInfo()
        {
            if (currentSelectedUnit == null) 
            {
                if (debugMode) Debug.LogWarning("SelectionInfoUI: currentSelectedUnit is null");
                return;
            }
            
            Unit unit = currentSelectedUnit.GetUnit();
            if (unit == null) 
            {
                if (debugMode) Debug.LogWarning("SelectionInfoUI: Unit component is null");
                return;
            }
            
            if (debugMode)
            {
                Debug.Log($"SelectionInfoUI: Updating UI for {unit.UnitName} - HP:{unit.health}/{unit.maxHealth}, ATK:{unit.attack}, DEF:{unit.defense}");
            }
            
            // Update unit name - use the UnitName property instead of GameObject name
            if (unitNameText != null)
            {
                string displayName = !string.IsNullOrEmpty(unit.UnitName) ? unit.UnitName : "Unknown Unit";
                unitNameText.text = displayName;
                if (debugMode) Debug.Log($"SelectionInfoUI: Set unit name to '{displayName}'");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: unitNameText is null!");
            }
            
            // Update health info
            UpdateHealthDisplay(unit);
            
            // Update attack stat
            if (attackText != null)
            {
                attackText.text = $"ATK: {unit.attack}";
                if (debugMode) Debug.Log($"SelectionInfoUI: Set attack to 'ATK: {unit.attack}'");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: attackText is null!");
            }
            
            // Update defense stat
            if (defenseText != null)
            {
                defenseText.text = $"DEF: {unit.defense}";
                if (debugMode) Debug.Log($"SelectionInfoUI: Set defense to 'DEF: {unit.defense}'");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: defenseText is null!");
            }
        }
        
        private void UpdateHealthDisplay(Unit unit)
        {
            // Update health text with current/max format
            if (healthText != null)
            {
                healthText.text = $"{unit.health}/{unit.maxHealth}";
                
                // Color code health text
                float healthPercentage = (float)unit.health / unit.maxHealth;
                if (healthPercentage > midHealthThreshold)
                    healthText.color = healthColorHigh;
                else if (healthPercentage > lowHealthThreshold)
                    healthText.color = healthColorMid;
                else
                    healthText.color = healthColorLow;
                    
                if (debugMode) Debug.Log($"SelectionInfoUI: Set health text to '{unit.health}/{unit.maxHealth}' with {healthPercentage:P0} health");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: healthText is null!");
            }
            
            // Update health bar
            if (healthBar != null)
            {
                float healthPercentage = (float)unit.health / unit.maxHealth;
                healthBar.value = healthPercentage;
                
                // Update health bar color
                Image fillImage = healthBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (healthPercentage > midHealthThreshold)
                        fillImage.color = healthColorHigh;
                    else if (healthPercentage > lowHealthThreshold)
                        fillImage.color = healthColorMid;
                    else
                        fillImage.color = healthColorLow;
                }
                else if (debugMode)
                {
                    Debug.LogWarning("SelectionInfoUI: healthBar fillImage is null!");
                }
                
                if (debugMode) Debug.Log($"SelectionInfoUI: Set health bar to {healthPercentage:P0}");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: healthBar is null!");
            }
        }
        
        private void ClearUnitInfo()
        {
            if (debugMode) Debug.Log("SelectionInfoUI: Clearing unit info");
            
            if (unitNameText != null)
                unitNameText.text = "No Unit Selected";
                
            if (healthText != null)
            {
                healthText.text = "--/--";
                healthText.color = Color.white;
            }
            
            if (healthBar != null)
            {
                healthBar.value = 0f;
                Image fillImage = healthBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.gray;
                }
            }
            
            if (attackText != null)
            {
                attackText.text = "ATK: --";
            }
            
            if (defenseText != null)
            {
                defenseText.text = "DEF: --";
            }
        }
        
        // Public methods for manual control
        public void ShowSelectionInfo(bool show)
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(show);
            }
        }
        
        public void SetHideWhenNoSelection(bool hide)
        {
            hideWhenNoSelection = hide;
        }
        
        // Public method to manually update health bar colors
        public void SetHealthBarColors(Color high, Color mid, Color low)
        {
            healthColorHigh = high;
            healthColorMid = mid;
            healthColorLow = low;
        }
        
        // Manual test method to force UI update
        [ContextMenu("Force Update UI")]
        public void ForceUpdateUI()
        {
            if (currentSelectedUnit != null)
            {
                Debug.Log("SelectionInfoUI: Forcing UI update...");
                UpdateUnitInfo();
            }
            else
            {
                Debug.Log("SelectionInfoUI: No unit selected to update");
            }
        }
    }
} 