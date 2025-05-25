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
        [SerializeField] private TextMeshProUGUI statusText; // For showing defending status
        
        [Header("Health Bar Settings")]
        [SerializeField] private Color healthColorHigh = Color.green;
        [SerializeField] private Color healthColorMid = Color.yellow;
        [SerializeField] private Color healthColorLow = Color.red;
        [SerializeField] private float midHealthThreshold = 0.6f;
        [SerializeField] private float lowHealthThreshold = 0.3f;
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenNoSelection = true;
        [SerializeField] private bool debugMode = false; // TEMPORARILY DISABLED for victory panel debugging
        
        private UnitSelectable currentSelectedUnit;
        private Unit currentDisplayedUnit; // Track what unit we're currently displaying
        private bool isShowingHover = false; // Track if we're showing hover vs selection
        
        private void Awake()
        {
            Debug.Log("SelectionInfoUI: Awake() called");
            LogUIReferenceStatus("AWAKE");
        }
        
        private void Start()
        {
            LogUIReferenceStatus("START OF START");
            
            // Start coroutine to wait for UnitSelectionManager to be ready
            StartCoroutine(WaitForSelectionManagerAndSubscribe());
            
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
        
        private System.Collections.IEnumerator WaitForSelectionManagerAndSubscribe()
        {
            // Wait up to 5 seconds for UnitSelectionManager to be ready
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                if (UnitSelectionManager.Instance != null)
                {
                    // Successfully found the manager, subscribe to events
                    UnitSelectionManager.Instance.OnUnitSelected += OnUnitSelected;
                    UnitSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
                    Debug.Log("SelectionInfoUI: Successfully subscribed to selection events");
                    yield break; // Exit the coroutine
                }
                
                // Wait a frame and try again
                yield return null;
                elapsed += Time.deltaTime;
            }
            
            // If we reach here, we timed out
            Debug.LogError("SelectionInfoUI: Timeout waiting for UnitSelectionManager.Instance! Events not subscribed.");
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
                Debug.LogError("SelectionInfoUI: defenseText is null! defence text won't update.");
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
                
                if (statusText == null)
                {
                    var found = selectionPanel.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        statusText = found;
                        Debug.Log($"SelectionInfoUI: Auto-found StatusText: {found.name}");
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
            
            currentSelectedUnit = selectedUnit;
            
            // Notify the hover tooltip system about selection change
            UnitHoverTooltip.OnSelectionChanged();
        }
        
        /// <summary>
        /// Show unit info for a specific unit (called by hover tooltip system)
        /// </summary>
        public void ShowUnitInfo(Unit unit, bool isHover = false)
        {
            if (unit == null)
            {
                HideUnitInfo();
                return;
            }
            
            currentDisplayedUnit = unit;
            isShowingHover = isHover;
            
            // Show the panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                if (debugMode)
                    Debug.Log($"SelectionInfoUI: Showing {(isHover ? "hover" : "selection")} info for {unit.UnitName}");
            }
            
            // Update the UI with unit info
            UpdateUnitInfo(unit, isHover);
        }
        
        /// <summary>
        /// Hide the unit info UI
        /// </summary>
        public void HideUnitInfo()
        {
            currentDisplayedUnit = null;
            isShowingHover = false;
            
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                if (debugMode)
                    Debug.Log("SelectionInfoUI: Panel hidden");
            }
            else
            {
                ClearUnitInfo();
            }
        }
        
        private void OnSelectionCleared()
        {
            if (debugMode)
                Debug.Log("SelectionInfoUI: Selection cleared");
            
            currentSelectedUnit = null;
            
            // Notify the hover tooltip system about selection change
            UnitHoverTooltip.OnSelectionChanged();
        }
        
        private void UpdateUnitInfo(Unit unit, bool isHover)
        {
            if (unit == null) 
            {
                Debug.LogWarning("SelectionInfoUI: unit is null");
                return;
            }
            
            if (debugMode)
            {
                Debug.Log($"SelectionInfoUI: Updating UI for {unit.UnitName} ({(isHover ? "hover" : "selection")}) - HP:{unit.health}/{unit.maxHealth}, ATK:{unit.attack}, DEF:{unit.defence}");
            }
            
            // Update unit name - use the UnitName property instead of GameObject name
            if (unitNameText != null)
            {
                string displayName = !string.IsNullOrEmpty(unit.UnitName) ? unit.UnitName : "Unknown Unit";
                unitNameText.text = displayName;
                
                // Set the unit name color to match the player's color (same for both hover and selection)
                Color playerColor = GetPlayerColor(unit.player);
                unitNameText.color = playerColor;
                
                if (debugMode) Debug.Log($"SelectionInfoUI: Set unit name to '{displayName}' with {unit.player} color ✓");
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
            
            // Update defence stat
            if (defenseText != null)
            {
                defenseText.text = $"DEF: {unit.defence}";
                if (debugMode) Debug.Log($"SelectionInfoUI: Set defence to 'DEF: {unit.defence}' ✓");
            }
            else
            {
                Debug.LogError("SelectionInfoUI: defenseText is NULL! Cannot update defence stat.");
            }
            
            // Update status text (defending, etc.)
            if (statusText != null)
            {
                if (unit.IsDefending)
                {
                    statusText.text = "DEFENDING";
                    statusText.color = Color.yellow; // Make defending status visible
                }
                else
                {
                    statusText.text = ""; // Clear status if not defending
                }
                
                if (debugMode) Debug.Log($"SelectionInfoUI: Set status to '{statusText.text}' ✓");
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
            {
                unitNameText.text = "No Unit Selected";
                unitNameText.color = Color.white; // Reset to default color
            }
                
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
            
            if (statusText != null)
            {
                statusText.text = "";
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
            if (currentDisplayedUnit != null)
            {
                Debug.Log("SelectionInfoUI: Forcing UI update...");
                UpdateUnitInfo(currentDisplayedUnit, isShowingHover);
            }
            else
            {
                Debug.Log("SelectionInfoUI: No unit displayed to update");
            }
        }
        
        /// <summary>
        /// Call this method when unit stats change during gameplay to refresh the display
        /// </summary>
        public void RefreshCurrentSelection()
        {
            if (currentDisplayedUnit != null)
            {
                UpdateUnitInfo(currentDisplayedUnit, isShowingHover);
            }
        }
        
        private Color GetPlayerColor(Unit.Player player)
        {
            // Find the UnitSpawner to get player colors
            UnitSpawner spawner = FindObjectOfType<UnitSpawner>();
            if (spawner != null)
            {
                return spawner.GetPlayerColor(player);
            }
            
            // Fallback colors if UnitSpawner not found
            switch (player)
            {
                case Unit.Player.Player1:
                    return Color.blue;
                case Unit.Player.Player2:
                    return Color.red;
                default:
                    return Color.white;
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