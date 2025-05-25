using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;
using TurnClash.Units;

public class CodexPanelManager : MonoBehaviour
{
    // UI References
    private RectTransform codexPanel;
    private GameObject menuPage;
    private GameObject entryPage;
    private Button toggleButton;
    private Button backButton;
    private Transform categoryContainer;
    [SerializeField] private Transform subcategoryContainer;
    
    // Entry Page References
    private TextMeshProUGUI entryTitleText;
    private TextMeshProUGUI entryDescriptionText;
    private Image entryImage;
    
    // Scroll View References
    private CodexScrollView menuScrollView;
    private CodexScrollView entryScrollView;
    private RectTransform entryPageScrollableContent; // Container for entry page's scrollable items
    
    // Data
    private TextAsset codexJsonFile;
    
    // Animation Settings
    private float slideDuration = 0.3f;
    private Ease slideEase = Ease.OutQuad;
    
    private CodexData codexData;
    private CodexCategory currentCategory;
    private string currentSubcategory;
    private CodexEntry currentEntry;
    private bool isPanelVisible = false;
    private Dictionary<string, bool> subcategoryExpanded = new Dictionary<string, bool>();
    private CameraMovement panelMovement;
    private CodexUISetup uiSetup;

    // Custom category button names
    private readonly Dictionary<string, CodexCategory> categoryButtonMap = new Dictionary<string, CodexCategory>
    {
        { "characters", CodexCategory.Characters },
        { "history", CodexCategory.History },
        { "events", CodexCategory.History } // Assuming "events" is a subcategory of History
    };
    
    // Category Buttons (assigned in Inspector)
    [SerializeField] private Button charactersButton;
    [SerializeField] private Button historyButton;
    [SerializeField] private Button locationsButton;
    [SerializeField] private Button organizationsButton;
    
    private void Start()
    {
        FindReferences();
        LoadCodexData();
        InitializeUISetup();
        SetupEventListeners();
    }
    
