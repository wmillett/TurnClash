using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class CodexPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private GameObject menuPage;
    [SerializeField] private GameObject entryPage;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform categoryContainer;
    [SerializeField] private Transform subcategoryContainer;
    [SerializeField] private Transform entryContainer;
    
    [Header("Entry Page References")]
    [SerializeField] private TextMeshProUGUI entryTitleText;
    [SerializeField] private TextMeshProUGUI entryDescriptionText;
    [SerializeField] private Image entryImage;
    
    [Header("Data")]
    [SerializeField] private TextAsset codexJsonFile;
    
    private CodexData codexData;
    private CodexCategory currentCategory;
    private string currentSubcategory;
    private CodexEntry currentEntry;
    private bool isPanelVisible = false;
    
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
        codexPanel.SetActive(false);
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
            Button categoryButton = categoryContainer.Find(category.ToString())?.GetComponent<Button>();
            if (categoryButton != null)
            {
                categoryButton.onClick.AddListener(() => OnCategorySelected(category));
            }
        }
    }
    
    private void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        codexPanel.SetActive(isPanelVisible);
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
            Button subcategoryButton = CreateSubcategoryButton(subcategory);
            subcategoryButton.transform.SetParent(subcategoryContainer, false);
        }
    }
    
    private Button CreateSubcategoryButton(string subcategory)
    {
        GameObject buttonObj = new GameObject(subcategory);
        Button button = buttonObj.AddComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.AddComponent<TextMeshProUGUI>();
        
        buttonText.text = subcategory;
        buttonText.alignment = TextAlignmentOptions.Left;
        
        button.onClick.AddListener(() => OnSubcategorySelected(subcategory));
        
        return button;
    }
    
    private void OnSubcategorySelected(string subcategory)
    {
        currentSubcategory = subcategory;
        UpdateEntriesUI();
    }
    
    private void UpdateEntriesUI()
    {
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }
        
        var entries = codexData.entries
            .Where(e => e.category == currentCategory.ToString() && e.subcategory == currentSubcategory);
            
        foreach (var entry in entries)
        {
            Button entryButton = CreateEntryButton(entry);
            entryButton.transform.SetParent(entryContainer, false);
        }
    }
    
    private Button CreateEntryButton(CodexEntry entry)
    {
        GameObject buttonObj = new GameObject(entry.title);
        Button button = buttonObj.AddComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.AddComponent<TextMeshProUGUI>();
        
        buttonText.text = entry.title;
        buttonText.alignment = TextAlignmentOptions.Left;
        
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