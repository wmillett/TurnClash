using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;
using System.Collections;

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
        [SerializeField] private bool debugMode = false; // Changed to false to reduce console spam
        
        private UnitSelectable currentSelectedUnit;
        
        private void Awake()
        {
            Debug.Log("SelectionInfoUI: Awake() called");
            LogUIReferenceStatus("AWAKE");
        }
        
        private void Start()
        {
            Debug.Log("SelectionInfoUI: Start() called");
            LogUIReferenceStatus("START");
            
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
                Debug.Log("SelectionInfoUI: Successfully subscribed to selection events");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: UnitSelectionManager.Instance is null! Events not subscribed.");
            }
            
            // Initially hide the panel
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                Debug.Log($"SelectionInfoUI: Panel hidden on start (hideWhenNoSelection = {hideWhenNoSelection})");
            }
            
            // Validate UI references
            ValidateUIReferences();
            
            // BACKUP: If references are null, try to find them automatically
            if (selectionPanel == null || unitNameText == null || healthText == null || attackText == null || defenseText == null || healthBar == null)
            {
                Debug.LogWarning("SelectionInfoUI: Some references are null, attempting to find them automatically...");
                FindUIReferencesAutomatically();
            }
            
            LogUIReferenceStatus("END OF START");
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
            
            if (healthText == null)
            {
                Debug.LogError("SelectionInfoUI: healthText is null! Health text won't update.");
                allReferencesValid = false;
            }
            
            if (healthBar == null)
            {
                Debug.LogError("SelectionInfoUI: healthBar is null! Health bar won't update.");
                allReferencesValid = false;
            }
            
            if (attackText == null)
            {
                Debug.LogError("SelectionInfoUI: attackText is null! Attack text won't update.");
                allReferencesValid = false;
            }
            
            if (defenseText == null)
            {
                Debug.LogError("SelectionInfoUI: defenseText is null! Defense text won't update.");
                allReferencesValid = false;
            }
            
            if (allReferencesValid)
            {
                Debug.Log("SelectionInfoUI: All UI references are properly assigned ✓");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: Some UI references are missing! Use SelectionUISetup to fix this.");
            }
        }
        
        private void FindUIReferencesAutomatically()
        {
            Debug.Log("SelectionInfoUI: Attempting automatic UI reference discovery...");
            
            // Try to find SelectionPanel by name
            if (selectionPanel == null)
            {
                var foundPanel = GameObject.Find("SelectionPanel");
                if (foundPanel != null)
                {
                    selectionPanel = foundPanel;
                    Debug.Log($"SelectionInfoUI: Auto-found SelectionPanel: {foundPanel.name}");
                }
            }
            
            // If we have a panel, look for children
            if (selectionPanel != null)
            {
                if (unitNameText == null)
                {
                    var found = selectionPanel.transform.Find("UnitNameText")?.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        unitNameText = found;
                        Debug.Log($"SelectionInfoUI: Auto-found UnitNameText: {found.name}");
                    }
                }
                
                if (healthText == null)
                {
                    var found = selectionPanel.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        healthText = found;
                        Debug.Log($"SelectionInfoUI: Auto-found HealthText: {found.name}");
                    }
                }
                
                if (attackText == null)
                {
                    var found = selectionPanel.transform.Find("AttackText")?.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        attackText = found;
                        Debug.Log($"SelectionInfoUI: Auto-found AttackText: {found.name}");
                    }
                }
                
                if (defenseText == null)
                {
                    var found = selectionPanel.transform.Find("DefenseText")?.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        defenseText = found;
                        Debug.Log($"SelectionInfoUI: Auto-found DefenseText: {found.name}");
                    }
                }
                
                if (healthBar == null)
                {
                    var found = selectionPanel.transform.Find("HealthBar")?.GetComponent<Slider>();
                    if (found != null)
                    {
                        healthBar = found;
                        Debug.Log($"SelectionInfoUI: Auto-found HealthBar: {found.name}");
                    }
                }
            }
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
        
        private void OnUnitSelected(UnitSelectable selectedUnit)
        {
            Debug.Log($"SelectionInfoUI: OnUnitSelected called with unit: {selectedUnit?.name}");
            
            // For single selection, show the selected unit
            // For multiple selection, show the first/last selected unit
            currentSelectedUnit = selectedUnit;
            
            if (selectionPanel != null)
            {
                Debug.Log($"SelectionInfoUI: Showing panel (was active: {selectionPanel.activeSelf})");
                selectionPanel.SetActive(true);
                Debug.Log($"SelectionInfoUI: Panel set to active (now active: {selectionPanel.activeSelf})");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: selectionPanel is NULL! Cannot show UI.");
            }
            
            // Add a small delay to ensure unit is fully initialized before updating UI
            StartCoroutine(UpdateUIAfterFrame());
        }
        
        private System.Collections.IEnumerator UpdateUIAfterFrame()
        {
            yield return null; // Wait one frame
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
                Debug.LogWarning("SelectionInfoUI: currentSelectedUnit is null");
                return;
            }
            
            Unit unit = currentSelectedUnit.GetUnit();
            if (unit == null) 
            {
                Debug.LogError("SelectionInfoUI: Unit component is null! This should not happen with the new unified Unit component.");
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
                if (debugMode) Debug.Log($"SelectionInfoUI: Set unit name to '{displayName}' ✓");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: unitNameText is NULL! Cannot update unit name.");
            }
            
            // Update health info
            UpdateHealthDisplay(unit);
            
            // Update attack stat
            if (attackText != null)
            {
                attackText.text = $"ATK: {unit.attack}";
                if (debugMode) Debug.Log($"SelectionInfoUI: Set attack to 'ATK: {unit.attack}' ✓");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: attackText is NULL! Cannot update attack stat.");
            }
            
            // Update defense stat
            if (defenseText != null)
            {
                defenseText.text = $"DEF: {unit.defense}";
                if (debugMode) Debug.Log($"SelectionInfoUI: Set defense to 'DEF: {unit.defense}' ✓");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: defenseText is NULL! Cannot update defense stat.");
            }
        }
        
        private void UpdateHealthDisplay(Unit unit)
        {
            // Validate unit health values to prevent NaN errors
            if (unit.maxHealth <= 0)
            {
                if (debugMode) 
                    Debug.LogWarning($"SelectionInfoUI: Unit {unit.UnitName} has invalid maxHealth: {unit.maxHealth}. Skipping health display update.");
                return;
            }
            
            // Update health text with current/max format
            if (healthText != null)
            {
                healthText.text = $"{unit.health}/{unit.maxHealth}";
                
                // Color code health text - safe division since we validated maxHealth > 0
                float healthPercentage = (float)unit.health / unit.maxHealth;
                
                // Clamp health percentage to valid range
                healthPercentage = Mathf.Clamp01(healthPercentage);
                
                if (healthPercentage > midHealthThreshold)
                    healthText.color = healthColorHigh;
                else if (healthPercentage > lowHealthThreshold)
                    healthText.color = healthColorMid;
                else
                    healthText.color = healthColorLow;
                    
                // Reduce debug spam - only log if debug mode and percentage is valid
                if (debugMode && !float.IsNaN(healthPercentage)) 
                    Debug.Log($"SelectionInfoUI: Set health text to '{unit.health}/{unit.maxHealth}' with {healthPercentage:P0} health");
            }
            else if (debugMode)
            {
                Debug.LogWarning("SelectionInfoUI: healthText is null!");
            }
            
            // Update health bar
            if (healthBar != null)
            {
                // Safe division since we validated maxHealth > 0
                float healthPercentage = (float)unit.health / unit.maxHealth;
                
                // Clamp to valid range and handle edge cases
                healthPercentage = Mathf.Clamp01(healthPercentage);
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
                
                // Reduce debug spam
                if (debugMode && !float.IsNaN(healthPercentage)) 
                    Debug.Log($"SelectionInfoUI: Set health bar to {healthPercentage:P0}");
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
        
        /// <summary>
        /// Call this method when unit stats change during gameplay to refresh the display
        /// </summary>
        public void RefreshCurrentSelection()
        {
            if (currentSelectedUnit != null)
            {
                UpdateUnitInfo();
            }
        }
        
        private void LogUIReferenceStatus(string context)
        {
            Debug.Log($"=== UI REFERENCE STATUS ({context}) ===");
            Debug.Log($"selectionPanel: {(selectionPanel != null ? selectionPanel.name : "NULL")}");
            Debug.Log($"unitNameText: {(unitNameText != null ? unitNameText.name : "NULL")}");
            Debug.Log($"healthText: {(healthText != null ? healthText.name : "NULL")}");
            Debug.Log($"healthBar: {(healthBar != null ? healthBar.name : "NULL")}");
            Debug.Log($"attackText: {(attackText != null ? attackText.name : "NULL")}");
            Debug.Log($"defenseText: {(defenseText != null ? defenseText.name : "NULL")}");
            Debug.Log($"=== END STATUS ({context}) ===");
        }
    }
} 