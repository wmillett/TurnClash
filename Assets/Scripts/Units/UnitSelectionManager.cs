using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TurnClash.Units;

namespace TurnClash.Units
{
    public class UnitSelectionManager : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private bool allowMultipleSelection = false; // For turn-based, usually single selection
        [SerializeField] private KeyCode multiSelectKey = KeyCode.LeftControl; // Hold to multi-select
        [SerializeField] private bool debugSelection = true;
        
        // Singleton instance
        private static UnitSelectionManager instance;
        private static bool isApplicationQuitting = false; // Prevent creation during shutdown
        private static bool isSceneUnloading = false; // Prevent creation during scene changes
        
        public static UnitSelectionManager Instance
        {
            get
            {
                // Don't create new instances during application quit or scene unload
                if (isApplicationQuitting || isSceneUnloading)
                {
                    return null;
                }
                
                if (instance == null)
                {
                    instance = FindObjectOfType<UnitSelectionManager>();
                    if (instance == null && !isApplicationQuitting && !isSceneUnloading)
                    {
                        GameObject go = new GameObject("UnitSelectionManager");
                        instance = go.AddComponent<UnitSelectionManager>();
                        Debug.Log("UnitSelectionManager: Created new singleton instance");
                    }
                }
                return instance;
            }
        }
        
        // Selected units tracking
        private List<UnitSelectable> selectedUnits = new List<UnitSelectable>();
        private UnitSelectable lastSelectedUnit;
        
        // Events for other systems to subscribe to
        public System.Action<UnitSelectable> OnUnitSelected;
        public System.Action<UnitSelectable> OnUnitDeselected;
        public System.Action<List<UnitSelectable>> OnSelectionChanged;
        public System.Action OnSelectionCleared;
        
