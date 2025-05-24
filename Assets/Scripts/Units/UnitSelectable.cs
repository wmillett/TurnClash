using UnityEngine;
using System.Collections.Generic;
using TurnClash.Units;

namespace TurnClash.Units
{
    [RequireComponent(typeof(Unit))]
    public class UnitSelectable : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0f, 1f); // Golden glow
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private bool useEmission = true;
        
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
            // Handle unit selection when clicked
            UnitSelectionManager.Instance?.SelectUnit(this);
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