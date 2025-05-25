using UnityEngine;
using UnityEngine.UI;
using TurnClash.Units;

namespace TurnClash.UI
{
    /// <summary>
    /// UI Button component that allows players to skip their turn early
    /// Provides the same functionality as pressing the X key
    /// </summary>
    public class SkipTurnButton : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button skipButton;
        
        [Header("Settings")]
        [SerializeField] private bool autoFindButton = true;
        [SerializeField] private bool debugSkip = false;
        
        private TurnManager turnManager;
        
        private void Start()
        {
            // Auto-find button if not assigned
            if (autoFindButton && skipButton == null)
            {
                skipButton = GetComponent<Button>();
                if (skipButton == null)
                {
                    Debug.LogError("SkipTurnButton: No Button component found! Please assign or add Button component.");
                    enabled = false;
                    return;
                }
            }
            
            // Validate button reference
            if (skipButton == null)
            {
                Debug.LogError("SkipTurnButton: No button assigned!");
                enabled = false;
                return;
            }
            
            // Find turn manager
            turnManager = TurnManager.Instance;
            if (turnManager == null)
            {
                Debug.LogError("SkipTurnButton: TurnManager.Instance not found!");
                enabled = false;
                return;
            }
            
            // Set up button click listener
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            
            if (debugSkip)
                Debug.Log("SkipTurnButton: Successfully initialized and connected to TurnManager");
        }
        
        private void OnSkipButtonClicked()
        {
            if (debugSkip)
                Debug.Log("SkipTurnButton: Skip button clicked");
            
            // Check if game is over - don't allow turn skipping when victory screen is active
            if (IsGameOver())
            {
                if (debugSkip)
                    Debug.Log("SkipTurnButton: Cannot skip turn - game is over (victory screen active)");
                return;
            }
            
            // Check if turn manager exists and game is started
            if (turnManager == null || !turnManager.IsGameStarted)
            {
                if (debugSkip)
                    Debug.Log("SkipTurnButton: Cannot skip turn - turn manager not ready or game not started");
                return;
            }
            
            // Call the same method that X key uses
            turnManager.EndTurnEarly();
            
            if (debugSkip)
                Debug.Log($"SkipTurnButton: Turn skipped for {turnManager.CurrentPlayer}");
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
        
        /// <summary>
        /// Enable or disable the skip button
        /// </summary>
        public void SetButtonEnabled(bool enabled)
        {
            if (skipButton != null)
            {
                skipButton.interactable = enabled;
            }
        }
        
        /// <summary>
        /// Manually set the button reference
        /// </summary>
        public void SetButton(Button button)
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipButtonClicked);
            }
            
            skipButton = button;
            
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipButtonClicked);
            }
        }
    }
} 