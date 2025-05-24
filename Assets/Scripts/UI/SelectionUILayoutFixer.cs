using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TurnClash.UI
{
    /// <summary>
    /// Script to fix the layout of SelectionPanel UI elements using Unity's Layout system
    /// Based on Unity UI documentation for proper element positioning
    /// </summary>
    public class SelectionUILayoutFixer : MonoBehaviour
    {
        [Header("Target Panel")]
        [SerializeField] private GameObject selectionPanel;
        
        [Header("Auto-fix Options")]
        [SerializeField] private bool autoFixOnStart = true;
        [SerializeField] private bool addLayoutComponents = true; // Now only removes automatic layout components
        
        private void Start()
        {
            if (autoFixOnStart)
            {
                FixUILayout();
            }
        }
        
        [ContextMenu("Fix UI Layout")]
        public void FixUILayout()
        {
            if (selectionPanel == null)
            {
                selectionPanel = GameObject.Find("SelectionPanel");
                if (selectionPanel == null)
                {
                    Debug.LogError("SelectionUILayoutFixer: No SelectionPanel found!");
                    return;
                }
            }
            
            Debug.Log("SelectionUILayoutFixer: Starting UI layout fix...");
            
            // Step 1: Set up the main panel
            SetupMainPanel();
            
            // Step 2: Add layout components if requested
            if (addLayoutComponents)
            {
                AddLayoutComponents();
            }
            
            // REMOVED: Element positioning code - user has positioned elements manually
            // if (repositionElements)
            // {
            //     PositionUIElements();
            // }
            
            Debug.Log("SelectionUILayoutFixer: UI layout fix completed - manual positioning preserved!");
        }
        
        private void SetupMainPanel()
        {
            RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
            if (panelRect == null) return;
            
            // PRESERVE all user settings from inspector - no size modifications
            // The user has manually set the height to 150px in the inspector
            
            // Ensure panel has a background (Image component)
            Image panelImage = selectionPanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = selectionPanel.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent
            }
            
            Debug.Log($"SelectionUILayoutFixer: Panel setup completed - all inspector settings preserved");
        }
        
        private void AddLayoutComponents()
        {
            // REMOVED: VerticalLayoutGroup and ContentSizeFitter to preserve user's manual positioning
            // The user has manually positioned all UI elements and wants to keep their layout
            
            // Remove any existing layout components that might interfere
            VerticalLayoutGroup layoutGroup = selectionPanel.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                DestroyImmediate(layoutGroup);
                Debug.Log("SelectionUILayoutFixer: Removed VerticalLayoutGroup to preserve manual positioning");
            }
            
            ContentSizeFitter sizeFitter = selectionPanel.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                DestroyImmediate(sizeFitter);
                Debug.Log("SelectionUILayoutFixer: Removed ContentSizeFitter to preserve manual sizing");
            }
            
            Debug.Log("SelectionUILayoutFixer: All automatic layout components removed - manual positioning preserved");
        }
        
        [ContextMenu("Reset Layout")]
        public void ResetLayout()
        {
            if (selectionPanel == null) return;
            
            // Remove layout components to start fresh
            VerticalLayoutGroup layoutGroup = selectionPanel.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null) DestroyImmediate(layoutGroup);
            
            ContentSizeFitter sizeFitter = selectionPanel.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null) DestroyImmediate(sizeFitter);
            
            // Remove any LayoutElement components that might have been added to children
            foreach (Transform child in selectionPanel.transform)
            {
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement != null) DestroyImmediate(layoutElement);
            }
            
            Debug.Log("SelectionUILayoutFixer: All layout components reset - ready for manual positioning");
        }
    }
} 