using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CodexToggleButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    private CodexPanelManager codexManager;
    private bool wasKeyPressed = false;

    private void Start()
    {
        // Find the codex panel and its manager
        if (codexPanel == null)
        {
            codexPanel = GameObject.Find("CodexPanel");
            if (codexPanel == null)
            {
                Debug.LogError("CodexPanel not found in scene!");
                return;
            }
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
        bool isKeyPressed = Input.GetKey(toggleKey);
        
        // Only toggle when the key is first pressed (not held)
        if (isKeyPressed && !wasKeyPressed)
        {
            ToggleCodex();
        }
        
        wasKeyPressed = isKeyPressed;
    }

    // Implement IPointerClickHandler for Event System
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleCodex();
    }

    private void ToggleCodex()
    {
        if (codexManager != null)
        {
            codexManager.TogglePanel();
        }
    }
} 