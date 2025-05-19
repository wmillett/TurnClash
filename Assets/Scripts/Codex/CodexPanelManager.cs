using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;

public class CodexPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public RectTransform codexPanel;
    [SerializeField] public GameObject menuPage;
    [SerializeField] public GameObject entryPage;
    [SerializeField] public Button toggleButton;
    [SerializeField] public Button backButton;
    [SerializeField] public Transform categoryContainer;
    [SerializeField] public Transform subcategoryContainer;
    [SerializeField] public Transform entryContainer;
    
    [Header("Entry Page References")]
    [SerializeField] public TextMeshProUGUI entryTitleText;
    [SerializeField] public TextMeshProUGUI entryDescriptionText;
    [SerializeField] public Image entryImage;
    
    [Header("Data")]
    [SerializeField] private TextAsset codexJsonFile;
    
    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private Ease slideEase = Ease.OutQuad;
    
    private CodexData codexData;
    private CodexCategory currentCategory;
    private string currentSubcategory;
    private CodexEntry currentEntry;
    private bool isPanelVisible = false;
    private Dictionary<string, bool> subcategoryExpanded = new Dictionary<string, bool>();
    
    private void Start()
    {
        LoadCodexData();
        SetupUI();
        SetupEventListeners();
    }
    
    private void LoadCodexData()
    {
        if (codexJsonFile != null)
        {
            codexData = JsonUtility.FromJson<CodexData>(codexJsonFile.text);
        }
        else
        {
            Debug.LogError("Codex JSON file not assigned!");
        }
    }
    
    private void SetupUI()
    {
        // Set initial position off-screen
        codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0);
        menuPage.SetActive(true);
        entryPage.SetActive(false);
        
        // Set initial category
        currentCategory = CodexCategory.Characters;
        UpdateCategoryUI();
    }
    
    private void SetupEventListeners()
    {
        toggleButton.onClick.AddListener(TogglePanel);
        backButton.onClick.AddListener(ShowMenuPage);
        
        // Setup category buttons
        foreach (CodexCategory category in System.Enum.GetValues(typeof(CodexCategory)))
        {
            // Find any button in the categoryContainer
            Button[] categoryButtons = categoryContainer.GetComponentsInChildren<Button>();
            foreach (Button categoryButton in categoryButtons)
            {
                // Check if the button's text matches the category name
                TextMeshProUGUI buttonText = categoryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && buttonText.text == category.ToString())
                {
                    categoryButton.onClick.AddListener(() => OnCategorySelected(category));
                    break;
                }
            }
        }
    }
    
    private void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        
        if (isPanelVisible)
        {
            codexPanel.gameObject.SetActive(true);
            codexPanel.DOAnchorPosX(0, slideDuration).SetEase(slideEase);
        }
        else
        {
            codexPanel.DOAnchorPosX(codexPanel.rect.width, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => codexPanel.gameObject.SetActive(false));
        }
    }
    
    private void OnCategorySelected(CodexCategory category)
    {
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
        
        foreach (Transform child in entryContainer)
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
        
        // Create expand/collapse arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(headerObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▼";
        arrowText.alignment = TextAlignmentOptions.Right;
        arrowText.fontSize = 12;
        
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
        headerButton.onClick.AddListener(() => ToggleSubcategory(subcategory, entriesObj, arrowText));
        
        // Initialize state
        bool isExpanded = subcategoryExpanded.ContainsKey(subcategory) ? subcategoryExpanded[subcategory] : false;
        entriesObj.SetActive(isExpanded);
        arrowText.text = isExpanded ? "▼" : "▶";
        
        return subcategoryObj;
    }
    
    private void ToggleSubcategory(string subcategory, GameObject entriesObj, TextMeshProUGUI arrowText)
    {
        bool isExpanded = !entriesObj.activeSelf;
        entriesObj.SetActive(isExpanded);
        arrowText.text = isExpanded ? "▼" : "▶";
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
        entryPage.SetActive(false);
        menuPage.SetActive(true);
    }
} 