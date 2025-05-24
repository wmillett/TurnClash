using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;

namespace TurnClash.UI
{
    /// <summary>
    /// Utility script to diagnose and fix common issues with the Selection UI system
    /// Addresses cleanup problems and UI update failures
    /// </summary>
    public class SelectionUIFixer : MonoBehaviour
    {
        [Header("Diagnostics")]
        [SerializeField] private bool runDiagnosticsOnStart = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Fix Options")]
        [SerializeField] private bool autoFixUIReferences = true;
        [SerializeField] private bool ensureManagerCleanup = true;
        
        private void Start()
        {
            if (runDiagnosticsOnStart)
            {
                Invoke(nameof(RunFullDiagnostics), 0.5f); // Small delay to ensure everything is initialized
            }
        }
        
        [ContextMenu("Run Full Diagnostics")]
        public void RunFullDiagnostics()
        {
            if (enableDebugLogs) Debug.Log("=== SelectionUIFixer: Running Full Diagnostics ===");
            
            CheckSelectionManager();
            CheckSelectionUI();
            CheckUnits();
            
            if (autoFixUIReferences)
            {
                AttemptUIFix();
            }
            
            if (enableDebugLogs) Debug.Log("=== SelectionUIFixer: Diagnostics Complete ===");
        }
        
        private void CheckSelectionManager()
        {
            if (enableDebugLogs) Debug.Log("--- Checking UnitSelectionManager ---");
            
            var manager = UnitSelectionManager.Instance;
            if (manager == null)
            {
                Debug.LogError("UnitSelectionManager.Instance is null!");
                return;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"UnitSelectionManager found: {manager.name}");
                Debug.Log($"Selected units count: {manager.SelectionCount}");
                Debug.Log($"Has selection: {manager.HasSelection}");
            }
            
            // Check if manager has DontDestroyOnLoad (which causes cleanup issues)
            if (manager.gameObject.scene.name == "DontDestroyOnLoad")
            {
                Debug.LogWarning("UnitSelectionManager is marked as DontDestroyOnLoad - this may cause cleanup issues!");
                
                if (ensureManagerCleanup)
                {
                    Debug.Log("Attempting to fix manager cleanup issue...");
                    // Remove DontDestroyOnLoad behavior
                    var newParent = FindObjectOfType<Canvas>()?.transform ?? transform;
                    manager.transform.SetParent(newParent);
                    Debug.Log("Manager moved to scene hierarchy for proper cleanup");
                }
            }
        }
        
        private void CheckSelectionUI()
        {
            if (enableDebugLogs) Debug.Log("--- Checking SelectionInfoUI ---");
            
            var selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI == null)
            {
                Debug.LogError("No SelectionInfoUI component found in scene!");
                Debug.LogError("Make sure SelectionInfoUI script is attached to a GameObject (likely your SelectionPanel)");
                return;
            }
            
            if (enableDebugLogs) 
            {
                Debug.Log($"SelectionInfoUI component found on GameObject: {selectionUI.gameObject.name}");
                Debug.Log($"SelectionInfoUI GameObject active: {selectionUI.gameObject.activeInHierarchy}");
                Debug.Log($"SelectionInfoUI component enabled: {selectionUI.enabled}");
            }
            
