using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TurnClash.UI
{
    /// <summary>
    /// Helper script to automatically create Selection UI components
    /// Attach this to an empty GameObject and click "Create Selection UI" in the inspector
    /// </summary>
    public class SelectionUISetup : MonoBehaviour
    {
        [Header("UI Setup")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Vector2 panelSize = new Vector2(300, 150);
        [SerializeField] private Vector2 panelPosition = new Vector2(-10, 10); // Bottom-left offset
        
        [Space]
        [Header("Auto Setup")]
        [SerializeField] private bool createUIOnStart = false;
        
        private void Start()
        {
            if (createUIOnStart)
            {
                CreateSelectionUI();
            }
        }
        
        [ContextMenu("Create Selection UI")]
        public void CreateSelectionUI()
        {
            // Find or create canvas
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
                if (targetCanvas == null)
                {
                    GameObject canvasGO = new GameObject("GameUI");
                    targetCanvas = canvasGO.AddComponent<Canvas>();
                    targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    
                    // Add Canvas Scaler
                    CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    
                    // Add Graphic Raycaster
                    canvasGO.AddComponent<GraphicRaycaster>();
                }
            }
            
            // Create Selection Panel
            GameObject panelGO = new GameObject("SelectionPanel");
            panelGO.transform.SetParent(targetCanvas.transform, false);
            
            // Add Image component for panel background
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
            
            // Set panel position and size
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0); // Bottom-left anchor
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.anchoredPosition = panelPosition;
            panelRect.sizeDelta = panelSize;
            
            // Create text elements
            CreateTextElement(panelGO, "UnitNameText", "Unit Name", new Vector2(10, -30), new Vector2(280, 30), 18, FontStyles.Bold);
            CreateTextElement(panelGO, "HealthText", "Health: --/--", new Vector2(10, -60), new Vector2(280, 25), 14, FontStyles.Normal);
            CreateTextElement(panelGO, "PlayerText", "Player: --", new Vector2(10, -85), new Vector2(280, 25), 14, FontStyles.Normal);
            CreateTextElement(panelGO, "PositionText", "Position: (--, --)", new Vector2(10, -110), new Vector2(280, 25), 14, FontStyles.Normal);
            
            // Add SelectionInfoUI component
            SelectionInfoUI selectionUI = panelGO.AddComponent<SelectionInfoUI>();
            
            // Auto-assign references using reflection or manual assignment
            AutoAssignUIReferences(selectionUI, panelGO);
            
            Debug.Log("Selection UI created successfully! Check the Canvas for the new SelectionPanel.");
        }
        
        private GameObject CreateTextElement(GameObject parent, string name, string text, Vector2 position, Vector2 size, int fontSize, FontStyles fontStyle)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);
            
            // Add TextMeshPro component
            TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = Color.white;
            
            // Set text position and size
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1); // Top-left anchor
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = position;
            textRect.sizeDelta = size;
            
            return textGO;
        }
        
        private void AutoAssignUIReferences(SelectionInfoUI selectionUI, GameObject panelGO)
        {
            // Use reflection to assign the UI references automatically
            var fields = typeof(SelectionInfoUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.Name == "selectionPanel")
                {
                    field.SetValue(selectionUI, panelGO);
                }
                else if (field.Name == "unitNameText")
                {
                    var textComponent = panelGO.transform.Find("UnitNameText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                }
                else if (field.Name == "healthText")
                {
                    var textComponent = panelGO.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                }
                else if (field.Name == "playerText")
                {
                    var textComponent = panelGO.transform.Find("PlayerText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                }
                else if (field.Name == "positionText")
                {
                    var textComponent = panelGO.transform.Find("PositionText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                }
            }
            
            Debug.Log("UI references auto-assigned to SelectionInfoUI component.");
        }
    }
} 