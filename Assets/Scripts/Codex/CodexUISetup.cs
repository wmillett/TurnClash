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
        SetupCategoryContainer();
        SetupSubcategoryContainer();
        SetupToggleButton();
        SetupInitialState();
    }

    private void SetupPanelPosition()
    {
        if (codexPanel != null)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            codexPanel.anchorMin = new Vector2(1, 0.5f);
            codexPanel.anchorMax = new Vector2(1, 0.5f);
            codexPanel.pivot = new Vector2(1, 0.5f);
            
            float widthPercent = 0.3f;
            float heightPercent = 0.8f;
            
            codexPanel.sizeDelta = new Vector2(screenWidth * widthPercent, screenHeight * heightPercent);
            codexPanel.anchoredPosition = new Vector2(codexPanel.rect.width, 0);
        }
    }

    private void SetupCategoryContainer()
    {
        if (categoryContainer != null)
        {
            RectTransform categoryRect = categoryContainer.GetComponent<RectTransform>();
            if (categoryRect != null)
            {
                categoryRect.localScale = Vector3.one;
                categoryRect.anchorMin = new Vector2(0, 1);
                categoryRect.anchorMax = new Vector2(1, 1);
                categoryRect.pivot = new Vector2(0.5f, 1);
                categoryRect.anchoredPosition = new Vector2(0, -5);
                categoryRect.sizeDelta = new Vector2(0, 50);
            }
            
            ResizeCategoryButtons();
        }
    }

    private void SetupSubcategoryContainer()
    {
        if (subcategoryContainer != null)
        {
            RectTransform subcategoryRect = subcategoryContainer.GetComponent<RectTransform>();
            if (subcategoryRect != null)
            {
                subcategoryRect.anchorMin = new Vector2(0, 0);
                subcategoryRect.anchorMax = new Vector2(1, 1);
                subcategoryRect.pivot = new Vector2(0.5f, 0.5f);
                subcategoryRect.anchoredPosition = new Vector2(0, -60);
                subcategoryRect.offsetMin = new Vector2(10, 10);
                subcategoryRect.offsetMax = new Vector2(-10, -60);
                
                SetupSubcategoryLayoutGroup();
            }
        }
    }

    private void SetupSubcategoryLayoutGroup()
    {
        VerticalLayoutGroup layoutGroup = subcategoryContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = subcategoryContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
        }
        
        ContentSizeFitter sizeFitter = subcategoryContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = subcategoryContainer.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
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
        menuPage.SetActive(true);
        entryPage.SetActive(false);
        
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
        foreach (Button button in categoryButtons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(120, 40);
                    
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.fontSize = 18;
                    }
                }
            }
        }
    }
} 