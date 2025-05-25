using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TurnClash.Units;

public class CodexToggleButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private bool debugToggle = false;
    [SerializeField] private bool enableKeyboardInput = false; // Disabled to avoid conflicts with ToggleMenu
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
        
        if (debugToggle)
            Debug.Log($"CodexToggleButton: Initialized successfully, listening for {toggleKey} key");
    }

    private void Update()
    {
        // Only handle keyboard input if enabled (disabled by default to avoid conflicts)
        if (!enableKeyboardInput)
            return;
            
        // Check for keyboard input
        bool isKeyPressed = Input.GetKey(toggleKey);
        
        // Only toggle when the key is first pressed (not held)
        if (isKeyPressed && !wasKeyPressed)
        {
            if (debugToggle)
                Debug.Log($"CodexToggleButton: {toggleKey} key pressed, toggling codex");
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
        // Check if game is over - don't allow codex access when victory screen is active
        if (IsGameOver())
        {
            if (debugToggle)
                Debug.Log("CodexToggleButton: Cannot open codex - game is over (victory screen active)");
            return;
        }
        
        if (codexManager != null)
        {
            if (debugToggle)
                Debug.Log("CodexToggleButton: Calling TogglePanel on CodexPanelManager");
            codexManager.TogglePanel();
        }
        else
        {
            Debug.LogError("CodexToggleButton: CodexPanelManager is null!");
        }
    }
    
    /// <summary>
    /// Check if the game is over by seeing if any player has been eliminated
    /// </summary>
    private bool IsGameOver()
    {
        // Try to find the CombatManager to check player elimination
        var combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            // Check if either player has been eliminated
            return combatManager.IsPlayerEliminated(Unit.Player.Player1) || 
                   combatManager.IsPlayerEliminated(Unit.Player.Player2);
        }
        
        return false; // If no combat manager found, assume game is still active
    }
} 