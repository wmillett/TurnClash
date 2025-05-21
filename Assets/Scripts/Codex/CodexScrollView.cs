using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CodexScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private float entrySpacing = 15f;
    [SerializeField] private float entryHeight = 60f;
    [SerializeField] private float paddingTop = 20f;
    [SerializeField] private float paddingBottom = 20f;
    
    // Public accessor for content container
    public RectTransform content => contentContainer;
    
    // Reference to parent CodexPanelManager
    private CodexPanelManager codexPanelManager;
    
    private void Awake()
    {
        codexPanelManager = GetComponentInParent<CodexPanelManager>();
        
        if (contentContainer == null)
        {
            // Find the content container if not assigned
            ScrollRect scrollRect = GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                contentContainer = scrollRect.content;
            }
        }
        
        if (contentContainer == null)
        {
            Debug.LogError("Content container not found in ScrollView. Make sure the ScrollRect has a content RectTransform assigned.");
        }
    }
    
    /// <summary>
    /// Populate the scroll view with entry buttons based on provided entries
    /// </summary>
    public void PopulateEntries(List<CodexEntry> entries)
    {
        if (contentContainer == null)
        {
            Debug.LogError("Content container not assigned in CodexScrollView!");
            return;
        }
        
        // Clear existing entries from content container
        ClearEntries();
        
        if (entries == null || entries.Count == 0)
        {
            Debug.Log("No entries to display");
            return;
        }
        
        // Calculate total height needed for content
        float totalContentHeight = (entries.Count * entryHeight) + 
                                  ((entries.Count - 1) * entrySpacing) + 
                                  paddingTop + paddingBottom;
        
        // Resize content container
        contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, totalContentHeight);
        
        // Create entry buttons
        float yPosition = -paddingTop;
        
        for (int i = 0; i < entries.Count; i++)
        {
            CodexEntry entry = entries[i];
            
            // Create entry button
            GameObject entryObj;
            
            if (entryPrefab != null)
            {
                // Instantiate as child of content container
                entryObj = Instantiate(entryPrefab, contentContainer);
            }
            else
            {
                entryObj = CreateEntryButton(entry.title);
                // Set parent to content container
                entryObj.transform.SetParent(contentContainer, false);
            }
            
            // Position entry within the content container
            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect == null)
            {
                // If it doesn't have a RectTransform, add one
                entryRect = entryObj.AddComponent<RectTransform>();
            }
            
            entryRect.anchorMin = new Vector2(0, 1);
            entryRect.anchorMax = new Vector2(1, 1);
            entryRect.pivot = new Vector2(0.5f, 1);
            entryRect.sizeDelta = new Vector2(0, entryHeight);
            entryRect.anchoredPosition = new Vector2(0, yPosition);
            
            // Set up the button click event
            Button entryButton = entryObj.GetComponent<Button>();
            if (entryButton != null)
            {
                CodexEntry entryCopy = entry; // Create a copy to avoid closure issues
                entryButton.onClick.AddListener(() => OnEntryClicked(entryCopy));
                
                // Set entry title
                TextMeshProUGUI titleText = entryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = entry.title;
                }
            }
            
            // Update y position for next entry
            yPosition -= (entryHeight + entrySpacing);
        }
        
        // Reset scroll position to top
        contentContainer.anchoredPosition = Vector2.zero;
    }
    
    private void OnEntryClicked(CodexEntry entry)
    {
        if (codexPanelManager != null)
        {
            codexPanelManager.ShowEntry(entry);
        }
    }
    
    private GameObject CreateEntryButton(string title)
    {
        GameObject buttonObj = new GameObject("EntryButton");
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        Image background = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();
        
        // Set up background
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Set up button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.pressedColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
        colors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Title");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(buttonObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        // Set up text
        text.text = title;
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        // Set up text rect
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(15, 5);
        textRect.offsetMax = new Vector2(-15, -5);
        
        return buttonObj;
    }
    
    private void ClearEntries()
    {
        // Remove all child objects from content container
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("Content container is null, cannot clear entries");
        }
    }
} 