    private void FindReferences()
    {
        // Find main panel
        codexPanel = GetComponent<RectTransform>();
        if (codexPanel == null)
        {
            Debug.LogError("CodexPanelManager must be on a GameObject with RectTransform!");
            return;
        }

        // Get camera movement component
        panelMovement = Camera.main.GetComponent<CameraMovement>();
        if (panelMovement == null)
        {
            Debug.LogError("CameraMovement component not found on Main Camera!");
        }

        // Find UI elements
        menuPage = FindChildByName("MenuPage");
        entryPage = FindChildByName("EntryPage");
        categoryContainer = FindChildByName("CategoryContainer")?.transform;
        
        // Remove the code that searches for SubcategoryContainer since it will be set in Inspector
        if (subcategoryContainer == null)
        {
            Debug.LogError("SubcategoryContainer not assigned in Inspector!");
        }
        
        toggleButton = FindChildByName("ToggleButton")?.GetComponent<Button>();
        backButton = FindChildByName("BackButton")?.GetComponent<Button>();

        // Find entry page elements
        if (entryPage != null)
        {
            // Find these elements within EntryPage initially
            entryTitleText = FindChildInDescendants(entryPage.transform, "Title")?.GetComponent<TextMeshProUGUI>();
            entryDescriptionText = FindChildInDescendants(entryPage.transform, "Description")?.GetComponent<TextMeshProUGUI>();
            entryImage = FindChildInDescendants(entryPage.transform, "Image")?.GetComponent<Image>();
            entryScrollView = FindChildInDescendants(entryPage.transform, "EntryScrollView")?.GetComponent<CodexScrollView>();
            // // Find or set up EntryScrollView
            // Transform entryScrollViewTransform = entryPage.transform.Find("EntryScrollView");
            // if (entryScrollViewTransform != null)
            // {
            //     entryScrollView = entryScrollViewTransform.GetComponent<CodexScrollView>();
            //     if (entryScrollView == null)
            //     {
            //         entryScrollView = entryScrollViewTransform.gameObject.AddComponent<CodexScrollView>();
            //     }
                
            //     // Setup container for scrollable entry content
            //     Transform existingScrollContainer = entryPage.transform.Find("EntryScrollContent");
            //     if (existingScrollContainer != null) {
            //         entryPageScrollableContent = existingScrollContainer.GetComponent<RectTransform>();
            //     } else {
            //         GameObject containerObj = new GameObject("EntryScrollContent");
            //         entryPageScrollableContent = containerObj.AddComponent<RectTransform>();
            //         containerObj.transform.SetParent(entryPage.transform, false);
                    
            //         // Set up EntryScrollContent layout
            //         entryPageScrollableContent.anchorMin = new Vector2(0, 0);
            //         entryPageScrollableContent.anchorMax = new Vector2(1, 1);
            //         entryPageScrollableContent.pivot = new Vector2(0.5f, 0.5f);
            //         entryPageScrollableContent.offsetMin = new Vector2(10, 10);
            //         entryPageScrollableContent.offsetMax = new Vector2(-10, -10);
                    
            //         VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
            //         vlg.padding = new RectOffset(15, 15, 15, 15);
            //         vlg.spacing = 10;
            //         vlg.childControlWidth = true;
            //         vlg.childForceExpandWidth = true;
            //         vlg.childAlignment = TextAnchor.UpperCenter;
                    
            //         ContentSizeFitter csf = containerObj.AddComponent<ContentSizeFitter>();
            //         csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            //     }
                
            //     // Reparent elements to EntryScrollContent and place it in EntryScrollView.content
            //     if (entryPageScrollableContent != null && entryScrollView.content != null)
            //     {
            //         // Move EntryScrollContent to be a child of the scroll view's content
            //         entryPageScrollableContent.SetParent(entryScrollView.content, false);
                    
            //         // Move UI elements to EntryScrollContent if they're not already there
            //         if (entryTitleText != null && entryTitleText.transform.parent != entryPageScrollableContent)
            //             entryTitleText.transform.SetParent(entryPageScrollableContent, false);
                    
            //         if (entryDescriptionText != null && entryDescriptionText.transform.parent != entryPageScrollableContent)
            //             entryDescriptionText.transform.SetParent(entryPageScrollableContent, false);
                    
            //         if (entryImage != null && entryImage.transform.parent != entryPageScrollableContent)
            //             entryImage.transform.SetParent(entryPageScrollableContent, false);
            //     }
            // }
            // else
            // {
            //     Debug.LogError("EntryScrollView not found as a child of EntryPage!");
            // }
        }
        
        // Find MenuScrollView as a child of MenuPage
        if (menuPage != null)
        {
            Transform menuScrollViewTransform = menuPage.transform.Find("MenuScrollView");
            if (menuScrollViewTransform != null)
            {
                menuScrollView = menuScrollViewTransform.GetComponent<CodexScrollView>();
                if (menuScrollView == null)
                {
                    Debug.LogWarning("CodexScrollView component not found on MenuScrollView, adding one.");
                    menuScrollView = menuScrollViewTransform.gameObject.AddComponent<CodexScrollView>();
                }
                
                // Move subcategoryContainer to MenuScrollView's content
                if (subcategoryContainer != null && menuScrollView.content != null)
                {
                    subcategoryContainer.SetParent(menuScrollView.content, false);
                    
                    // Setup proper layout
                    RectTransform subcategoryRect = subcategoryContainer.GetComponent<RectTransform>();
                    if (subcategoryRect != null)
                    {
                        subcategoryRect.anchorMin = new Vector2(0, 1);
                        subcategoryRect.anchorMax = new Vector2(1, 1);
                        subcategoryRect.pivot = new Vector2(0.5f, 1);
                        subcategoryRect.anchoredPosition = Vector2.zero;
                        subcategoryRect.offsetMin = new Vector2(10, 0);
                        subcategoryRect.offsetMax = new Vector2(-10, 0);
                    }
                }
            }
            else
            {
                Debug.LogError("MenuScrollView GameObject not found as a child of MenuPage!");
            }
        }

        // Load JSON file from Resources
        codexJsonFile = Resources.Load<TextAsset>("CodexData");
        if (codexJsonFile == null)
        {
            Debug.LogError("CodexData.json not found in Resources folder!");
        }
        
        // Verify references
        VerifyReferences();
    }

