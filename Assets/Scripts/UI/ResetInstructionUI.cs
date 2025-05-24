using UnityEngine;
using TMPro;
using TurnClash.Units;

namespace TurnClash.UI
{
    /// <summary>
    /// UI component that displays reset instructions
    /// Particularly useful during game over or as a permanent help display
    /// </summary>
    public class ResetInstructionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI resetInstructionText;
        [SerializeField] private GameObject resetPanel;
        
        [Header("Display Settings")]
        [SerializeField] private bool showOnGameOver = true;
        [SerializeField] private bool showPermanently = false;
        [SerializeField] private bool autoFindGameManager = true;
        [SerializeField] private bool autoFindCombatManager = true;
        
        [Header("Text Settings")]
        [SerializeField] private string resetMessage = "Press 'R' to Reset Game";
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private bool flashText = false;
        [SerializeField] private float flashSpeed = 2f;
        
        private GameManager gameManager;
        private CombatManager combatManager;
        private bool isGameOver = false;
        private float flashTimer = 0f;
        
        private void Start()
        {
            // Auto-find managers
            if (autoFindGameManager)
            {
                gameManager = GameManager.Instance;
            }
            
            if (autoFindCombatManager)
            {
                combatManager = CombatManager.Instance;
            }
            
            // Auto-find reset instruction text if not assigned
            if (resetInstructionText == null)
            {
                resetInstructionText = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            // Subscribe to game over events
            if (combatManager != null && showOnGameOver)
            {
                combatManager.OnPlayerEliminationCheck += OnGameOver;
            }
            
            // Set up initial display
            UpdateDisplay();
            
            Debug.Log("ResetInstructionUI: Initialized");
        }
        
        private void Update()
        {
            // Handle text flashing if enabled
            if (flashText && resetInstructionText != null && ShouldShowInstructions())
            {
                flashTimer += Time.deltaTime * flashSpeed;
                float alpha = (Mathf.Sin(flashTimer) + 1f) / 2f; // Normalize to 0-1
                Color currentColor = textColor;
                currentColor.a = Mathf.Lerp(0.3f, 1f, alpha);
                resetInstructionText.color = currentColor;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (combatManager != null)
            {
                combatManager.OnPlayerEliminationCheck -= OnGameOver;
            }
        }
        
        private void OnGameOver(Unit.Player eliminatedPlayer)
        {
            isGameOver = true;
            UpdateDisplay();
            Debug.Log("ResetInstructionUI: Game over detected, showing reset instructions");
        }
        
        private void UpdateDisplay()
        {
            bool shouldShow = ShouldShowInstructions();
            
            // Update panel visibility
            if (resetPanel != null)
            {
                resetPanel.SetActive(shouldShow);
            }
            
            // Update text
            if (resetInstructionText != null)
            {
                resetInstructionText.gameObject.SetActive(shouldShow);
                
                if (shouldShow)
                {
                    resetInstructionText.text = resetMessage;
                    if (!flashText)
                    {
                        resetInstructionText.color = textColor;
                    }
                }
            }
        }
        
        private bool ShouldShowInstructions()
        {
            if (showPermanently) return true;
            if (showOnGameOver && isGameOver) return true;
            return false;
        }
        
        /// <summary>
        /// Manually set the reset instruction text
        /// </summary>
        public void SetResetMessage(string message)
        {
            resetMessage = message;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Show or hide the reset instructions
        /// </summary>
        public void ShowInstructions(bool show)
        {
            if (resetPanel != null)
            {
                resetPanel.SetActive(show);
            }
            
            if (resetInstructionText != null)
            {
                resetInstructionText.gameObject.SetActive(show);
            }
        }
        
        /// <summary>
        /// Set whether instructions should be shown permanently
        /// </summary>
        public void SetShowPermanently(bool permanent)
        {
            showPermanently = permanent;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Enable or disable text flashing effect
        /// </summary>
        public void SetFlashText(bool flash)
        {
            flashText = flash;
            if (!flash && resetInstructionText != null)
            {
                resetInstructionText.color = textColor;
            }
        }
    }
} 