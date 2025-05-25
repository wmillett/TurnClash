using UnityEngine;
using System.Collections.Generic;
using TurnClash.Units;

namespace TurnClash.Units
{
    [RequireComponent(typeof(Unit))]
    public class UnitSelectable : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0f, 1f); // Golden glow for selection
        [SerializeField] private Color attackableColor = new Color(1f, 0.2f, 0.2f, 1f); // Red glow for attackable enemies
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private bool useEmission = true;
        
        // Visual feedback state
        private bool isAttackableHighlighted = false;
        
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
                DisableGlow();
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
                // If unit is selected, restore selection glow, otherwise restore original
                if (isSelected)
                {
                    EnableGlow();
                }
                else
                {
                    DisableGlow();
                }
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