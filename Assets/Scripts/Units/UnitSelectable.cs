using UnityEngine;
using System.Collections.Generic;
using TurnClash.Units;
using TurnClash.UI;

namespace TurnClash.Units
{
    [RequireComponent(typeof(Unit))]
    public class UnitSelectable : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0f, 1f); // Golden glow for selection
        [SerializeField] private Color attackableColor = new Color(1f, 0.2f, 0.2f, 1f); // Red glow for attackable enemies
        [SerializeField] private Color defendingTint = new Color(0.6f, 0.6f, 0.9f, 1f); // More visible blue-grey tint for defending
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private bool useEmission = true;
        [SerializeField] private bool debugVisualEffects = false; // Debug visual effect changes
        
        // Visual feedback state
        private bool isAttackableHighlighted = false;
        private bool isDefendingHighlighted = false;
        
        private Unit unit;
        private List<Renderer> renderers;
        private List<Material> materials;
        private Dictionary<Material, Color> originalEmissionColors;
        private bool isSelected = false;
        
        // Events
        public System.Action<UnitSelectable> OnSelected;
        public System.Action<UnitSelectable> OnDeselected;
        
        private void Awake()
        {
            unit = GetComponent<Unit>();
            
            // Get all renderers in this object and its children
            renderers = new List<Renderer>();
            renderers.AddRange(GetComponentsInChildren<Renderer>());
            
            // Cache all materials and their original emission colors
            materials = new List<Material>();
            originalEmissionColors = new Dictionary<Material, Color>();
            
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    materials.Add(material);
                    
                    // Store original emission color
                    if (material.HasProperty("_EmissionColor"))
                    {
                        originalEmissionColors[material] = material.GetColor("_EmissionColor");
                    }
                }
            }
        }
        
        private void OnMouseDown()
        {
            // Check if movement preview is active and this unit can be attacked
            if (MovementPreview.Instance != null && CanBeAttackedByPreviewUnit())
            {
                // Attack this unit instead of selecting it
                MovementPreview.Instance.OnEnemyUnitClicked(this);
                return;
            }
            
            // Normal unit selection behavior
            UnitSelectionManager.Instance?.SelectUnit(this);
        }
        
        /// <summary>
        /// Check if this unit can be attacked by the currently previewed unit
        /// </summary>
        private bool CanBeAttackedByPreviewUnit()
        {
            // Get the currently previewed unit
            var currentPreviewUnit = MovementPreview.Instance.GetCurrentPreviewUnit();
            if (currentPreviewUnit == null || !currentPreviewUnit.IsAlive())
                return false;
                
            // Check if this unit is an enemy of the previewed unit
            if (unit.player == currentPreviewUnit.player)
                return false; // Same team, can't attack
                
            // Check if this unit is within attack range
            Vector2Int thisPosition = unit.GetGridPosition();
            Vector2Int previewUnitPosition = currentPreviewUnit.GetGridPosition();
            
            // Calculate distance (should be 1 for adjacent attack)
            int distance = Mathf.Abs(thisPosition.x - previewUnitPosition.x) + 
                          Mathf.Abs(thisPosition.y - previewUnitPosition.y);
                          
            return distance <= 1; // Within attack range
        }
        
        public void Select()
        {
            if (isSelected) return;
            
            isSelected = true;
            
            if (useEmission)
            {
                EnableGlow();
            }
            
            OnSelected?.Invoke(this);
        }
        
        public void Deselect()
        {
            if (!isSelected) return;
            
            isSelected = false;
            
            if (useEmission)
            {
                // Restore the proper visual state (might need defending tint)
                RestoreProperVisualState();
            }
            
            OnDeselected?.Invoke(this);
        }
        
        private void EnableGlow()
        {
            foreach (var material in materials)
            {
                if (material != null)
                {
                    // Enable emission
                    material.EnableKeyword("_EMISSION");
                    
                    // Set emission color with intensity
                    Color emissionColor = glowColor * glowIntensity;
                    material.SetColor("_EmissionColor", emissionColor);
                    
                    // For URP/HDRP compatibility
                    material.SetColor("_EmissiveColor", emissionColor);
                }
            }
        }
        
        private void DisableGlow()
        {
            foreach (var material in materials)
            {
                if (material != null && originalEmissionColors.ContainsKey(material))
                {
                    // Restore original emission color
                    material.SetColor("_EmissionColor", originalEmissionColors[material]);
                    material.SetColor("_EmissiveColor", originalEmissionColors[material]);
                    
                    // If original emission was black, disable emission
                    if (originalEmissionColors[material] == Color.black)
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
        
        public Unit GetUnit()
        {
            return unit;
        }
        
        public bool IsSelected()
        {
            return isSelected;
        }
        
        /// <summary>
        /// Highlight this unit as attackable (red glow)
        /// </summary>
        public void HighlightAsAttackable()
        {
            if (isAttackableHighlighted) return;
            
            isAttackableHighlighted = true;
            
            if (useEmission)
            {
                EnableAttackableGlow();
            }
        }
        
        /// <summary>
        /// Remove attackable highlight
        /// </summary>
        public void RemoveAttackableHighlight()
        {
            if (!isAttackableHighlighted) return;
            
            isAttackableHighlighted = false;
            
            if (useEmission)
            {
                // Restore the proper visual state
                RestoreProperVisualState();
            }
        }
        
        private void EnableAttackableGlow()
        {
            foreach (var material in materials)
            {
                if (material != null)
                {
                    // Enable emission
                    material.EnableKeyword("_EMISSION");
                    
                    // Set emission color with intensity
                    Color emissionColor = attackableColor * glowIntensity;
                    material.SetColor("_EmissionColor", emissionColor);
                    
                    // For URP/HDRP compatibility
                    material.SetColor("_EmissiveColor", emissionColor);
                }
            }
        }
        
        /// <summary>
        /// Show defending visual effect (greyish tint)
        /// </summary>
        public void ShowDefendingEffect()
        {
            if (isDefendingHighlighted) return;
            
            if (debugVisualEffects)
                Debug.Log($"UnitSelectable {name}: Showing defending effect");
            
            isDefendingHighlighted = true;
            
            // Restore proper visual state which will include the defending tint
            RestoreProperVisualState();
        }
        
        /// <summary>
        /// Remove defending visual effect
        /// </summary>
        public void RemoveDefendingEffect()
        {
            if (!isDefendingHighlighted) return;
            
            isDefendingHighlighted = false;
            
            if (debugVisualEffects)
                Debug.Log($"UnitSelectable {name}: Removing defending effect");
            
            // Restore the proper visual state based on current conditions
            RestoreProperVisualState();
        }
        
        private void EnableDefendingTint()
        {
            // Get the current team color and apply defending tint to it
            Color teamColor = GetCurrentTeamColor();
            Color tintedColor = Color.Lerp(teamColor, defendingTint, 0.75f);
            
            foreach (var material in materials)
            {
                if (material != null)
                {
                    // Apply tinted color to both standard and URP properties
                    if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", tintedColor);
                    }
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", tintedColor);
                    }
                }
            }
        }
        
        private void DisableDefendingTint()
        {
            // Get the current team color instead of using stored original colors
            Color teamColor = GetCurrentTeamColor();
            
            foreach (var material in materials)
            {
                if (material != null)
                {
                    // Apply the current team color
                    if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", teamColor);
                    }
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", teamColor);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the current team color for this unit
        /// </summary>
        private Color GetCurrentTeamColor()
        {
            if (unit == null) return Color.white;
            
            // Find the UnitSpawner to get the current team color
            UnitSpawner spawner = FindObjectOfType<UnitSpawner>();
            if (spawner != null)
            {
                return spawner.GetPlayerColor(unit.player);
            }
            
            // Fallback colors if UnitSpawner not found
            switch (unit.player)
            {
                case Unit.Player.Player1:
                    return Color.blue;
                case Unit.Player.Player2:
                    return Color.red;
                default:
                    return Color.white;
            }
        }
        
        /// <summary>
        /// Restore the proper visual state based on current highlighting conditions
        /// </summary>
        private void RestoreProperVisualState()
        {
            if (debugVisualEffects)
                Debug.Log($"UnitSelectable {name}: Restoring visual state - Selected: {isSelected}, Attackable: {isAttackableHighlighted}, Defending: {isDefendingHighlighted}");
            
            // First, reset all visual effects to team color
            Color teamColor = GetCurrentTeamColor();
            
            // Reset base colors to team color
            foreach (var material in materials)
            {
                if (material != null)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", teamColor);
                    }
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", teamColor);
                    }
                }
            }
            
            // Restore emission to original
            foreach (var material in materials)
            {
                if (material != null && originalEmissionColors.ContainsKey(material))
                {
                    material.SetColor("_EmissionColor", originalEmissionColors[material]);
                    material.SetColor("_EmissiveColor", originalEmissionColors[material]);
                    
                    if (originalEmissionColors[material] == Color.black)
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
            
            // Apply defending tint first if unit is defending
            if (isDefendingHighlighted)
            {
                if (debugVisualEffects)
                    Debug.Log($"UnitSelectable {name}: Applying defending tint");
                EnableDefendingTint();
            }
            
            // Then apply emission effects on top
            if (isAttackableHighlighted)
            {
                if (debugVisualEffects)
                    Debug.Log($"UnitSelectable {name}: Applying attackable glow");
                EnableAttackableGlow();
            }
            else if (isSelected)
            {
                if (debugVisualEffects)
                    Debug.Log($"UnitSelectable {name}: Applying selection glow");
                EnableGlow();
            }
            
            if (debugVisualEffects)
                Debug.Log($"UnitSelectable {name}: Visual state restoration complete");
        }
        
        /// <summary>
        /// Called when mouse enters the unit - show hover info in SelectionInfoUI
        /// </summary>
        private void OnMouseEnter()
        {
            if (unit != null)
            {
                // Find SelectionInfoUI and show hover info
                var selectionUI = FindObjectOfType<SelectionInfoUI>();
                if (selectionUI != null)
                {
                    selectionUI.ShowUnitInfo(unit, isHover: true);
                }
                
                if (debugVisualEffects)
                    Debug.Log($"UnitSelectable {name}: Mouse entered, showing hover info");
            }
        }
        
        /// <summary>
        /// Called when mouse exits the unit - restore selection info or hide
        /// </summary>
        private void OnMouseExit()
        {
            if (unit != null)
            {
                // Find SelectionInfoUI and restore proper state
                var selectionUI = FindObjectOfType<SelectionInfoUI>();
                if (selectionUI != null)
                {
                    selectionUI.OnHoverExit();
                }
                
                if (debugVisualEffects)
                    Debug.Log($"UnitSelectable {name}: Mouse exited, restoring selection info");
            }
        }
        
        private void OnDestroy()
        {
            // Clean up when destroyed
            if (isSelected)
            {
                UnitSelectionManager.Instance?.DeselectUnit(this);
            }
        }
    }
} 