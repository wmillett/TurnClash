using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodexUISetup
{
    private RectTransform codexPanel;
    private GameObject menuPage;
    private GameObject entryPage;
    private Button toggleButton;
    private Transform categoryContainer;
    private Transform subcategoryContainer;
    private Button[] categoryButtons;
    private CodexPanelManager panelManager;
    private Button baseEntryButton;

    public Button BaseEntryButton => baseEntryButton;

    public CodexUISetup(
        RectTransform codexPanel,
        GameObject menuPage,
        GameObject entryPage,
        Button toggleButton,
        Transform categoryContainer,
        Transform subcategoryContainer,
        Button[] categoryButtons,
        CodexPanelManager panelManager)
    {
        this.codexPanel = codexPanel;
        this.menuPage = menuPage;
        this.entryPage = entryPage;
        this.toggleButton = toggleButton;
        this.categoryContainer = categoryContainer;
        this.subcategoryContainer = subcategoryContainer;
        this.categoryButtons = categoryButtons;
        this.panelManager = panelManager;

        baseEntryButton = subcategoryContainer.Find("BaseButton")?.GetComponent<Button>();
        if (baseEntryButton == null)
        {
            Debug.LogError("BaseButton template not found in SubcategoryContainer!");
        }
    }

    public void SetupUI()
    {
        SetupPanelPosition();
        SetupMenuPage();
        SetupEntryPage();
        SetupCategoryContainer();
        SetupSubcategoryContainer();
        SetupToggleButton();
        SetupInitialState();
    }

    private void SetupPanelPosition()
    {
        if (codexPanel != null)
        {
            // Use proper anchoring for slide-in animation
            // Anchor to center-right of screen (not stretching top to bottom)
            codexPanel.anchorMin = new Vector2(1, 0.5f);
            codexPanel.anchorMax = new Vector2(1, 0.5f);
            codexPanel.pivot = new Vector2(1, 0.5f);
            
            // Use Canvas-relative sizing that works with Canvas Scaler
            Canvas parentCanvas = codexPanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    // Calculate size based on canvas dimensions (works with Canvas Scaler)
                    float canvasWidth = canvasRect.rect.width;
                    float canvasHeight = canvasRect.rect.height;
                    
                    float panelWidth = canvasWidth * 0.35f; // 35% of canvas width (increased from 30%)
                    float panelHeight = canvasHeight * 0.6f; // 60% of canvas height (reduced from 80% - was too tall)
                    
                    codexPanel.sizeDelta = new Vector2(panelWidth, panelHeight);
                    
                    Debug.Log($"CodexUISetup: Panel sized to {panelWidth}x{panelHeight} (Canvas: {canvasWidth}x{canvasHeight})");
                }
                else
                {
                    // Fallback: use fixed size that scales with Canvas Scaler
                    codexPanel.sizeDelta = new Vector2(450, 450); // Increased width from 400 to 450
                    Debug.Log("CodexUISetup: Using fallback panel size 450x450");
                }
            }
            else
            {
                // Fallback: use fixed size that scales with Canvas Scaler
                codexPanel.sizeDelta = new Vector2(450, 450); // Increased width from 400 to 450
                Debug.Log("CodexUISetup: Using fallback panel size 450x450 (no canvas)");
            }
            
            // IMPORTANT: Position panel OFF-SCREEN initially for slide-in animation
            // Use anchoredPosition instead of position for UI elements
            codexPanel.anchoredPosition = new Vector2(codexPanel.sizeDelta.x, 0); // Move right by panel width to hide it
            
            Debug.Log($"CodexUISetup: Panel positioned off-screen at {codexPanel.anchoredPosition} for slide-in animation");
        }
    }

    private void SetupMenuPage()
    {
        if (menuPage != null)
        {
            RectTransform menuPageRect = menuPage.GetComponent<RectTransform>();
            if (menuPageRect != null)
            {
                // Use proper anchoring for responsive layout
                menuPageRect.anchorMin = new Vector2(0, 0);
                menuPageRect.anchorMax = new Vector2(1, 1);
                menuPageRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Use small margins to ensure content fits properly
                menuPageRect.offsetMin = new Vector2(5, 5); // Left, Bottom margins (smaller)
                menuPageRect.offsetMax = new Vector2(-5, -5); // Right, Top margins (smaller)
            }
            
            // Setup the MenuScrollView within MenuPage
            SetupMenuScrollView();
        }
    }

    /// <summary>
    /// Setup MenuScrollView with proper responsive anchoring and spacing
    /// </summary>
    private void SetupMenuScrollView()
    {
        if (menuPage == null) return;
        
        // Find MenuScrollView within MenuPage
        Transform menuScrollViewTransform = FindChildInDescendants(menuPage.transform, "MenuScrollView");
        if (menuScrollViewTransform != null)
        {
            RectTransform scrollViewRect = menuScrollViewTransform.GetComponent<RectTransform>();
            if (scrollViewRect != null)
            {
                // Anchor to fill the MenuPage but leave space for category buttons at top
                scrollViewRect.anchorMin = new Vector2(0, 0);
                scrollViewRect.anchorMax = new Vector2(1, 1);
                scrollViewRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Use margins - leave space at top for category buttons (60px) and small margins elsewhere
                scrollViewRect.offsetMin = new Vector2(10, 10); // Left, Bottom margins
                scrollViewRect.offsetMax = new Vector2(-10, -70); // Right, Top margins (more space at top for buttons)
                
                // Ensure ScrollRect component is properly configured
                ScrollRect scrollRect = menuScrollViewTransform.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    // Configure scroll rect for vertical scrolling
                    scrollRect.horizontal = false;
                    scrollRect.vertical = true;
                    scrollRect.movementType = ScrollRect.MovementType.Clamped;
                    scrollRect.scrollSensitivity = 20f;
                    
                    // Setup viewport if it exists
                    if (scrollRect.viewport != null)
                    {
                        RectTransform viewportRect = scrollRect.viewport;
                        viewportRect.anchorMin = new Vector2(0, 0);
                        viewportRect.anchorMax = new Vector2(1, 1);
                        viewportRect.offsetMin = Vector2.zero;
                        viewportRect.offsetMax = Vector2.zero;
                    }
                    
                    // Setup content if it exists
                    if (scrollRect.content != null)
                    {
                        RectTransform contentRect = scrollRect.content;
                        contentRect.anchorMin = new Vector2(0, 1);
                        contentRect.anchorMax = new Vector2(1, 1);
                        contentRect.pivot = new Vector2(0.5f, 1);
                        contentRect.anchoredPosition = Vector2.zero;
                        
                        // Add ContentSizeFitter for dynamic content sizing
                        ContentSizeFitter contentSizeFitter = contentRect.GetComponent<ContentSizeFitter>();
                        if (contentSizeFitter == null)
                        {
                            contentSizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
                        }
                        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        
                        // Add VerticalLayoutGroup for proper content organization
                        VerticalLayoutGroup contentLayoutGroup = contentRect.GetComponent<VerticalLayoutGroup>();
                        if (contentLayoutGroup == null)
                        {
                            contentLayoutGroup = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
                        }
                        contentLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                        contentLayoutGroup.spacing = 5; // Smaller spacing
                        contentLayoutGroup.padding = new RectOffset(5, 5, 5, 5); // Smaller padding
                        contentLayoutGroup.childControlWidth = true;
                        contentLayoutGroup.childControlHeight = false;
                        contentLayoutGroup.childForceExpandWidth = true;
                        contentLayoutGroup.childForceExpandHeight = false;
                    }
                }
            }
        }
    }

    public void SetupEntryPage()
    {
        if (entryPage != null)
        {
            RectTransform entryPageRect = entryPage.GetComponent<RectTransform>();
            if (entryPageRect != null)
            {
                // Use proper anchoring for responsive layout
                entryPageRect.anchorMin = new Vector2(0, 0);
                entryPageRect.anchorMax = new Vector2(1, 1);
                entryPageRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Use margins instead of fixed positioning for responsive design
                entryPageRect.offsetMin = new Vector2(10, 10); // Left, Bottom margins
                entryPageRect.offsetMax = new Vector2(-10, -10); // Right, Top margins
            }
            
            // Setup the BackButton at top-left
            SetupEntryBackButton();
            
            // Setup the Title at the top
            SetupEntryTitle();
            
            // Setup the Image in the middle
            SetupEntryImage();
            
            // Setup the EntryScrollView at the bottom half
            SetupEntryScrollView();
        }
    }

    public void SetupCategoryContainer()
    {
        if (categoryContainer != null)
        {
            RectTransform categoryRect = categoryContainer.GetComponent<RectTransform>();
            if (categoryRect != null)
            {
                // Use proper anchoring for responsive layout - anchor to top of panel
                categoryRect.anchorMin = new Vector2(0, 1);
                categoryRect.anchorMax = new Vector2(1, 1);
                categoryRect.pivot = new Vector2(0.5f, 1);
                
                // Position at the very top with small margins and reasonable height
                categoryRect.offsetMin = new Vector2(10, -40); // Left, Bottom margins (40px height for buttons)
                categoryRect.offsetMax = new Vector2(-10, -5); // Right, Top margins (small top margin)
                
                Debug.Log($"CodexUISetup: CategoryContainer positioned with height ~35px");
            }
            
            // Setup responsive button sizing
            ResizeCategoryButtons();
        }
    }

    public void SetupSubcategoryContainer()
    {
        if (subcategoryContainer != null)
        {
            RectTransform subcategoryRect = subcategoryContainer.GetComponent<RectTransform>();
            if (subcategoryRect != null)
            {
                // Use proper anchoring for responsive layout
                subcategoryRect.anchorMin = new Vector2(0, 0);
                subcategoryRect.anchorMax = new Vector2(1, 1);
                subcategoryRect.pivot = new Vector2(0.5f, 0.5f);
                
                // Use margins instead of fixed positioning for responsive design
                subcategoryRect.offsetMin = new Vector2(10, 70); // Left, Bottom margins
                subcategoryRect.offsetMax = new Vector2(-10, -10); // Right, Top margins
            }
            
            // Setup layout group for subcategory content
            SetupSubcategoryLayoutGroup();
        }
    }

    private void SetupSubcategoryLayoutGroup()
    {
        VerticalLayoutGroup layoutGroup = subcategoryContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = subcategoryContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // Configure layout group for entry buttons
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 8; // Reduced spacing between buttons
        layoutGroup.padding = new RectOffset(5, 5, 5, 5); // Reduced padding
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        
        ContentSizeFitter sizeFitter = subcategoryContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = subcategoryContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        // Configure content size fitter
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // Changed back to PreferredSize to accommodate all buttons
    }

    private void SetupToggleButton()
    {
        if (toggleButton != null)
        {
            ResetButtonPosition();
        }
    }

    private void SetupInitialState()
    {
        // Ensure panel starts hidden and inactive
        if (codexPanel != null)
        {
            codexPanel.gameObject.SetActive(false);
        }
        
        // Set initial pages state
        if (menuPage != null)
        {
            menuPage.SetActive(true);
        }
        
        if (entryPage != null)
        {
            entryPage.SetActive(false);
        }
        
        // Set initial category to Characters
        if (panelManager != null)
        {
            panelManager.OnCategorySelected(CodexCategory.Characters);
        }
    }

    public void SetButtonSize()
    {
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.SetParent(codexPanel);
        
        toggleRect.anchorMin = new Vector2(0, 0.5f);
        toggleRect.anchorMax = new Vector2(0, 0.5f);
        toggleRect.pivot = new Vector2(1, 0.5f);
        
        toggleRect.anchoredPosition = new Vector2(5, 0);
        toggleRect.sizeDelta = new Vector2(100, 100);
    }
    
    public void ResetButtonPosition()
    {
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.SetParent(codexPanel.parent);
        toggleRect.anchorMin = new Vector2(1, 0.5f);
        toggleRect.anchorMax = new Vector2(1, 0.5f);
        toggleRect.pivot = new Vector2(1, 0.5f);
        toggleRect.anchoredPosition = new Vector2(0, 0);
        toggleRect.sizeDelta = new Vector2(100, 30);
    }

    private void ResizeCategoryButtons()
    {
        // Set up the category container with proper layout
        if (categoryContainer != null)
        {
            // Add HorizontalLayoutGroup if it doesn't exist
            HorizontalLayoutGroup layoutGroup = categoryContainer.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = categoryContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            
            // Configure layout group for responsive button sizing
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 3; // Small spacing between buttons
            layoutGroup.padding = new RectOffset(5, 5, 3, 3); // Small padding
            layoutGroup.childControlHeight = false; // Don't force control height
            layoutGroup.childControlWidth = false;  // Don't force control width
            layoutGroup.childForceExpandHeight = false; // Don't force expand height
            layoutGroup.childForceExpandWidth = false;  // Don't force expand width
            
            // Remove ContentSizeFitter from container - we want fixed size
            ContentSizeFitter sizeFitter = categoryContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                UnityEngine.Object.Destroy(sizeFitter);
            }
            
            Debug.Log($"CodexUISetup: CategoryContainer layout configured with no force-expand");
        }
        
        // Configure individual buttons for reasonable sizing
        foreach (Button button in categoryButtons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Set compact size for buttons with increased height
                    buttonRect.sizeDelta = new Vector2(38, 25); // 38px wide, 25px tall
                    
                    // Add LayoutElement for size constraints
                    LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = button.gameObject.AddComponent<LayoutElement>();
                    }
                    
                    // Set compact fixed sizes with increased height
                    layoutElement.minWidth = 35;   // Minimum button width
                    layoutElement.minHeight = 22;  // Minimum button height
                    layoutElement.preferredWidth = 38;  // Preferred width
                    layoutElement.preferredHeight = 25; // Preferred height
                    layoutElement.flexibleWidth = 0;    // No flexible growth
                    layoutElement.flexibleHeight = 0;   // No flexible growth
                }
                
                // Configure button text for proper scaling
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    // Enable auto-sizing for text with better quality
                    buttonText.enableAutoSizing = true;
                    buttonText.fontSizeMin = 6;
                    buttonText.fontSizeMax = 10;
                    buttonText.fontSize = 8; // Base font size
                    
                    // Ensure text fits within button bounds
                    RectTransform textRect = buttonText.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        // Fill the button but with small margins
                        textRect.anchorMin = Vector2.zero;
                        textRect.anchorMax = Vector2.one;
                        textRect.offsetMin = new Vector2(2, 2);
                        textRect.offsetMax = new Vector2(-2, -2);
                    }
                }
                
                Debug.Log($"CodexUISetup: Button {button.name} configured with size 38x25");
            }
        }
    }

    /// <summary>
    /// Setup BackButton at top-left of EntryPage
    /// </summary>
    private void SetupEntryBackButton()
    {
        if (entryPage == null) return;
        
        // Find BackButton within EntryPage
        Transform backButtonTransform = FindChildInDescendants(entryPage.transform, "BackButton");
        if (backButtonTransform != null)
        {
            RectTransform backButtonRect = backButtonTransform.GetComponent<RectTransform>();
            if (backButtonRect != null)
            {
                // Anchor to top-left corner
                backButtonRect.anchorMin = new Vector2(0, 1);
                backButtonRect.anchorMax = new Vector2(0, 1);
                backButtonRect.pivot = new Vector2(0, 1);
                
                // Position at top-left with small margins
                backButtonRect.anchoredPosition = new Vector2(10, -10);
                backButtonRect.sizeDelta = new Vector2(80, 30); // Reasonable button size
                
                Debug.Log("CodexUISetup: BackButton positioned at top-left");
            }
        }
        else
        {
            Debug.LogWarning("CodexUISetup: BackButton not found in EntryPage");
        }
    }
    
    /// <summary>
    /// Setup Title at the top of EntryPage
    /// </summary>
    private void SetupEntryTitle()
    {
        if (entryPage == null) return;
        
        // Find Title within EntryPage
        Transform titleTransform = FindChildInDescendants(entryPage.transform, "Title");
        if (titleTransform != null)
        {
            RectTransform titleRect = titleTransform.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                // Anchor to top, spanning width
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                
                // Position below BackButton with margins
                titleRect.anchoredPosition = new Vector2(0, -50); // Below back button
                titleRect.sizeDelta = new Vector2(-20, 40); // Full width minus margins, reasonable height
                
                Debug.Log("CodexUISetup: Title positioned at top");
            }
        }
        else
        {
            Debug.LogWarning("CodexUISetup: Title not found in EntryPage");
        }
    }
    
    /// <summary>
    /// Setup Image in the middle of EntryPage
    /// </summary>
    private void SetupEntryImage()
    {
        if (entryPage == null) return;
        
        // Find Image within EntryPage
        Transform imageTransform = FindChildInDescendants(entryPage.transform, "Image");
        if (imageTransform != null)
        {
            RectTransform imageRect = imageTransform.GetComponent<RectTransform>();
            if (imageRect != null)
            {
                // Anchor to center-top area
                imageRect.anchorMin = new Vector2(0, 0.5f);
                imageRect.anchorMax = new Vector2(1, 1);
                imageRect.pivot = new Vector2(0.5f, 1);
                
                // Position in the upper middle area
                imageRect.anchoredPosition = new Vector2(0, -100); // Below title
                imageRect.sizeDelta = new Vector2(-40, -120); // Full width minus margins, flexible height
                
                Debug.Log("CodexUISetup: Image positioned in middle area");
            }
        }
        else
        {
            Debug.LogWarning("CodexUISetup: Image not found in EntryPage");
        }
    }
    
    /// <summary>
    /// Setup EntryScrollView to take bottom half of EntryPage
    /// </summary>
    private void SetupEntryScrollView()
    {
        if (entryPage == null) return;
        
        // Find EntryScrollView within EntryPage
        Transform entryScrollViewTransform = FindChildInDescendants(entryPage.transform, "EntryScrollView");
        if (entryScrollViewTransform != null)
        {
            RectTransform scrollViewRect = entryScrollViewTransform.GetComponent<RectTransform>();
            if (scrollViewRect != null)
            {
                // Anchor to bottom half of the panel
                scrollViewRect.anchorMin = new Vector2(0, 0);
                scrollViewRect.anchorMax = new Vector2(1, 0.5f);
                scrollViewRect.pivot = new Vector2(0.5f, 0);
                
                // Use margins for responsive spacing
                scrollViewRect.offsetMin = new Vector2(15, 15); // Left, Bottom margins
                scrollViewRect.offsetMax = new Vector2(-15, -15); // Right, Top margins
                
                // Ensure ScrollRect component is properly configured
                ScrollRect scrollRect = entryScrollViewTransform.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    // Configure scroll rect for vertical scrolling
                    scrollRect.horizontal = false;
                    scrollRect.vertical = true;
                    scrollRect.movementType = ScrollRect.MovementType.Clamped;
                    scrollRect.scrollSensitivity = 20f;
                    
                    // Setup viewport if it exists
                    if (scrollRect.viewport != null)
                    {
                        RectTransform viewportRect = scrollRect.viewport;
                        viewportRect.anchorMin = new Vector2(0, 0);
                        viewportRect.anchorMax = new Vector2(1, 1);
                        viewportRect.offsetMin = Vector2.zero;
                        viewportRect.offsetMax = Vector2.zero;
                    }
                    
                    // Setup content if it exists
                    if (scrollRect.content != null)
                    {
                        RectTransform contentRect = scrollRect.content;
                        contentRect.anchorMin = new Vector2(0, 1);
                        contentRect.anchorMax = new Vector2(1, 1);
                        contentRect.pivot = new Vector2(0.5f, 1);
                        contentRect.anchoredPosition = Vector2.zero;
                        
                        // Add ContentSizeFitter for dynamic content sizing
                        ContentSizeFitter contentSizeFitter = contentRect.GetComponent<ContentSizeFitter>();
                        if (contentSizeFitter == null)
                        {
                            contentSizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
                        }
                        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        
                        // Add VerticalLayoutGroup for proper content organization
                        VerticalLayoutGroup contentLayoutGroup = contentRect.GetComponent<VerticalLayoutGroup>();
                        if (contentLayoutGroup == null)
                        {
                            contentLayoutGroup = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
                        }
                        contentLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                        contentLayoutGroup.spacing = 10;
                        contentLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
                        contentLayoutGroup.childControlWidth = true;
                        contentLayoutGroup.childControlHeight = false;
                        contentLayoutGroup.childForceExpandWidth = true;
                        contentLayoutGroup.childForceExpandHeight = false;
                    }
                }
                
                Debug.Log("CodexUISetup: EntryScrollView configured to take bottom half");
            }
        }
        else
        {
            Debug.LogWarning("CodexUISetup: EntryScrollView not found in EntryPage");
        }
    }
    
    /// <summary>
    /// Helper method to find child objects recursively
    /// </summary>
    private Transform FindChildInDescendants(Transform parent, string name)
    {
        // Check direct children first
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
                return child;
        }
        
        // Check grandchildren recursively
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform found = FindChildInDescendants(child, name);
            if (found != null)
                return found;
        }
        
        return null;
    }

    /// <summary>
    /// Debug method to check panel positioning
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPanelPosition()
    {
        if (codexPanel != null)
        {
            Debug.Log($"=== CODEX PANEL DEBUG ===");
            Debug.Log($"Panel Size: {codexPanel.sizeDelta}");
            Debug.Log($"Panel Position: {codexPanel.anchoredPosition}");
            Debug.Log($"Panel Rect: {codexPanel.rect}");
            Debug.Log($"Panel Active: {codexPanel.gameObject.activeInHierarchy}");
            Debug.Log($"Panel Anchors: Min={codexPanel.anchorMin}, Max={codexPanel.anchorMax}");
            Debug.Log($"Panel Pivot: {codexPanel.pivot}");
            Debug.Log($"========================");
        }
    }
} 