using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;

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
    
    // New Scroll View Reference
    private CodexScrollView menuScrollView;
    
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
        SetupUI();
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
            entryTitleText = FindChildByName("Title")?.GetComponent<TextMeshProUGUI>();
            entryDescriptionText = FindChildByName("Description")?.GetComponent<TextMeshProUGUI>();
            entryImage = FindChildByName("Image")?.GetComponent<Image>();
        }
        
        // Find or add scroll view component
        if (menuPage != null)
        {
            GameObject scrollViewObj = FindChildByName("ScrollView");
            if (scrollViewObj != null)
            {
                menuScrollView = scrollViewObj.GetComponent<CodexScrollView>();
                if (menuScrollView == null)
                {
                    menuScrollView = scrollViewObj.AddComponent<CodexScrollView>();
                }
            }
            else
            {
                Debug.LogError("ScrollView not found in MenuPage! Please add a ScrollView GameObject.");
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
    
    private void SetupUI()
    {
        // Position and scale the Codex Panel
        if (codexPanel != null)
        {
            // Set panel to go off-screen initially
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // Match scene anchor settings
            codexPanel.anchorMin = new Vector2(1, 0.5f);
            codexPanel.anchorMax = new Vector2(1, 0.5f);
            codexPanel.pivot = new Vector2(1, 0.5f);
            
            // Determine size based on screen dimensions
            float widthPercent = 0.3f; // Use 30% of screen width
            float heightPercent = 0.8f; // Use 80% of screen height
            
            // Set the panel size
            codexPanel.sizeDelta = new Vector2(screenWidth * widthPercent, screenHeight * heightPercent);
            
            // Position panel off-screen to the right
            codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0);
        }
        
        // Configure category buttons container
        if (categoryContainer != null)
        {
            RectTransform categoryRect = categoryContainer.GetComponent<RectTransform>();
            if (categoryRect != null)
            {
                // Reset scale to prevent size issues
                categoryRect.localScale = Vector3.one;
                
                // Set proper anchoring and size relative to the CodexPanel
                categoryRect.anchorMin = new Vector2(0, 1);  // Anchor to top-left
                categoryRect.anchorMax = new Vector2(1, 1);  // Anchor to top-right
                categoryRect.pivot = new Vector2(0.5f, 1);   // Pivot at top-center
                
                // Position very close to the top of the CodexPanel
                categoryRect.anchoredPosition = new Vector2(0, -5);
                
                // Set appropriate height and make it stretch horizontally
                categoryRect.sizeDelta = new Vector2(0, 50); // Increased height to 50 for bigger buttons
            }
            
            // Resize all category buttons
            ResizeCategoryButtons();
        }
        
        // Configure subcategory container
        if (subcategoryContainer != null)
        {
            RectTransform subcategoryRect = subcategoryContainer.GetComponent<RectTransform>();
            if (subcategoryRect != null)
            {
                // Set proper anchoring and sizing
                subcategoryRect.anchorMin = new Vector2(0, 0);  // Anchor to bottom-left
                subcategoryRect.anchorMax = new Vector2(1, 1);  // Anchor to top-right
                subcategoryRect.pivot = new Vector2(0.5f, 0.5f); // Pivot at center
                
                // Position below the category container
                subcategoryRect.anchoredPosition = new Vector2(0, -60);
                
                // Adjust size to fill the rest of the panel area
                subcategoryRect.offsetMin = new Vector2(10, 10); // Left, bottom margins
                subcategoryRect.offsetMax = new Vector2(-10, -60); // Right, top (negative from top) margins
                
                // Add a Layout Group to arrange buttons properly
                VerticalLayoutGroup layoutGroup = subcategoryContainer.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = subcategoryContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                    layoutGroup.childAlignment = TextAnchor.UpperCenter;
                    layoutGroup.spacing = 15;
                    layoutGroup.padding = new RectOffset(10, 10, 10, 10);
                    layoutGroup.childControlHeight = true;
                    layoutGroup.childControlWidth = true;
                    layoutGroup.childForceExpandHeight = false;
                    layoutGroup.childForceExpandWidth = true;
                }
                
                // Add Content Size Fitter to adjust content size
                ContentSizeFitter sizeFitter = subcategoryContainer.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = subcategoryContainer.gameObject.AddComponent<ContentSizeFitter>();
                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
        }

        // Ensure toggle button is always visible and positioned correctly
        if (toggleButton != null)
        {
            resetButtonPosition();
        }

        menuPage.SetActive(true);
        entryPage.SetActive(false);

        // Set initial category
        currentCategory = CodexCategory.Characters;
        UpdateCategoryUI();

        // Start with panel hidden
        codexPanel.gameObject.SetActive(true);
        isPanelVisible = false;
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
    

    private void setButtonSize()
    {
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.SetParent(codexPanel);
        
        // Anchor to the left edge but slightly inset
        toggleRect.anchorMin = new Vector2(0, 0.5f);
        toggleRect.anchorMax = new Vector2(0, 0.5f);
        toggleRect.pivot = new Vector2(1, 0.5f); // Change pivot to right side of button
        
        // Position the button so it's partially overlapping the left edge of the panel
        toggleRect.anchoredPosition = new Vector2(5, 0); // Positive X means inward from left edge
        
        // Make the button smaller (adjust size as needed)
        toggleRect.sizeDelta = new Vector2(100, 100);
    }
    
    private void resetButtonPosition()
    {
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.SetParent(codexPanel.parent);
        toggleRect.anchorMin = new Vector2(1, 0.5f);
        toggleRect.anchorMax = new Vector2(1, 0.5f);
        toggleRect.pivot = new Vector2(1, 0.5f); // Change from 0.5f to 1 for right-aligned pivot
        toggleRect.anchoredPosition = new Vector2(0, 0); // Fixed position 10 pixels from right edge
        
        // Restore original button size if needed
        toggleRect.sizeDelta = new Vector2(100, 30);
    }
    
    public void TogglePanel()
    {
        Debug.Log("Toggle button clicked");
        isPanelVisible = !isPanelVisible;

        if (isPanelVisible)
        {
            codexPanel.gameObject.SetActive(true);

            // Force scale to be exactly 1
            codexPanel.localScale = Vector3.one;

            // Get screen dimensions
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // Set consistent panel size (30% width, 80% height)
            float widthPercent = 0.3f;
            float heightPercent = 0.8f;
            
            // Set size
            codexPanel.sizeDelta = new Vector2(screenWidth * widthPercent, screenHeight * heightPercent);

            // Ensure toggle button is a child of the panel
            if (toggleButton != null)
            {
                setButtonSize();
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
            // Animate panel sliding out to the right
            codexPanel.DOAnchorPosX(codexPanel.rect.width, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => {
                    // Move toggle button back to canvas level after animation
                    if (toggleButton != null)
                    {
                        resetButtonPosition();
                    }

                    codexPanel.gameObject.SetActive(false);
                    if (panelMovement != null)
                    {
                        panelMovement.SetEnabled(true);
                    }
                });
        }
    }
    
    private void OnCategorySelected(CodexCategory category)
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
        }
        else
        {
            Debug.LogError("CodexScrollView component not found!");
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
        GameObject buttonObj = new GameObject(entry.title);
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        Button button = buttonObj.AddComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.AddComponent<TextMeshProUGUI>();
        
        // Setup button appearance
        buttonText.text = entry.title;
        buttonText.alignment = TextAlignmentOptions.Left;
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        
        // Add hover effect
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
        button.colors = colors;
        
        button.onClick.AddListener(() => ShowEntry(entry));
        
        return button;
    }
    
    public void ShowEntry(CodexEntry entry)
    {
        Debug.Log($"Showing entry: {entry.title}");
        currentEntry = entry;
        entryTitleText.text = entry.title;
        entryDescriptionText.text = entry.description;
        
        if (!string.IsNullOrEmpty(entry.imagePath))
        {
            Sprite sprite = Resources.Load<Sprite>(entry.imagePath);
            if (sprite != null)
            {
                entryImage.sprite = sprite;
                entryImage.gameObject.SetActive(true);
            }
            else
            {
                entryImage.gameObject.SetActive(false);
                Debug.LogWarning($"Image not found: {entry.imagePath}");
            }
        }
        else
        {
            entryImage.gameObject.SetActive(false);
        }
        
        ShowEntryPage();
    }
    
    private void ShowEntryPage()
    {
        menuPage.SetActive(false);
        entryPage.SetActive(true);
    }
    
    private void ShowMenuPage()
    {
        Debug.Log("Back button clicked - showing menu page");
        entryPage.SetActive(false);
        menuPage.SetActive(true);
    }

    // Resize all category buttons to be bigger
    private void ResizeCategoryButtons()
    {
        Button[] categoryButtons = { charactersButton, historyButton, locationsButton, organizationsButton };
        
        foreach (Button button in categoryButtons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Make buttons bigger
                    buttonRect.sizeDelta = new Vector2(120, 40);
                    
                    // Adjust text size
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.fontSize = 18; // Increase font size
                    }
                }
            }
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
    }
} 