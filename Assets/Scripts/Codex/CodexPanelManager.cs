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
        // Set initial position in the middle of the screen
        RectTransform canvasRect = codexPanel.parent as RectTransform;
        if (canvasRect != null)
        {
            codexPanel.anchoredPosition = new Vector2(
                canvasRect.rect.width - codexPanel.rect.width / 2,
                canvasRect.rect.height / 2
            );
        }
        else
        {
            codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0);
        }

        menuPage.SetActive(true);
        entryPage.SetActive(false);
        
        // Set initial category
        currentCategory = CodexCategory.Characters;
        UpdateCategoryUI();

        // Start with panel hidden but positioned correctly
        codexPanel.gameObject.SetActive(true);
        codexPanel.anchoredPosition = new Vector2(codexPanel.anchoredPosition.x + codexPanel.rect.width, codexPanel.anchoredPosition.y);
        isPanelVisible = false;
    }
    
    private void SetupEventListeners()
    {
        toggleButton.onClick.AddListener(TogglePanel);
        backButton.onClick.AddListener(ShowMenuPage);
        
        // Setup category buttons
        Button[] categoryButtons = categoryContainer.GetComponentsInChildren<Button>();
        foreach (Button categoryButton in categoryButtons)
        {
            TextMeshProUGUI buttonText = categoryButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string buttonName = buttonText.text.ToLower();
                if (categoryButtonMap.TryGetValue(buttonName, out CodexCategory category))
                {
                    categoryButton.onClick.AddListener(() => {
                        Debug.Log($"Category button clicked: {buttonName}");
                        OnCategorySelected(category);
                    });
                }
            }
        }
    }
    
    public void TogglePanel()
    {
        Debug.Log("Toggle button clicked");
        isPanelVisible = !isPanelVisible;
        
        if (isPanelVisible)
        {
            codexPanel.gameObject.SetActive(true);
            // Animate from off-screen to visible position
            codexPanel.DOAnchorPosX(codexPanel.anchoredPosition.x - codexPanel.rect.width, slideDuration)
                .SetEase(slideEase)
                .OnStart(() => {
                    if (panelMovement != null)
                    {
                        panelMovement.SetEnabled(false); // Disable camera movement when panel is open
                    }
                });
        }
        else
        {
            // Animate to off-screen position
            codexPanel.DOAnchorPosX(codexPanel.anchoredPosition.x + codexPanel.rect.width, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => {
                    codexPanel.gameObject.SetActive(false);
                    if (panelMovement != null)
                    {
                        panelMovement.SetEnabled(true); // Re-enable camera movement when panel is closed
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
        foreach (string subcategory in subcategories)
        {
            GameObject subcategoryObj = CreateSubcategoryObject(subcategory);
            subcategoryObj.transform.SetParent(subcategoryContainer, false);
        }
    }
    
    private GameObject CreateSubcategoryObject(string subcategory)
    {
        GameObject subcategoryObj = new GameObject(subcategory);
        RectTransform rectTransform = subcategoryObj.AddComponent<RectTransform>();
        
        // Create header button
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(subcategoryObj.transform, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        Button headerButton = headerObj.AddComponent<Button>();
        TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
        
        // Setup header
        headerText.text = subcategory;
        headerText.alignment = TextAlignmentOptions.Left;
        headerText.fontSize = 16;
        headerText.color = Color.white;
        
        // Create expand/collapse arrow using Image
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(headerObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        Image arrowImage = arrowObj.AddComponent<Image>();
        
        // Set arrow image to a simple triangle sprite
        arrowImage.sprite = CreateTriangleSprite();
        arrowImage.color = Color.white;
        
        // Position the arrow on the right side
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-10, 0);
        
        // Create entries container
        GameObject entriesObj = new GameObject("Entries");
        entriesObj.transform.SetParent(subcategoryObj.transform, false);
        RectTransform entriesRect = entriesObj.AddComponent<RectTransform>();
        VerticalLayoutGroup entriesLayout = entriesObj.AddComponent<VerticalLayoutGroup>();
        entriesLayout.spacing = 5;
        entriesLayout.padding = new RectOffset(20, 0, 0, 0);
        
        // Setup layout
        LayoutElement layoutElement = subcategoryObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 30;
        
        // Setup header button click
        headerButton.onClick.AddListener(() => ToggleSubcategory(subcategory, entriesObj, arrowImage));
        
        // Initialize state
        bool isExpanded = subcategoryExpanded.ContainsKey(subcategory) ? subcategoryExpanded[subcategory] : false;
        entriesObj.SetActive(isExpanded);
        arrowImage.transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 0 : -90);
        
        return subcategoryObj;
    }

    private Sprite CreateTriangleSprite()
    {
        // Create a simple triangle texture
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                // Create a simple triangle shape
                float centerX = 16;
                float centerY = 16;
                float radius = 12;
                
                // Calculate distance from center
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                // Create a triangle shape
                if (distance < radius && 
                    Mathf.Abs(dx) < radius * 0.8f && 
                    dy > -radius * 0.5f)
                {
                    colors[y * 32 + x] = Color.white;
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
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