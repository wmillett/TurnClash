using UnityEngine;

public class ToggleMenu : MonoBehaviour
{
    [SerializeField] private KeyCode codexKey = KeyCode.C;
    [SerializeField] private bool debugToggle = false;
    
    private GameObject codexPanel;
    private CodexPanelManager codexManager;
    private bool wasKeyPressed = false;
    private bool codexSystemAvailable = false;

    private void Start()
    {
        // Try to find the codex system - it might not exist in all scenes
        FindCodexSystem();
    }
    
    private void FindCodexSystem()
    {
        // First try to find by name
        codexPanel = GameObject.Find("CodexPanel");
        
        // If not found by name, try to find CodexPanelManager component
        if (codexPanel == null)
        {
            codexManager = FindObjectOfType<CodexPanelManager>();
            if (codexManager != null)
            {
                codexPanel = codexManager.gameObject;
                if (debugToggle)
                    Debug.Log($"ToggleMenu: Found CodexPanelManager on {codexPanel.name}");
            }
        }
        else
        {
            codexManager = codexPanel.GetComponent<CodexPanelManager>();
        }
        
        // Check if we have a working codex system
        if (codexPanel != null && codexManager != null)
        {
            codexSystemAvailable = true;
            if (debugToggle)
                Debug.Log($"ToggleMenu: Codex system found and ready (Panel: {codexPanel.name})");
        }
        else
        {
            codexSystemAvailable = false;
            if (debugToggle)
                Debug.Log("ToggleMenu: No codex system found in this scene - C key will be ignored");
        }
    }

    private void Update()
    {
        // Only process input if codex system is available
        if (!codexSystemAvailable)
            return;
            
        // Check for keyboard input
        bool isKeyPressed = Input.GetKey(codexKey);
        
        // Only toggle when the key is first pressed (not held)
        if (isKeyPressed && !wasKeyPressed)
        {
            ToggleCodex();
        }
        
        wasKeyPressed = isKeyPressed;
    }

    private void ToggleCodex()
    {
        if (codexManager != null)
        {
            if (debugToggle)
                Debug.Log("ToggleMenu: Toggling codex panel");
            codexManager.TogglePanel();
        }
    }
    
    /// <summary>
    /// Public method to check if codex system is available
    /// </summary>
    public bool IsCodexSystemAvailable()
    {
        return codexSystemAvailable;
    }
    
    /// <summary>
    /// Manually refresh the codex system search (useful if codex is spawned later)
    /// </summary>
    public void RefreshCodexSystem()
    {
        FindCodexSystem();
    }
} 