    private GameObject FindChildByName(string name)
    {
        // First try direct child
        Transform child = transform.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        // If not found, search recursively
        foreach (Transform t in transform)
        {
            child = FindChildRecursive(t, name);
            if (child != null)
            {
                return child.gameObject;
            }
        }

        Debug.LogWarning($"Could not find child '{name}' in {gameObject.name} or its children");
        return null;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        // Check if this is the child we're looking for
        if (parent.name == name)
        {
            return parent;
        }

        // Search through all children
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private Transform FindChildInDescendants(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildInDescendants(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void LogMissingComponents()
    {
        if (menuPage == null) Debug.LogError("MenuPage not found!");
        if (entryPage == null) Debug.LogError("EntryPage not found!");
        if (categoryContainer == null) Debug.LogError("CategoryContainer not found!");
        if (subcategoryContainer == null) Debug.LogError("SubcategoryContainer not found!");
        if (toggleButton == null) Debug.LogError("ToggleButton not found!");
        if (backButton == null) Debug.LogError("BackButton not found!");
        if (entryTitleText == null) Debug.LogError("EntryTitleText not found!");
        if (entryDescriptionText == null) Debug.LogError("EntryDescriptionText not found!");
        if (entryImage == null) Debug.LogError("EntryImage not found!");
    }
    
    private void LoadCodexData()
    {
        if (codexJsonFile != null)
        {
            codexData = JsonUtility.FromJson<CodexData>(codexJsonFile.text);
        }
        else
        {
            // Create empty codex data if no JSON file is assigned
            codexData = new CodexData
            {
                entries = new List<CodexEntry>()
            };
            Debug.LogWarning("No Codex JSON file found. Using empty codex data.");
        }
    }
    
    private void InitializeUISetup()
    {
        Button[] categoryButtons = { charactersButton, historyButton, locationsButton, organizationsButton };
        uiSetup = new CodexUISetup(
            codexPanel,
            menuPage,
            entryPage,
            toggleButton,
            categoryContainer,
            subcategoryContainer,
            categoryButtons,
            this
        );
        uiSetup.SetupUI();
    }
    
    private void SetupEventListeners()
    {
        toggleButton.onClick.AddListener(TogglePanel);
        backButton.onClick.AddListener(ShowMenuPage);
        
        // Wire up real category buttons
        if (charactersButton != null)
            charactersButton.onClick.AddListener(() => OnCategorySelected(CodexCategory.Characters));
        if (historyButton != null)
            historyButton.onClick.AddListener(() => OnCategorySelected(CodexCategory.History));
        if (locationsButton != null)
            locationsButton.onClick.AddListener(() => OnCategorySelected(CodexCategory.Locations));
        if (organizationsButton != null)
            organizationsButton.onClick.AddListener(() => OnCategorySelected(CodexCategory.Organizations));
    }
    
    /// <summary>
    /// Refresh UI layout - useful when aspect ratio changes
    /// Only refresh if panel is not currently animating
    /// </summary>
    public void RefreshUILayout()
    {
        // Don't refresh during animation or when panel is hidden
        if (uiSetup != null && isPanelVisible && codexPanel.gameObject.activeInHierarchy)
        {
            // Only refresh the internal layout, not the panel position
            // to avoid interfering with slide animations
            uiSetup.SetupEntryPage();
            uiSetup.SetupCategoryContainer();
            uiSetup.SetupSubcategoryContainer();
            Debug.Log("CodexPanelManager: UI layout refreshed for aspect ratio change (partial refresh)");
        }
    }
    
    public void TogglePanel()
    {
        // Check if game is over - don't allow codex access when victory screen is active
        if (IsGameOver())
        {
            Debug.Log("CodexPanelManager: Cannot toggle codex panel - game is over (victory screen active)");
            return;
        }
        
        Debug.Log("Toggle button clicked");
        isPanelVisible = !isPanelVisible;

        if (isPanelVisible)
        {
            codexPanel.gameObject.SetActive(true);

            // Force scale to be exactly 1 (important for Canvas Scaler compatibility)
            codexPanel.localScale = Vector3.one;

            // DON'T call SetupUI() here - it interferes with the animation positioning
            // The UI should already be set up from the initial setup
            
            // Ensure the panel starts at the correct off-screen position before animating
            codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0);

            // Ensure toggle button is a child of the panel
            if (toggleButton != null)
            {
                uiSetup.SetButtonSize();
            }

            // Animate panel sliding in from right
            codexPanel.DOAnchorPosX(0, slideDuration)
                .SetEase(slideEase)
                .OnStart(() => {
                    if (panelMovement != null)
                    {
                        panelMovement.SetEnabled(false);
                    }
                });
        }
        else
        {
            // Show menu page before closing
            ShowMenuPage();
            
            // Animate panel sliding out to the right
            codexPanel.DOAnchorPosX(codexPanel.rect.width, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => {
                    // Move toggle button back to canvas level after animation
                    if (toggleButton != null)
                    {
                        uiSetup.ResetButtonPosition();
                    }

                    codexPanel.gameObject.SetActive(false);
                    if (panelMovement != null)
                    {
                        panelMovement.SetEnabled(true);
                    }
                });
        }
    }
    
    public void OnCategorySelected(CodexCategory category)
    {
        Debug.Log($"Category selected: {category}");
        currentCategory = category;
        UpdateCategoryUI();
        
        // Make sure menu page is visible (not entry page)
        ShowMenuPage();
    }
    
    private void UpdateCategoryUI()
    {
        // Get entries for current category
        List<CodexEntry> categoryEntries = codexData.entries
            .Where(e => e.category == currentCategory.ToString())
            .ToList();
        
        Debug.Log($"Found {categoryEntries.Count} entries for category {currentCategory}");
        
        // Update the scroll view with these entries
        if (menuScrollView != null)
        {
            menuScrollView.PopulateEntries(categoryEntries);
            
            // Reset scroll position to top
            if (menuScrollView.content != null)
            {
                menuScrollView.content.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            Debug.LogError("MenuScrollView component not found!");
        }
        
        // Update visual state of category buttons
        UpdateCategoryButtonsState();
    }
    
    private void UpdateCategoryButtonsState()
    {
        // Style the selected button differently
        Button[] categoryButtons = { charactersButton, historyButton, locationsButton, organizationsButton };
        
        foreach (Button button in categoryButtons)
        {
            if (button != null)
            {
                // Get button's category
                CodexCategory buttonCategory = GetButtonCategory(button);
                
                // Update visual state
                Image buttonImage = button.GetComponent<Image>();
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonImage != null)
                {
                    if (buttonCategory == currentCategory)
                    {
                        // Selected state
                        buttonImage.color = new Color(0.3f, 0.3f, 0.8f, 1f);
                        if (buttonText != null)
                        {
                            buttonText.color = Color.white;
                            buttonText.fontStyle = FontStyles.Bold;
                        }
                    }
                    else
                    {
                        // Normal state
                        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                        if (buttonText != null)
                        {
                            buttonText.color = Color.white;
                            buttonText.fontStyle = FontStyles.Normal;
                        }
                    }
                }
            }
        }
    }
    
    private CodexCategory GetButtonCategory(Button button)
    {
        if (button == charactersButton) return CodexCategory.Characters;
        if (button == historyButton) return CodexCategory.History;
        if (button == locationsButton) return CodexCategory.Locations;
        if (button == organizationsButton) return CodexCategory.Organizations;
        
        return CodexCategory.Characters; // Default
    }
    
    private void ToggleSubcategory(string subcategory, GameObject entriesObj, Image arrowImage)
    {
        bool isExpanded = !entriesObj.activeSelf;
        entriesObj.SetActive(isExpanded);
        arrowImage.transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 0 : -90);
        subcategoryExpanded[subcategory] = isExpanded;
        
        if (isExpanded)
        {
            UpdateEntriesForSubcategory(subcategory, entriesObj.transform);
        }
    }
    
    private void UpdateEntriesForSubcategory(string subcategory, Transform container)
    {
        var entries = codexData.entries
            .Where(e => e.category == currentCategory.ToString() && e.subcategory == subcategory);
            
        foreach (var entry in entries)
        {
            Button entryButton = CreateEntryButton(entry);
            entryButton.transform.SetParent(container, false);
        }
    }
    
    private Button CreateEntryButton(CodexEntry entry)
    {
        if (uiSetup == null || uiSetup.BaseEntryButton == null)
        {
            Debug.LogError("Base entry button template not found!");
            return null;
        }

        // Create a new button as a child of the subcategory container
        GameObject buttonObj = GameObject.Instantiate(uiSetup.BaseEntryButton.gameObject, subcategoryContainer);
        buttonObj.name = entry.title;
        buttonObj.SetActive(true); // Make sure the new button is active

        // Get the button component
        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"Button component not found on {entry.title} button!");
            return null;
        }

