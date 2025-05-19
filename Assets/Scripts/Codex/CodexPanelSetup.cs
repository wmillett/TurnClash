using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodexPanelSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CodexPanelManager panelManager;
    
    private void Awake()
    {
        if (panelManager == null)
        {
            panelManager = GetComponent<CodexPanelManager>();
        }
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Create main panel
        GameObject panel = new GameObject("CodexPanel");
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Set panel size and position
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 0);
        
        // Create toggle button
        GameObject toggleButton = CreateButton("ToggleButton", new Vector2(-50, 0));
        toggleButton.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
        toggleButton.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
        toggleButton.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        toggleButton.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        
        // Create menu page
        GameObject menuPage = CreatePage("MenuPage");
        
        // Create category container
        GameObject categoryContainer = CreateContainer("CategoryContainer", menuPage.transform);
        categoryContainer.GetComponent<VerticalLayoutGroup>().spacing = 10;
        
        // Create category buttons
        string[] categories = { "Characters", "History", "Locations", "Organizations" };
        foreach (string category in categories)
        {
            GameObject categoryButton = CreateButton(category, Vector2.zero);
            categoryButton.transform.SetParent(categoryContainer.transform, false);
            TextMeshProUGUI buttonText = categoryButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = category;
            buttonText.fontSize = 18;
            buttonText.fontStyle = FontStyles.Bold;
        }
        
        // Create subcategory container
        GameObject subcategoryContainer = CreateContainer("SubcategoryContainer", menuPage.transform);
        subcategoryContainer.GetComponent<VerticalLayoutGroup>().spacing = 5;
        
        // Create entry page
        GameObject entryPage = CreatePage("EntryPage");
        entryPage.SetActive(false);
        
        // Create back button
        GameObject backButton = CreateButton("BackButton", new Vector2(10, -10));
        backButton.transform.SetParent(entryPage.transform, false);
        backButton.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        backButton.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        backButton.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
        
        // Create entry content
        GameObject entryContent = new GameObject("EntryContent");
        entryContent.transform.SetParent(entryPage.transform, false);
        RectTransform contentRect = entryContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(20, 50);
        contentRect.offsetMax = new Vector2(-20, -20);
        
        // Create entry title
        GameObject titleObj = CreateText("Title", entryContent.transform);
        titleObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        titleObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        titleObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
        titleObj.GetComponent<TextMeshProUGUI>().fontSize = 24;
        titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // Create entry image
        GameObject imageObj = new GameObject("Image");
        imageObj.transform.SetParent(entryContent.transform, false);
        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        Image image = imageObj.AddComponent<Image>();
        imageRect.anchorMin = new Vector2(0, 0);
        imageRect.anchorMax = new Vector2(1, 0);
        imageRect.pivot = new Vector2(0.5f, 0);
        imageRect.sizeDelta = new Vector2(0, 200);
        
        // Create entry description
        GameObject descObj = CreateText("Description", entryContent.transform);
        descObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        descObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
        descObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        descObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100);
        descObj.GetComponent<TextMeshProUGUI>().fontSize = 16;
        
        // Assign references to panel manager
        panelManager.codexPanel = panelRect;
        panelManager.menuPage = menuPage;
        panelManager.entryPage = entryPage;
        panelManager.toggleButton = toggleButton.GetComponent<Button>();
        panelManager.backButton = backButton.GetComponent<Button>();
        panelManager.categoryContainer = categoryContainer.transform;
        panelManager.subcategoryContainer = subcategoryContainer.transform;
        panelManager.entryTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        panelManager.entryDescriptionText = descObj.GetComponent<TextMeshProUGUI>();
        panelManager.entryImage = image;
    }
    
    private GameObject CreateButton(string name, Vector2 position)
    {
        GameObject button = new GameObject(name);
        RectTransform rectTransform = button.AddComponent<RectTransform>();
        Image image = button.AddComponent<Image>();
        Button buttonComponent = button.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        return button;
    }
    
    private GameObject CreateContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(20, 20);
        rectTransform.offsetMax = new Vector2(-20, -20);
        
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        
        return container;
    }
    
    private GameObject CreatePage(string name)
    {
        GameObject page = new GameObject(name);
        page.transform.SetParent(transform, false);
        RectTransform rectTransform = page.AddComponent<RectTransform>();
        
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        return page;
    }
    
    private GameObject CreateText(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;
        
        return textObj;
    }
} 