        // Properties
        public List<UnitSelectable> SelectedUnits => new List<UnitSelectable>(selectedUnits);
        public UnitSelectable FirstSelectedUnit => selectedUnits.Count > 0 ? selectedUnits[0] : null;
        public UnitSelectable LastSelectedUnit => lastSelectedUnit;
        public int SelectionCount => selectedUnits.Count;
        public bool HasSelection => selectedUnits.Count > 0;
        
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.Log("UnitSelectionManager: Destroying duplicate instance");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            isApplicationQuitting = false; // Reset the flag when awaking
            Debug.Log("UnitSelectionManager: Instance created and initialized");
        }
        
        private void Start()
        {
            // Ensure the flag is cleared on scene start
            isApplicationQuitting = false;
            isSceneUnloading = false;
            Debug.Log("UnitSelectionManager: Start() called, ready for operations");
            
            // Subscribe to scene unloading events to automatically detect scene changes
            if (!sceneEventsSubscribed)
            {
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                sceneEventsSubscribed = true;
                Debug.Log("UnitSelectionManager: Subscribed to scene change events");
            }
        }
        
        // Track if we've subscribed to scene events (static to persist across instances)
        private static bool sceneEventsSubscribed = false;
        
        private static void OnActiveSceneChanged(Scene current, Scene next)
        {
            // When scene changes, mark as unloading to prevent creation during transition
            isSceneUnloading = true;
            Debug.Log("UnitSelectionManager: Scene changing, preventing new instance creation");
        }
        
        private void OnApplicationQuit()
        {
            // Prevent any new singleton creation during quit
            isApplicationQuitting = true;
            if (debugSelection)
                Debug.Log("UnitSelectionManager: Application quitting, preventing new instance creation");
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // Clean up when losing focus (but not during quit or scene unloading)
            if (!hasFocus && !isApplicationQuitting && !isSceneUnloading)
            {
                if (debugSelection)
                    Debug.Log("UnitSelectionManager: Application lost focus, clearing selections");
                ClearSelection(false);
            }
        }
        
        private void Update()
        {
            // Don't process input during application quit or scene unloading
            if (isApplicationQuitting || isSceneUnloading)
                return;
                
            // Handle input for clearing selection
            if (Input.GetMouseButtonDown(0))
            {
                // Check if we clicked on empty space
                CheckForEmptySpaceClick();
            }
            
            // Handle right-click to deselect if something is selected
            if (Input.GetMouseButtonDown(1))
            {
                if (HasSelection)
                {
                    ClearSelection();
                    if (debugSelection)
                        Debug.Log("Selection cleared via right-click");
                }
            }
            
            // Handle escape key to clear selection
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
            }
        }
        
        public void SelectUnit(UnitSelectable unit)
        {
            if (unit == null || isApplicationQuitting || isSceneUnloading) return;
            
            bool isMultiSelect = allowMultipleSelection && Input.GetKey(multiSelectKey);
            
            // If not multi-selecting, clear current selection first
            if (!isMultiSelect)
            {
                ClearSelection(false); // Don't trigger events yet
            }
            
            // Don't select if already selected
            if (selectedUnits.Contains(unit))
            {
                if (debugSelection)
                    Debug.Log($"Unit {unit.name} is already selected");
                return;
            }
            
            // Add to selection
            selectedUnits.Add(unit);
            lastSelectedUnit = unit;
            
            // Notify the unit it's selected
            unit.Select();
            
            // Fire events with validation
            if (debugSelection)
            {
                Debug.Log($"UnitSelectionManager: Firing OnUnitSelected for {unit.name}");
                Debug.Log($"OnUnitSelected has {OnUnitSelected?.GetInvocationList()?.Length ?? 0} subscribers");
                Debug.Log($"OnSelectionChanged has {OnSelectionChanged?.GetInvocationList()?.Length ?? 0} subscribers");
            }
            
            OnUnitSelected?.Invoke(unit);
            OnSelectionChanged?.Invoke(SelectedUnits);
            
            if (debugSelection)
                Debug.Log($"Selected unit: {unit.name}. Total selected: {selectedUnits.Count}");
        }
        
        public void DeselectUnit(UnitSelectable unit)
        {
            if (unit == null || !selectedUnits.Contains(unit) || isApplicationQuitting || isSceneUnloading) return;
            
            selectedUnits.Remove(unit);
            
            // Update last selected if this was it
            if (lastSelectedUnit == unit)
            {
                lastSelectedUnit = selectedUnits.Count > 0 ? selectedUnits[selectedUnits.Count - 1] : null;
            }
            
            // Notify the unit it's deselected
            unit.Deselect();
            
            // Fire events
            OnUnitDeselected?.Invoke(unit);
            OnSelectionChanged?.Invoke(SelectedUnits);
            
            if (debugSelection)
                Debug.Log($"Deselected unit: {unit.name}. Total selected: {selectedUnits.Count}");
        }
        
        public void ClearSelection(bool triggerEvents = true)
        {
            if (selectedUnits.Count == 0) return;
            
            // Deselect all units
            var unitsToDeselect = new List<UnitSelectable>(selectedUnits);
            selectedUnits.Clear();
            lastSelectedUnit = null;
            
            foreach (var unit in unitsToDeselect)
            {
                if (unit != null)
                {
                    unit.Deselect();
                    if (triggerEvents && !isApplicationQuitting && !isSceneUnloading)
                        OnUnitDeselected?.Invoke(unit);
                }
            }
            
            if (triggerEvents && !isApplicationQuitting && !isSceneUnloading)
            {
                OnSelectionCleared?.Invoke();
                OnSelectionChanged?.Invoke(SelectedUnits);
            }
            
            if (debugSelection)
                Debug.Log("Selection cleared");
        }
        
        public void ToggleUnitSelection(UnitSelectable unit)
        {
            if (unit == null || isApplicationQuitting || isSceneUnloading) return;
            
            if (IsUnitSelected(unit))
            {
                DeselectUnit(unit);
            }
            else
            {
                SelectUnit(unit);
            }
        }
        
        public bool IsUnitSelected(UnitSelectable unit)
        {
            return unit != null && selectedUnits.Contains(unit);
        }
        
        public List<UnitSelectable> GetSelectedUnitsOfPlayer(Unit.Player player)
        {
            if (isApplicationQuitting || isSceneUnloading) return new List<UnitSelectable>();
            return selectedUnits.Where(unit => unit.GetUnit()?.player == player).ToList();
        }
        
        public void SelectUnitsOfPlayer(Unit.Player player)
        {
            if (isApplicationQuitting || isSceneUnloading) return;
            
            var playerUnits = FindObjectsOfType<UnitSelectable>()
                .Where(unit => unit.GetUnit().player == player)
                .ToList();
            
            ClearSelection(false);
            
            foreach (var unit in playerUnits)
            {
                selectedUnits.Add(unit);
                unit.Select();
                OnUnitSelected?.Invoke(unit);
            }
            
            if (playerUnits.Count > 0)
            {
                lastSelectedUnit = playerUnits[playerUnits.Count - 1];
                OnSelectionChanged?.Invoke(SelectedUnits);
            }
            
            if (debugSelection)
                Debug.Log($"Selected all {player} units: {playerUnits.Count} units");
        }
        
        private void CheckForEmptySpaceClick()
        {
            if (isApplicationQuitting || isSceneUnloading) return;
            
            // Cast a ray from camera to see what we hit
            Camera cam = Camera.main;
            if (cam == null) return;
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Check if we hit something with a UnitSelectable
            if (Physics.Raycast(ray, out hit))
            {
                UnitSelectable hitUnit = hit.collider.GetComponent<UnitSelectable>();
                if (hitUnit != null)
                {
                    // We hit a unit, selection will be handled by the unit itself
                    return;
                }
            }
            
            // We didn't hit a unit, clear selection
            ClearSelection();
        }
        
        // Utility methods for external systems
        public void SetMultipleSelectionEnabled(bool enabled)
        {
            allowMultipleSelection = enabled;
        }
        
        public void SetDebugMode(bool enabled)
        {
            debugSelection = enabled;
        }
        
        // Clean up when destroyed
        private void OnDestroy()
        {
            if (debugSelection)
                Debug.Log("UnitSelectionManager: OnDestroy called");
                
            // Clear all selections before destroying (don't trigger events during cleanup)
            ClearSelection(false);
            
            // Clear all event subscriptions to prevent memory leaks
            OnUnitSelected = null;
            OnUnitDeselected = null;
            OnSelectionChanged = null;
            OnSelectionCleared = null;
            
            // Clear singleton reference if this is the active instance
            if (instance == this)
            {
                instance = null;
            }
            
            // Only set quitting flag if we're actually quitting the application
            // Don't set it during scene changes or manual destroy
            if (debugSelection)
                Debug.Log("UnitSelectionManager: Cleanup complete, instance cleared");
        }
        
        // Add method to validate and clean up null units
        private void CleanupNullUnits()
        {
            if (isApplicationQuitting || isSceneUnloading) return;
            
            if (selectedUnits.RemoveAll(unit => unit == null) > 0)
            {
                if (debugSelection)
                    Debug.Log("UnitSelectionManager: Removed null units from selection");
                    
                // Update last selected unit if it was null
                if (lastSelectedUnit == null && selectedUnits.Count > 0)
                {
                    lastSelectedUnit = selectedUnits[selectedUnits.Count - 1];
                }
                else if (selectedUnits.Count == 0)
                {
                    lastSelectedUnit = null;
                }
            }
        }
        
        // Call this periodically or when accessing selected units
        public void ValidateSelection()
        {
            if (!isApplicationQuitting && !isSceneUnloading)
            {
                CleanupNullUnits();
            }
        }
        
        /// <summary>
        /// Reset the application quitting flag - called by GameManager on scene start
        /// </summary>
        public static void ResetForNewScene()
        {
            isApplicationQuitting = false;
            isSceneUnloading = false; // Clear scene unloading flag
            Debug.Log("UnitSelectionManager: Reset for new scene");
        }
        
        /// <summary>
        /// Mark that scene is unloading - called by GameManager before scene change
        /// </summary>
        public static void MarkSceneUnloading()
        {
            isSceneUnloading = true;
            Debug.Log("UnitSelectionManager: Scene unloading, preventing new instance creation");
        }
    }
} 