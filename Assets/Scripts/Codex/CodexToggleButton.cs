using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TurnClash.Units;

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
        // Check if game is over - don't allow codex access when victory screen is active
        if (IsGameOver())
        {
            Debug.Log("CodexToggleButton: Cannot open codex - game is over (victory screen active)");
            return;
        }
        
        if (codexManager != null)
        {
            codexManager.TogglePanel();
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