            // Use reflection to check UI references
            var uiType = typeof(SelectionInfoUI);
            var fields = uiType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool hasErrors = false;
            bool hasSelectionPanel = false;
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(GameObject) || 
                    field.FieldType == typeof(TextMeshProUGUI) || 
                    field.FieldType == typeof(Slider))
                {
                    var value = field.GetValue(selectionUI);
                    if (value == null)
                    {
                        Debug.LogError($"SelectionInfoUI.{field.Name} is null!");
                        hasErrors = true;
                    }
                    else 
                    {
                        if (enableDebugLogs)
                            Debug.Log($"SelectionInfoUI.{field.Name} ✓ -> {((UnityEngine.Object)value).name}");
                        
                        // Check if selectionPanel is assigned correctly
                        if (field.Name == "selectionPanel")
                        {
                            hasSelectionPanel = true;
                            GameObject panel = (GameObject)value;
                            if (enableDebugLogs)
                            {
                                Debug.Log($"Selection panel active: {panel.activeInHierarchy}");
                                Debug.Log($"Selection panel children count: {panel.transform.childCount}");
                            }
                        }
                    }
                }
            }
            
            if (!hasErrors && enableDebugLogs)
            {
                Debug.Log("All SelectionInfoUI references are properly assigned ✓");
            }
            
            if (!hasSelectionPanel)
            {
                Debug.LogError("SelectionInfoUI.selectionPanel is not assigned! This will prevent UI from showing.");
            }
            
            // Additional debugging for UI activation issues
            if (enableDebugLogs)
            {
                Debug.Log("--- UI Activation Debugging ---");
                var manager = UnitSelectionManager.Instance;
                if (manager != null)
                {
                    Debug.Log($"UnitSelectionManager has {manager.OnUnitSelected?.GetInvocationList()?.Length ?? 0} OnUnitSelected subscribers");
                    
                    // Check if SelectionInfoUI is subscribed to events
                    if (manager.OnUnitSelected != null)
                    {
                        var subscribers = manager.OnUnitSelected.GetInvocationList();
                        bool isSubscribed = false;
                        foreach (var sub in subscribers)
                        {
                            if (sub.Target?.GetType() == typeof(SelectionInfoUI))
                            {
                                isSubscribed = true;
                                break;
                            }
                        }
                        Debug.Log($"SelectionInfoUI subscribed to OnUnitSelected: {isSubscribed}");
                    }
                }
            }
        }
        
        private void CheckUnits()
        {
            if (enableDebugLogs) Debug.Log("--- Checking Units ---");
            
            var units = FindObjectsOfType<Unit>();
            var selectableUnits = FindObjectsOfType<UnitSelectable>();
            
            if (enableDebugLogs)
            {
                Debug.Log($"Found {units.Length} Unit components");
                Debug.Log($"Found {selectableUnits.Length} UnitSelectable components");
            }
            
            // Check if units have proper components
            foreach (var unit in units)
            {
                var selectable = unit.GetComponent<UnitSelectable>();
                var collider = unit.GetComponent<Collider>();
                
                if (selectable == null)
                {
                    Debug.LogWarning($"Unit {unit.name} is missing UnitSelectable component!");
                }
                
                if (collider == null)
                {
                    Debug.LogWarning($"Unit {unit.name} is missing Collider component!");
                }
                
                if (enableDebugLogs && selectable != null && collider != null)
                {
                    Debug.Log($"Unit {unit.name} ✓ (has UnitSelectable and Collider)");
                }
            }
        }
        
        private void AttemptUIFix()
        {
            if (enableDebugLogs) Debug.Log("--- Attempting UI Fix ---");
            
            var selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI == null)
            {
                Debug.LogError("Cannot fix UI: No SelectionInfoUI found!");
                return;
            }
            
            // Check if there's a SelectionUISetup that can help
            var uiSetup = FindObjectOfType<SelectionUISetup>();
            if (uiSetup != null)
            {
                Debug.Log("Found SelectionUISetup - attempting to recreate UI...");
                uiSetup.CreateSelectionUI();
                Debug.Log("UI recreation attempt completed. Check console for results.");
            }
            else
            {
                Debug.LogWarning("No SelectionUISetup found. Cannot auto-fix UI references.");
                Debug.LogWarning("Please add SelectionUISetup component to scene and use 'Setup Selection UI' to fix references.");
            }
        }
        
        [ContextMenu("Force Update Current Selection")]
        public void ForceUpdateCurrentSelection()
        {
            var selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI != null)
            {
                selectionUI.ForceUpdateUI();
                Debug.Log("Forced UI update");
            }
            else
            {
                Debug.LogError("No SelectionInfoUI found to update!");
            }
        }
        
        [ContextMenu("Test Selection Events")]
        public void TestSelectionEvents()
        {
            if (enableDebugLogs) Debug.Log("--- Testing Selection Events ---");
            
            var manager = UnitSelectionManager.Instance;
            if (manager == null)
            {
                Debug.LogError("Cannot test: UnitSelectionManager.Instance is null!");
                return;
            }
            
            var units = FindObjectsOfType<UnitSelectable>();
            if (units.Length == 0)
            {
                Debug.LogError("Cannot test: No UnitSelectable objects found!");
                return;
            }
            
            Debug.Log($"Testing selection with {units[0].name}...");
            manager.SelectUnit(units[0]);
            
            // Test after a frame
            Invoke(nameof(TestSelectionEventsDelayed), 0.1f);
        }
        
        private void TestSelectionEventsDelayed()
        {
            var manager = UnitSelectionManager.Instance;
            if (manager.HasSelection)
            {
                Debug.Log($"Selection test successful: {manager.FirstSelectedUnit?.name} is selected");
            }
            else
            {
                Debug.LogError("Selection test failed: No unit selected after SelectUnit call");
            }
            
            // Clear selection
            manager.ClearSelection();
            Debug.Log("Selection cleared for test cleanup");
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && ensureManagerCleanup)
            {
                // App resumed - check if manager still exists properly
                CheckSelectionManager();
            }
        }
        
        [ContextMenu("Debug UI Activation Issue")]
        public void DebugUIActivationIssue()
        {
            if (enableDebugLogs) Debug.Log("=== Debugging UI Activation Issue ===");
            
            var selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI == null)
            {
                Debug.LogError("SelectionInfoUI component not found!");
                return;
            }
            
            Debug.Log($"SelectionInfoUI found on: {selectionUI.gameObject.name}");
            
            // Check if the SelectionInfoUI references are set up
            var selectionPanelField = typeof(SelectionInfoUI).GetField("selectionPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hideWhenNoSelectionField = typeof(SelectionInfoUI).GetField("hideWhenNoSelection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (selectionPanelField != null)
            {
                var panel = selectionPanelField.GetValue(selectionUI) as GameObject;
                if (panel != null)
                {
                    Debug.Log($"Selection panel: {panel.name}");
                    Debug.Log($"Panel active in hierarchy: {panel.activeInHierarchy}");
                    Debug.Log($"Panel active self: {panel.activeSelf}");
                    Debug.Log($"Panel children count: {panel.transform.childCount}");
                    
                    // List children
                    for (int i = 0; i < panel.transform.childCount; i++)
                    {
                        var child = panel.transform.GetChild(i);
                        Debug.Log($"  Child {i}: {child.name} (active: {child.gameObject.activeSelf})");
                    }
                }
                else
                {
                    Debug.LogError("Selection panel is null!");
                }
            }
            
            if (hideWhenNoSelectionField != null)
            {
                var hideWhenNoSelection = (bool)hideWhenNoSelectionField.GetValue(selectionUI);
                Debug.Log($"Hide when no selection: {hideWhenNoSelection}");
            }
            
            // Check event subscription
            var manager = UnitSelectionManager.Instance;
            if (manager != null)
            {
                Debug.Log($"Manager found: {manager.name}");
                Debug.Log($"OnUnitSelected subscribers: {manager.OnUnitSelected?.GetInvocationList()?.Length ?? 0}");
                Debug.Log($"OnSelectionCleared subscribers: {manager.OnSelectionCleared?.GetInvocationList()?.Length ?? 0}");
            }
            
            Debug.Log("=== End UI Activation Debug ===");
        }
        
        [ContextMenu("Comprehensive UI Diagnostic")]
        public void ComprehensiveUIDiagnostic()
        {
            Debug.Log("=== COMPREHENSIVE UI DIAGNOSTIC ===");
            
            // 1. Check for compilation errors
            Debug.Log("1. Checking for script compilation issues...");
            
            // 2. Check if SelectionInfoUI type exists
            var selectionUIType = System.Type.GetType("TurnClash.UI.SelectionInfoUI");
            if (selectionUIType == null)
            {
                Debug.LogError("SelectionInfoUI type not found! This means:");
                Debug.LogError("  - Script has compilation errors");
                Debug.LogError("  - Namespace mismatch (should be TurnClash.UI)");
                Debug.LogError("  - Script file is missing or corrupted");
                return;
            }
            else
            {
                Debug.Log("✓ SelectionInfoUI type found in TurnClash.UI namespace");
            }
            
            // 3. Try to find SelectionInfoUI component
            var selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI == null)
            {
                Debug.LogError("SelectionInfoUI component not found in scene!");
                Debug.LogError("This means:");
                Debug.LogError("  - No GameObject has SelectionInfoUI component attached");
                Debug.LogError("  - GameObject with component is inactive");
                Debug.LogError("  - Component is disabled");
                
                // 4. Let's check all GameObjects to see if any should have this component
                Debug.Log("Checking all GameObjects for potential SelectionInfoUI candidates...");
                
                var allGameObjects = FindObjectsOfType<GameObject>();
                bool foundSelectionPanel = false;
                
                foreach (var go in allGameObjects)
                {
                    if (go.name.ToLower().Contains("selection"))
                    {
                        foundSelectionPanel = true;
                        Debug.Log($"Found GameObject with 'selection' in name: {go.name}");
                        Debug.Log($"  - Active: {go.activeInHierarchy}");
                        //Debug.Log($"  - Components: {string.Join(", ", go.GetComponents<Component>().Select(c => c.GetType().Name))}");
                        
                        // Check if it has SelectionInfoUI component
                        var selectionComp = go.GetComponent<SelectionInfoUI>();
                        if (selectionComp != null)
                        {
                            Debug.Log($"  - ✓ Has SelectionInfoUI component!");
                        }
                        else
                        {
                            Debug.Log($"  - ✗ No SelectionInfoUI component");
                        }
                    }
                }
                
                if (!foundSelectionPanel)
                {
                    Debug.LogWarning("No GameObject found with 'selection' in the name. Looking for UI-related objects...");
                    
                    foreach (var go in allGameObjects)
                    {
                        if (go.name.ToLower().Contains("ui") || go.name.ToLower().Contains("panel"))
                        {
                            Debug.Log($"Found UI GameObject: {go.name}");
                            Debug.Log($"  - Active: {go.activeInHierarchy}");
                            var selectionComp = go.GetComponent<SelectionInfoUI>();
                            if (selectionComp != null)
                            {
                                Debug.Log($"  - ✓ Has SelectionInfoUI component!");
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"✓ SelectionInfoUI component found on: {selectionUI.gameObject.name}");
                Debug.Log($"  - GameObject active: {selectionUI.gameObject.activeInHierarchy}");
                Debug.Log($"  - Component enabled: {selectionUI.enabled}");
            }
            
            // 5. Check for Canvas and UI setup
            Debug.Log("5. Checking UI setup...");
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✓ Canvas found: {canvas.name}");
                Debug.Log($"  - Render Mode: {canvas.renderMode}");
                Debug.Log($"  - Active: {canvas.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("No Canvas found in scene!");
            }
            
            Debug.Log("=== END COMPREHENSIVE DIAGNOSTIC ===");
        }
    }
} 