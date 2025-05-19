using UnityEngine;

public class ToggleMenu : MonoBehaviour
{
    [SerializeField] private KeyCode codexKey = KeyCode.C;
    private GameObject codexPanel;
    private CodexPanelManager codexManager;
    private bool wasKeyPressed = false;

    private void Start()
    {
        // Find the codex panel and its manager
        codexPanel = GameObject.Find("CodexPanel");
        if (codexPanel == null)
        {
            Debug.LogError("CodexPanel not found in scene!");
            return;
        }

        codexManager = codexPanel.GetComponent<CodexPanelManager>();
        if (codexManager == null)
        {
            Debug.LogError("CodexPanelManager not found on CodexPanel!");
            return;
        }
    }

    private void Update()
    {
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
            codexManager.TogglePanel();
        }
    }
} 