        // Set up the button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = entry.title;
        }
        else
        {
            Debug.LogError($"TextMeshProUGUI component not found on {entry.title} button!");
        }

        // Add click listener
        button.onClick.AddListener(() => ShowEntry(entry));

        return button;
    }
    
    public void ShowEntry(CodexEntry entry)
    {
        Debug.Log($"Showing entry: {entry.title}");
        currentEntry = entry;

        if (entryTitleText == null || entryDescriptionText == null || entryImage == null)
        {
            Debug.LogError("One or more entry page UI elements (Title, Description, Image) are null.");
            return;
        }

        entryTitleText.text = entry.title;
        
        // Find EntryScrollView through EntryPage
        if (entryPage != null)
        {
            Transform entryScrollViewTransform = entryPage.transform.Find("EntryScrollView");
            if (entryScrollViewTransform != null)
            {
                // Find the viewport first
                Transform viewport = entryScrollViewTransform.Find("Viewport");
                if (viewport != null)
                {
                    // Then find the content inside the viewport
                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        // Get or create TextMeshProUGUI component on the content
                        TextMeshProUGUI contentText = content.GetComponent<TextMeshProUGUI>();
                        if (contentText == null)
                        {
                            contentText = content.gameObject.AddComponent<TextMeshProUGUI>();
                        }
                        
                        // Configure the text component for proper wrapping
                        contentText.text = entry.description;
                        contentText.fontSize = 14;
                        contentText.color = Color.white;
                        contentText.alignment = TextAlignmentOptions.TopLeft;
                        
                        // Enable text wrapping and overflow handling
                        contentText.enableWordWrapping = true;
                        contentText.overflowMode = TextOverflowModes.Overflow; // Allow vertical overflow for scrolling
                        contentText.textWrappingMode = TextWrappingModes.Normal;
                        
                        // Set margins for better readability
                        contentText.margin = new Vector4(10, 10, 10, 10);
                        
                        // Add ContentSizeFitter to automatically adjust height
                        ContentSizeFitter sizeFitter = content.gameObject.GetComponent<ContentSizeFitter>();
                        if (sizeFitter == null)
                        {
                            sizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();
                        }
                        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        
                        // Set up the content's RectTransform for proper text wrapping
                        RectTransform contentRect = content.GetComponent<RectTransform>();
                        contentRect.anchorMin = new Vector2(0, 1);
                        contentRect.anchorMax = new Vector2(1, 1);
                        contentRect.pivot = new Vector2(0.5f, 1);
                        contentRect.anchoredPosition = Vector2.zero;
                        
                        // Set width to fill container, height will be determined by ContentSizeFitter
                        contentRect.sizeDelta = new Vector2(0, 0);
                        contentRect.offsetMin = new Vector2(0, contentRect.offsetMin.y); // Left edge
                        contentRect.offsetMax = new Vector2(0, contentRect.offsetMax.y); // Right edge
                    }
                    else
                    {
                        Debug.LogError("Content not found inside Viewport!");
                    }
                }
                else
                {
                    Debug.LogError("Viewport not found in EntryScrollView!");
                }
            }
            else
            {
                Debug.LogError("EntryScrollView not found in EntryPage!");
            }
        }
        else
        {
            Debug.LogError("EntryPage is null!");
        }
        
        // Handle image loading
        if (!string.IsNullOrEmpty(entry.imagePath))
        {
            Debug.Log($"Attempting to load image for entry: {entry.title} with path: {entry.imagePath}");
            
            // Try to load the image with different extensions
            Sprite sprite = LoadImageWithExtensions(entry.imagePath);
            if (sprite != null)
            {
                entryImage.sprite = sprite;
                entryImage.gameObject.SetActive(true);
                
                // Configure image to maintain aspect ratio and fit properly
                entryImage.preserveAspect = true;
                entryImage.type = Image.Type.Simple;
                
                Debug.Log($"✅ Successfully loaded and assigned image: {entry.imagePath}");
            }
            else
            {
                entryImage.gameObject.SetActive(false);
                Debug.LogWarning($"❌ Image not found: {entry.imagePath} (tried multiple loading methods)");
            }
        }
        else
        {
            entryImage.gameObject.SetActive(false);
            Debug.Log($"No image path specified for entry: {entry.title}");
        }
        
        ShowEntryPage();
    }
    
    private void ShowEntryPage()
    {
        menuPage.SetActive(false);
        entryPage.SetActive(true);
        
        // Hide CategoryContainer when EntryPage is active
        if (categoryContainer != null)
        {
            categoryContainer.gameObject.SetActive(false);
        }
        
        // Reset entry scroll position to top
        if (entryScrollView != null && entryScrollView.content != null)
        {
            entryScrollView.content.anchoredPosition = Vector2.zero;
        }
    }
    
    private void ShowMenuPage()
    {
        Debug.Log("Showing menu page");
        entryPage.SetActive(false);
        menuPage.SetActive(true);
        
        // Show CategoryContainer when MenuPage is active
        if (categoryContainer != null)
        {
            categoryContainer.gameObject.SetActive(true);
        }
        
        // Reset menu scroll position to top
        if (menuScrollView != null && menuScrollView.content != null)
        {
            menuScrollView.content.anchoredPosition = Vector2.zero;
        }
    }

    private void VerifyReferences()
    {
        if (menuPage == null) Debug.LogError("MenuPage not found!");
        if (entryPage == null) Debug.LogError("EntryPage not found!");
        if (categoryContainer == null) Debug.LogError("CategoryContainer not found!");
        if (subcategoryContainer == null) Debug.LogError("SubcategoryContainer not found!");
        if (toggleButton == null) Debug.LogError("ToggleButton not found!");
        if (backButton == null) Debug.LogError("BackButton not found!");
        if (entryTitleText == null) Debug.LogError("EntryTitleText not found!");
        if (entryDescriptionText == null) Debug.LogError("EntryDescriptionText not found!");
        if (entryImage == null) Debug.LogError("EntryImage not found!");
        if (menuScrollView == null) Debug.LogError("MenuScrollView not found!");
        if (entryScrollView == null) Debug.LogError("EntryScrollView not found!");
        Debug.Log("VerifyReferences: All references verified successfully");
    }
    
    /// <summary>
    /// Try to load an image with different file extensions
    /// </summary>
    private Sprite LoadImageWithExtensions(string basePath)
    {
        Debug.Log($"🔍 LoadImageWithExtensions called with basePath: {basePath}");
        
        // First try to load as sprite directly
        Debug.Log($"🔍 Step 1: Trying to load as sprite...");
        Sprite sprite = TryLoadAsSprite(basePath);
        if (sprite != null)
        {
            Debug.Log($"✅ Successfully loaded as sprite!");
            return sprite;
        }
        
        // If sprite loading fails, try loading as texture and convert to sprite
        Debug.Log($"🔍 Step 2: Trying to load as texture and convert...");
        sprite = TryLoadAsTextureAndConvert(basePath);
        if (sprite != null)
        {
            Debug.Log($"✅ Successfully loaded as texture and converted to sprite!");
            return sprite;
        }
        
        Debug.LogWarning($"❌ Could not find image at any of the attempted paths for: {basePath}");
        return null;
    }
    
    /// <summary>
    /// Try to load image as sprite directly from Resources
    /// </summary>
    private Sprite TryLoadAsSprite(string basePath)
    {
        string[] extensions = { "", ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
        
        foreach (string extension in extensions)
        {
            string fullPath = basePath + extension;
            Debug.Log($"Trying to load as sprite: {fullPath}");
            
            Sprite sprite = Resources.Load<Sprite>(fullPath);
            if (sprite != null)
            {
                Debug.Log($"✅ Found sprite at: {fullPath}");
                return sprite;
            }
            else
            {
                Debug.Log($"❌ No sprite found at: {fullPath}");
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Try to load image as texture and convert to sprite
    /// </summary>
    private Sprite TryLoadAsTextureAndConvert(string basePath)
    {
        string[] extensions = { "", ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
        
        foreach (string extension in extensions)
        {
            string fullPath = basePath + extension;
            Debug.Log($"Trying to load as texture: {fullPath}");
            
            // Try loading as Texture2D
            Texture2D texture = Resources.Load<Texture2D>(fullPath);
            if (texture != null)
            {
                Debug.Log($"✅ Found texture at: {fullPath}, converting to sprite");
                
                // Create sprite from texture
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), // Pivot at center
                    100.0f // Pixels per unit
                );
                
                return sprite;
            }
            
            // Try loading as TextAsset (for raw image data)
            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);
            if (textAsset != null)
            {
                Debug.Log($"✅ Found image data at: {fullPath}, loading as texture");
                
                // Create a new texture
                Texture2D texture2D = new Texture2D(2, 2);
                
                // Load image data into texture using ImageConversion
                if (ImageConversion.LoadImage(texture2D, textAsset.bytes))
                {
                    Debug.Log($"✅ Successfully loaded image data from: {fullPath}");
                    
                    // Create sprite from texture
                    Sprite sprite = Sprite.Create(
                        texture2D,
                        new Rect(0, 0, texture2D.width, texture2D.height),
                        new Vector2(0.5f, 0.5f), // Pivot at center
                        100.0f // Pixels per unit
                    );
                    
                    return sprite;
                }
                else
                {
                    Debug.LogWarning($"❌ Failed to load image data from: {fullPath}");
                    UnityEngine.Object.Destroy(texture2D);
                }
            }
            else
            {
                Debug.Log($"❌ No TextAsset found at: {fullPath}");
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if the game is over by seeing if any player has been eliminated
    /// </summary>
    private bool IsGameOver()
    {
        // Try to find the CombatManager to check player elimination
        var combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            // Check if either player has been eliminated
            return combatManager.IsPlayerEliminated(Unit.Player.Player1) || 
                   combatManager.IsPlayerEliminated(Unit.Player.Player2);
        }
        
        return false; // If no combat manager found, assume game is still active
    }
} 