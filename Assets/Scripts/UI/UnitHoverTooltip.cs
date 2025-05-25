using UnityEngine;
using TurnClash.Units;

namespace TurnClash.UI
{
    /// <summary>
    /// Manages hover tooltips for units using the existing SelectionInfoUI
    /// Handles priority: hover > selection > hide
    /// </summary>
    public class UnitHoverTooltip : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private bool enableHoverTooltips = true;
        [SerializeField] private float hoverDelay = 0.1f; // Small delay before showing tooltip
        // Debug hover now controlled by DebugManager
        
        // Static tracking for the hover system
        private static UnitHoverTooltip instance;
        private static Unit currentHoveredUnit;
        private static bool isHovering = false;
        private static bool isQuitting = false;
        
        // Timer for hover delay
        private float hoverTimer = 0f;
        private Unit pendingHoverUnit;
        
        // Reference to the selection UI
        private SelectionInfoUI selectionUI;
        
        public static UnitHoverTooltip Instance
        {
            get
            {
                if (isQuitting) return null;
                
                if (instance == null)
                {
                    instance = FindObjectOfType<UnitHoverTooltip>();
                    if (instance == null && !isQuitting)
                    {
                        GameObject go = new GameObject("UnitHoverTooltip");
                        instance = go.AddComponent<UnitHoverTooltip>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            isQuitting = false;
        }
        
        private void Start()
        {
            // Find the SelectionInfoUI
            selectionUI = FindObjectOfType<SelectionInfoUI>();
            if (selectionUI == null)
            {
                Debug.LogError("UnitHoverTooltip: SelectionInfoUI not found! Hover tooltips will not work.");
                enabled = false;
            }
            
#if DEBUG_HOVER_TOOLTIP
            Debug.Log("UnitHoverTooltip: Initialized successfully");
#endif
        }
        
        private void Update()
        {
            if (!enableHoverTooltips) return;
            
            // Handle hover delay timer
            if (pendingHoverUnit != null)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= hoverDelay)
                {
                    // Delay elapsed, show the tooltip
                    SetHoveredUnit(pendingHoverUnit);
                    pendingHoverUnit = null;
                    hoverTimer = 0f;
                }
            }
        }
        
        /// <summary>
        /// Called when mouse enters a unit (from UnitSelectable)
        /// </summary>
        public static void OnUnitHoverEnter(Unit unit)
        {
            if (!isQuitting && Instance != null && Instance.enableHoverTooltips)
            {
                Instance.StartHoverDelay(unit);
            }
        }
        
        /// <summary>
        /// Called when mouse exits a unit (from UnitSelectable)
        /// </summary>
        public static void OnUnitHoverExit(Unit unit)
        {
            if (!isQuitting && Instance != null)
            {
                Instance.StopHover(unit);
            }
        }
        
        /// <summary>
        /// Start the hover delay timer
        /// </summary>
        private void StartHoverDelay(Unit unit)
        {
            if (unit == null) return;
            
            // Cancel any pending hover
            pendingHoverUnit = null;
            hoverTimer = 0f;
            
            // Start new hover delay
            pendingHoverUnit = unit;
            
#if DEBUG_HOVER_TOOLTIP
            Debug.Log($"UnitHoverTooltip: Starting hover delay for {unit.UnitName}");
#endif
        }
        
        /// <summary>
        /// Stop hovering over a unit
        /// </summary>
        private void StopHover(Unit unit)
        {
            // Cancel pending hover if it matches
            if (pendingHoverUnit == unit)
            {
                pendingHoverUnit = null;
                hoverTimer = 0f;
                
#if DEBUG_HOVER_TOOLTIP
                Debug.Log($"UnitHoverTooltip: Cancelled pending hover for {unit.UnitName}");
#endif
            }
            
            // Clear current hover if it matches
            if (currentHoveredUnit == unit)
            {
                SetHoveredUnit(null);
                
#if DEBUG_HOVER_TOOLTIP
                Debug.Log($"UnitHoverTooltip: Stopped hovering {unit.UnitName}");
#endif
            }
        }
        
        /// <summary>
        /// Set the currently hovered unit
        /// </summary>
        private void SetHoveredUnit(Unit unit)
        {
            if (currentHoveredUnit == unit) return;
            
            currentHoveredUnit = unit;
            isHovering = unit != null;
            
            // Update the SelectionInfoUI based on priority
            UpdateSelectionUI();
            
#if DEBUG_HOVER_TOOLTIP
            if (unit != null)
                Debug.Log($"UnitHoverTooltip: Now hovering {unit.UnitName}");
            else
                Debug.Log("UnitHoverTooltip: No longer hovering any unit");
#endif
        }
        
        /// <summary>
        /// Update the SelectionInfoUI based on hover and selection priority
        /// </summary>
        private void UpdateSelectionUI()
        {
            if (selectionUI == null) return;
            
            // Priority: hover > selection > hide
            if (isHovering && currentHoveredUnit != null)
            {
                // Show hovered unit (highest priority)
                selectionUI.ShowUnitInfo(currentHoveredUnit, isHover: true);
            }
            else
            {
                // No hover, check for selection
                UnitSelectionManager selectionManager = UnitSelectionManager.Instance;
                if (selectionManager != null && selectionManager.HasSelection)
                {
                    UnitSelectable selectedUnitSelectable = selectionManager.FirstSelectedUnit;
                    if (selectedUnitSelectable != null)
                    {
                        Unit selectedUnit = selectedUnitSelectable.GetComponent<Unit>();
                        if (selectedUnit != null)
                        {
                            // Show selected unit
                            selectionUI.ShowUnitInfo(selectedUnit, isHover: false);
                        }
                        else
                        {
                            // Hide UI if no valid selection
                            selectionUI.HideUnitInfo();
                        }
                    }
                    else
                    {
                        // Hide UI if no valid selection
                        selectionUI.HideUnitInfo();
                    }
                }
                else
                {
                    // No selection, hide UI
                    selectionUI.HideUnitInfo();
                }
            }
        }
        
        /// <summary>
        /// Called by SelectionInfoUI when selection changes
        /// This ensures proper priority handling
        /// </summary>
        public static void OnSelectionChanged()
        {
            if (!isQuitting && Instance != null)
            {
                Instance.UpdateSelectionUI();
            }
        }
        
        /// <summary>
        /// Enable or disable hover tooltips
        /// </summary>
        public void SetHoverTooltipsEnabled(bool enabled)
        {
            enableHoverTooltips = enabled;
            
            if (!enabled)
            {
                // Clear any current hover
                SetHoveredUnit(null);
                pendingHoverUnit = null;
                hoverTimer = 0f;
            }
        }
        
        /// <summary>
        /// Set the hover delay
        /// </summary>
        public void SetHoverDelay(float delay)
        {
            hoverDelay = Mathf.Max(0f, delay);
        }
        
        /// <summary>
        /// Get the currently hovered unit (for external access)
        /// </summary>
        public static Unit GetCurrentHoveredUnit()
        {
            return currentHoveredUnit;
        }
        
        /// <summary>
        /// Check if we're currently hovering over any unit
        /// </summary>
        public static bool IsHoveringUnit()
        {
            return isHovering && currentHoveredUnit != null;
        }
        
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                currentHoveredUnit = null;
                isHovering = false;
            }
        }
        
        /// <summary>
        /// Static cleanup method to be called when scenes change
        /// </summary>
        public static void Cleanup()
        {
            isQuitting = true;
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
            instance = null;
            currentHoveredUnit = null;
            isHovering = false;
        }
    }
} 