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
    private Transform subcategoryContainer;
    
    // Entry Page References
    private TextMeshProUGUI entryTitleText;
    private TextMeshProUGUI entryDescriptionText;
    private Image entryImage;
    
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
        subcategoryContainer = FindChildByName("SubcategoryContainer")?.transform;
        toggleButton = FindChildByName("ToggleButton")?.GetComponent<Button>();
        backButton = FindChildByName("BackButton")?.GetComponent<Button>();

        // Find entry page elements
        if (entryPage != null)
        {
            entryTitleText = FindChildByName("Title")?.GetComponent<TextMeshProUGUI>();
            entryDescriptionText = FindChildByName("Description")?.GetComponent<TextMeshProUGUI>();
            entryImage = FindChildByName("Image")?.GetComponent<Image>();
        }

        // Load JSON file from Resources
        codexJsonFile = Resources.Load<TextAsset>("CodexData");
        if (codexJsonFile == null)
        {
            Debug.LogError("CodexData.json not found in Resources folder!");
        }

        // Log any missing components
        LogMissingComponents();
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
        // Set initial position using screen coordinates
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float aspectRatio = screenWidth / screenHeight;

        // Use consistent 60% width regardless of aspect ratio
        float widthPercent = 0.6f;
        float heightPercent = 0.8f;

        // Set the panel's size to a moderate size adjusted for aspect ratio
        codexPanel.sizeDelta = new Vector2(screenWidth * widthPercent, screenHeight * heightPercent);

        // Force the scale to be exactly 1 to prevent scaling issues
        codexPanel.localScale = Vector3.one;

        // Position the panel on the right side of the screen
        codexPanel.anchorMin = new Vector2(1, 0.5f);
        codexPanel.anchorMax = new Vector2(1, 0.5f);
        codexPanel.pivot = new Vector2(1, 0.5f);
        codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0); // Start fully off-screen

        // Setup CategoryContainer
        if (categoryContainer != null)
        {
            RectTransform categoryRect = categoryContainer.GetComponent<RectTransform>();
            if (categoryRect != null)
            {
                // Reset scale to prevent size issues
                categoryRect.localScale = Vector3.one;
                
                // Set proper anchoring and size
                categoryRect.anchorMin = new Vector2(0, 1);  // Anchor to top-left
                categoryRect.anchorMax = new Vector2(1, 1);  // Anchor to top-right
                categoryRect.pivot = new Vector2(0.5f, 1);   // Pivot at top-center
                
                // Position it with a small margin from the top
                categoryRect.anchoredPosition = new Vector2(0, -20);
                
                // Set the fixed height and make it stretch horizontally
                categoryRect.sizeDelta = new Vector2(0, 50); 
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
        toggleRect.pivot = new Vector2(0.5f, 0.5f);
        toggleRect.anchoredPosition = new Vector2(-toggleRect.rect.width / 2, 0);
        
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

            // Get screen dimensions and aspect ratio
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float aspectRatio = screenWidth / screenHeight;

            // Use consistent 60% width regardless of aspect ratio
            float widthPercent = 0.6f;
            float heightPercent = 0.8f;

            // Set size with aspect ratio adjustment
            codexPanel.sizeDelta = new Vector2(screenWidth * widthPercent, screenHeight * heightPercent);

            // Ensure toggle button is a child of the panel
            if (toggleButton != null)
            {
                setButtonSize();
            }

            // Log panel size for debugging
            Debug.Log($"Screen size: {screenWidth} x {screenHeight}, Aspect ratio: {aspectRatio}");
            Debug.Log($"Panel size: {codexPanel.rect.width} x {codexPanel.rect.height}");
            Debug.Log($"Panel local scale: {codexPanel.localScale}");

            // Animate panel sliding in
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
            // Animate panel sliding out
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
    }
    
    private void UpdateCategoryUI()
    {
        // Clear existing subcategories and entries
        foreach (Transform child in subcategoryContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get unique subcategories for current category
        var subcategories = codexData.entries
            .Where(e => e.category == currentCategory.ToString())
            .Select(e => e.subcategory)
            .Distinct();
            
        // Create subcategory buttons
        // foreach (string subcategory in subcategories)
        // {
        //     GameObject subcategoryObj = CreateSubcategoryObject(subcategory);
        //     subcategoryObj.transform.SetParent(subcategoryContainer, false);
        // }
    }
    
    // private GameObject CreateSubcategoryObject(string subcategory)
    // {
    //     GameObject subcategoryObj = new GameObject(subcategory);
    //     RectTransform rectTransform = subcategoryObj.AddComponent<RectTransform>();
        
    //     // Create header button
    //     GameObject headerObj = new GameObject("Header");
    //     headerObj.transform.SetParent(subcategoryObj.transform, false);
    //     RectTransform headerRect = headerObj.AddComponent<RectTransform>();
    //     Button headerButton = headerObj.AddComponent<Button>();
    //     TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
        
    //     // Setup header
    //     headerText.text = subcategory;
    //     headerText.alignment = TextAlignmentOptions.Left;
    //     headerText.fontSize = 16;
    //     headerText.color = Color.white;
        
    //     // Create expand/collapse arrow using Image
    //     GameObject arrowObj = new GameObject("Arrow");
    //     arrowObj.transform.SetParent(headerObj.transform, false);
    //     RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
    //     Image arrowImage = arrowObj.AddComponent<Image>();
        
    //     // Set arrow image to a simple triangle sprite
    //     arrowImage.sprite = CreateTriangleSprite();
    //     arrowImage.color = Color.white;
        
    //     // Position the arrow on the right side
    //     arrowRect.anchorMin = new Vector2(1, 0.5f);
    //     arrowRect.anchorMax = new Vector2(1, 0.5f);
    //     arrowRect.pivot = new Vector2(0.5f, 0.5f);
    //     arrowRect.sizeDelta = new Vector2(20, 20);
    //     arrowRect.anchoredPosition = new Vector2(-10, 0);
        
    //     // Create entries container
    //     GameObject entriesObj = new GameObject("Entries");
    //     entriesObj.transform.SetParent(subcategoryObj.transform, false);
    //     RectTransform entriesRect = entriesObj.AddComponent<RectTransform>();
    //     VerticalLayoutGroup entriesLayout = entriesObj.AddComponent<VerticalLayoutGroup>();
    //     entriesLayout.spacing = 5;
    //     entriesLayout.padding = new RectOffset(20, 0, 0, 0);
        
    //     // Setup layout
    //     LayoutElement layoutElement = subcategoryObj.AddComponent<LayoutElement>();
    //     layoutElement.minHeight = 30;
        
    //     // Setup header button click
    //     headerButton.onClick.AddListener(() => ToggleSubcategory(subcategory, entriesObj, arrowImage));
        
    //     // Initialize state
    //     bool isExpanded = subcategoryExpanded.ContainsKey(subcategory) ? subcategoryExpanded[subcategory] : false;
    //     entriesObj.SetActive(isExpanded);
    //     arrowImage.transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 0 : -90);
        
    //     return subcategoryObj;
    // }

    // private Sprite CreateTriangleSprite()
    // {
    //     // Create a simple triangle texture
    //     Texture2D texture = new Texture2D(32, 32);
    //     Color[] colors = new Color[32 * 32];
        
    //     for (int y = 0; y < 32; y++)
    //     {
    //         for (int x = 0; x < 32; x++)
    //         {
    //             // Create a simple triangle shape
    //             float centerX = 16;
    //             float centerY = 16;
    //             float radius = 12;
                
    //             // Calculate distance from center
    //             float dx = x - centerX;
    //             float dy = y - centerY;
    //             float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
    //             // Create a triangle shape
    //             if (distance < radius && 
    //                 Mathf.Abs(dx) < radius * 0.8f && 
    //                 dy > -radius * 0.5f)
    //             {
    //                 colors[y * 32 + x] = Color.white;
    //             }
    //             else
    //             {
    //                 colors[y * 32 + x] = Color.clear;
    //             }
    //         }
    //     }
        
    //     texture.SetPixels(colors);
    //     texture.Apply();
        
    //     return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    // }

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
    
    private void ShowEntry(CodexEntry entry)
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
} 