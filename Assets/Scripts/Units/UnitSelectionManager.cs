using UnityEngine;
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
        public static UnitSelectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UnitSelectionManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("UnitSelectionManager");
                        instance = go.AddComponent<UnitSelectionManager>();
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
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            // Handle input for clearing selection
            if (Input.GetMouseButtonDown(0))
            {
                // Check if we clicked on empty space
                CheckForEmptySpaceClick();
            }
            
            // Handle escape key to clear selection
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
            }
        }
        
        public void SelectUnit(UnitSelectable unit)
        {
            if (unit == null) return;
            
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
            
            // Fire events
            OnUnitSelected?.Invoke(unit);
            OnSelectionChanged?.Invoke(SelectedUnits);
            
            if (debugSelection)
                Debug.Log($"Selected unit: {unit.name}. Total selected: {selectedUnits.Count}");
        }
        
        public void DeselectUnit(UnitSelectable unit)
        {
            if (unit == null || !selectedUnits.Contains(unit)) return;
            
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
                    if (triggerEvents)
                        OnUnitDeselected?.Invoke(unit);
                }
            }
            
            if (triggerEvents)
            {
                OnSelectionCleared?.Invoke();
                OnSelectionChanged?.Invoke(SelectedUnits);
            }
            
            if (debugSelection)
                Debug.Log("Selection cleared");
        }
        
        public void ToggleUnitSelection(UnitSelectable unit)
        {
            if (unit == null) return;
            
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
        
        public List<UnitSelectable> GetSelectedUnitsOfPlayer(Creature.Player player)
        {
            return selectedUnits.Where(unit => unit.GetUnit().player == player).ToList();
        }
        
        public void SelectUnitsOfPlayer(Creature.Player player)
        {
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
            if (instance == this)
            {
                instance = null;
            }
        }
    }
} 