using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TurnClash.UI
{
    /// <summary>
    /// Helper script to automatically create Selection UI components
    /// Can work with existing SelectionPanel or create a new one
    /// </summary>
    public class SelectionUISetup : MonoBehaviour
    {
        [Header("UI Setup")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private GameObject existingSelectionPanel; // NEW: Use existing panel
        [SerializeField] private Vector2 panelSize = new Vector2(300, 180); // Increased height for health bar
        [SerializeField] private Vector2 panelPosition = new Vector2(-10, 10); // Bottom-left offset
        
        [Space]
        [Header("Auto Setup")]
        [SerializeField] private bool createUIOnStart = false;
        [SerializeField] private bool useExistingPanel = true; // NEW: Toggle for existing vs new panel
        
        private void Start()
        {
            Debug.Log($"SelectionUISetup: Start() called with createUIOnStart = {createUIOnStart}");
            
            // SAFETY CHECK: Don't overwrite existing properly configured UI
            if (createUIOnStart)
            {
                // Check if there's already a properly configured SelectionInfoUI
                var existingUI = FindObjectOfType<SelectionInfoUI>();
                if (existingUI != null)
                {
                    // Check if it has references assigned (indicating manual setup)
                    var panelField = typeof(SelectionInfoUI).GetField("selectionPanel", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var panel = panelField?.GetValue(existingUI);
                    
                    if (panel != null)
                    {
                        Debug.LogWarning("SelectionUISetup: Found existing SelectionInfoUI with assigned references. Skipping auto-creation to preserve manual setup.");
                        Debug.LogWarning("SelectionUISetup: If you want to recreate the UI, manually call CreateSelectionUI() or disable the existing SelectionInfoUI component first.");
                        return;
                    }
                }
                
                Debug.Log("SelectionUISetup: Auto-creating UI on start - this will OVERWRITE existing references!");
                CreateSelectionUI();
            }
            else
            {
                Debug.Log("SelectionUISetup: NOT creating UI on start (createUIOnStart = false)");
            }
        }
        
        [ContextMenu("Setup Selection UI")]
        public void CreateSelectionUI()
        {
            GameObject panelGO;
            
            if (useExistingPanel && existingSelectionPanel != null)
            {
                // Use existing panel
                panelGO = existingSelectionPanel;
                Debug.Log("Using existing SelectionPanel: " + panelGO.name);
                
                // Clear existing children if any
                ClearExistingChildren(panelGO);
            }
            else
            {
                // Create new panel (original functionality)
                panelGO = CreateNewPanel();
            }
            
            // Create UI elements
            CreateUIElements(panelGO);
            
            // Add or update SelectionInfoUI component
            SetupSelectionInfoComponent(panelGO);
            
            Debug.Log("Selection UI setup completed successfully!");
        }
        
        private GameObject CreateNewPanel()
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
            
            return panelGO;
        }
        
        private void ClearExistingChildren(GameObject panel)
        {
            // Clear existing UI elements if we're setting up an existing panel
            for (int i = panel.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(panel.transform.GetChild(i).gameObject);
            }
        }
        
        private void CreateUIElements(GameObject panelGO)
        {
            // Create text elements with updated layout for health bar
            CreateTextElement(panelGO, "UnitNameText", "Unit Name", new Vector2(10, -25), new Vector2(280, 30), 18, FontStyles.Bold);
            
            // Create Health Bar
            CreateHealthBar(panelGO, "HealthBar", new Vector2(10, -55), new Vector2(280, 20));
            
            // Create Health Text (shows current/max values)
            CreateTextElement(panelGO, "HealthText", "Health: --/--", new Vector2(10, -80), new Vector2(280, 20), 12, FontStyles.Normal);
            
            // Create Attack and defence stats
            CreateTextElement(panelGO, "AttackText", "ATK: --", new Vector2(10, -105), new Vector2(140, 25), 14, FontStyles.Normal);
            CreateTextElement(panelGO, "DefenseText", "DEF: --", new Vector2(150, -105), new Vector2(140, 25), 14, FontStyles.Normal);
        }
        
        private GameObject CreateHealthBar(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            // Create health bar container
            GameObject healthBarGO = new GameObject(name);
            healthBarGO.transform.SetParent(parent.transform, false);
            
            // Add RectTransform component (required for UI elements)
            RectTransform healthBarRect = healthBarGO.AddComponent<RectTransform>();
            
            // Set position and size
            healthBarRect.anchorMin = new Vector2(0, 1); // Top-left anchor
            healthBarRect.anchorMax = new Vector2(0, 1);
            healthBarRect.pivot = new Vector2(0, 1);
            healthBarRect.anchoredPosition = position;
            healthBarRect.sizeDelta = size;
            
            // Add Slider component
            Slider healthSlider = healthBarGO.AddComponent<Slider>();
            healthSlider.interactable = false; // Make it non-interactive
            healthSlider.transition = Selectable.Transition.None;
            
            // Create Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(healthBarGO.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray background
            
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Create Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(healthBarGO.transform, false);
            
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Create Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.green; // Health color
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Configure slider
            healthSlider.fillRect = fillRect;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            
            return healthBarGO;
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
        
        private void SetupSelectionInfoComponent(GameObject panelGO)
        {
            // Remove existing SelectionInfoUI if present
            SelectionInfoUI existingComponent = panelGO.GetComponent<SelectionInfoUI>();
            if (existingComponent != null)
            {
                DestroyImmediate(existingComponent);
            }
            
            // Add new SelectionInfoUI component
            SelectionInfoUI selectionUI = panelGO.AddComponent<SelectionInfoUI>();
            
            // Auto-assign references
            AutoAssignUIReferences(selectionUI, panelGO);
        }
        
        private void AutoAssignUIReferences(SelectionInfoUI selectionUI, GameObject panelGO)
        {
            Debug.Log("SelectionUISetup: Auto-assigning UI references...");
            
            // Use reflection to assign the UI references automatically
            var fields = typeof(SelectionInfoUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.Name == "selectionPanel")
                {
                    field.SetValue(selectionUI, panelGO);
                    Debug.Log($"SelectionUISetup: Assigned selectionPanel = {panelGO.name}");
                }
                else if (field.Name == "unitNameText")
                {
                    var textComponent = panelGO.transform.Find("UnitNameText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                    Debug.Log($"SelectionUISetup: Assigned unitNameText = {(textComponent != null ? "✓" : "✗ NOT FOUND")}");
                }
                else if (field.Name == "healthText")
                {
                    var textComponent = panelGO.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                    Debug.Log($"SelectionUISetup: Assigned healthText = {(textComponent != null ? "✓" : "✗ NOT FOUND")}");
                }
                else if (field.Name == "healthBar")
                {
                    var healthBar = panelGO.transform.Find("HealthBar")?.GetComponent<Slider>();
                    field.SetValue(selectionUI, healthBar);
                    Debug.Log($"SelectionUISetup: Assigned healthBar = {(healthBar != null ? "✓" : "✗ NOT FOUND")}");
                }
                else if (field.Name == "attackText")
                {
                    var textComponent = panelGO.transform.Find("AttackText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                    Debug.Log($"SelectionUISetup: Assigned attackText = {(textComponent != null ? "✓" : "✗ NOT FOUND")}");
                }
                else if (field.Name == "defenseText")
                {
                    var textComponent = panelGO.transform.Find("DefenseText")?.GetComponent<TextMeshProUGUI>();
                    field.SetValue(selectionUI, textComponent);
                    Debug.Log($"SelectionUISetup: Assigned defenseText = {(textComponent != null ? "✓" : "✗ NOT FOUND")}");
                }
            }
            
            Debug.Log("SelectionUISetup: UI references auto-assignment completed.");
            
            // Verify all references were assigned
            Debug.Log("SelectionUISetup: Verifying assigned references...");
            var selectionPanelField = typeof(SelectionInfoUI).GetField("selectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unitNameTextField = typeof(SelectionInfoUI).GetField("unitNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var healthTextField = typeof(SelectionInfoUI).GetField("healthText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var healthBarField = typeof(SelectionInfoUI).GetField("healthBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var attackTextField = typeof(SelectionInfoUI).GetField("attackText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defenseTextField = typeof(SelectionInfoUI).GetField("defenseText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Debug.Log($"Final verification:");
            Debug.Log($"  - selectionPanel: {(selectionPanelField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
            Debug.Log($"  - unitNameText: {(unitNameTextField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
            Debug.Log($"  - healthText: {(healthTextField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
            Debug.Log($"  - healthBar: {(healthBarField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
            Debug.Log($"  - attackText: {(attackTextField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
            Debug.Log($"  - defenseText: {(defenseTextField?.GetValue(selectionUI) != null ? "✓" : "✗")}");
        }
    }
} 