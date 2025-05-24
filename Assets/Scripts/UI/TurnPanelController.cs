using UnityEngine;
using TMPro;
using TurnClash.Units;
using System.Collections;

namespace TurnClash.UI
{
    /// <summary>
    /// Simple controller for the TurnPanel UI that displays current player turn information
    /// </summary>
    public class TurnPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI turnText;
        
        [Header("Player Colors")]
        [SerializeField] private Color player1Color = new Color(0.2f, 0.4f, 1f, 1f); // Nice blue
        [SerializeField] private Color player2Color = new Color(1f, 0.3f, 0.3f, 1f); // Nice red
        
        [Header("Text Settings")]
        [SerializeField] private bool useRichText = true;
        [SerializeField] private bool showMoveCount = true;
        [SerializeField] private bool showTurnNumber = true; // Show turn counter
        [SerializeField] private string turnTextFormat = "{0}'s Turn";
        [SerializeField] private string turnWithMovesFormat = "{0}'s Turn\n<size=75%>Action {1}/{2}</size>";
        [SerializeField] private string turnWithNumberFormat = "Turn {0} - {1}'s Turn";
        [SerializeField] private string turnWithNumberAndMovesFormat = "Turn {0} - {1}'s Turn\n<size=75%>Action {2}/{3}</size>";
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoFindTurnText = true;
        [SerializeField] private bool autoFindTurnManager = true;
        
        private TurnManager turnManager;
        
        private void Start()
        {
            // Auto-find TurnText if not assigned
            if (autoFindTurnText && turnText == null)
            {
                turnText = GetComponentInChildren<TextMeshProUGUI>();
                if (turnText == null)
                {
                    // Try to find by name
                    Transform turnTextTransform = transform.Find("TurnText");
                    if (turnTextTransform != null)
                    {
                        turnText = turnTextTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
            }
            
            // Validate TurnText setup
            if (turnText == null)
            {
                Debug.LogError("TurnPanelController: No TurnText found! Please assign the TextMeshProUGUI component.");
                enabled = false;
                return;
            }
            
            // Enable rich text if specified
            if (useRichText)
            {
                turnText.richText = true;
            }
            
            // Start coroutine to wait for TurnManager to be ready
            StartCoroutine(WaitForTurnManagerAndSubscribe());
        }
        
        private System.Collections.IEnumerator WaitForTurnManagerAndSubscribe()
        {
            // Wait up to 5 seconds for TurnManager to be ready
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                // Auto-find TurnManager
                if (autoFindTurnManager)
                {
                    turnManager = TurnManager.Instance;
                }
                
                if (turnManager != null)
                {
                    // Subscribe to turn events
                    turnManager.OnTurnStart += OnTurnStart;
                    turnManager.OnTurnEnd += OnTurnEnd;
                    turnManager.OnMoveCountChanged += OnMoveCountChanged;
                    turnManager.OnTurnNumberChanged += OnTurnNumberChanged;
                    
                    // Initialize display
                    UpdateTurnDisplay();
                    
                    Debug.Log("TurnPanelController: Successfully initialized and connected to TurnManager");
                    yield break; // Exit the coroutine
                }
                
                // Wait a frame and try again
                yield return null;
                elapsed += Time.deltaTime;
            }
            
            // If we reach here, we timed out
            Debug.LogError("TurnPanelController: Timeout waiting for TurnManager.Instance!");
            enabled = false;
        }
        
        private void OnTurnStart(Unit.Player player)
        {
            UpdateTurnDisplay();
        }
        
        private void OnTurnEnd(Unit.Player player)
        {
            UpdateTurnDisplay();
        }
        
        private void OnMoveCountChanged(Unit.Player player, int currentMoves, int maxMoves)
        {
            UpdateTurnDisplay();
        }
        
        private void OnTurnNumberChanged(int turnNumber)
        {
            UpdateTurnDisplay();
        }
        
        private void UpdateTurnDisplay()
        {
            if (turnText == null || turnManager == null || !turnManager.IsGameStarted)
            {
                if (turnText != null)
                {
                    turnText.text = "Game Starting...";
                    turnText.color = Color.white;
                }
                return;
            }
            
            Unit.Player currentPlayer = turnManager.CurrentPlayer;
            Color playerColor = GetPlayerColor(currentPlayer);
            string playerName = GetPlayerDisplayName(currentPlayer);
            int turnNumber = turnManager.CurrentTurnNumber;
            
            string displayText;
            
            // Determine which format to use based on settings
            if (showTurnNumber && showMoveCount)
            {
                // Show both turn number and move count
                int currentMove = turnManager.CurrentMoveCount + 1; // Show as 1-indexed
                int maxMoves = turnManager.MaxMovesPerTurn;
                
                if (useRichText)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGBA(playerColor);
                    displayText = string.Format(turnWithNumberAndMovesFormat, 
                        turnNumber,
                        $"<color=#{colorHex}>{playerName}</color>", 
                        currentMove, 
                        maxMoves);
                }
                else
                {
                    displayText = string.Format(turnWithNumberAndMovesFormat, turnNumber, playerName, currentMove, maxMoves);
                }
            }
            else if (showTurnNumber)
            {
                // Show turn number but not move count
                if (useRichText)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGBA(playerColor);
                    displayText = string.Format(turnWithNumberFormat, 
                        turnNumber,
                        $"<color=#{colorHex}>{playerName}</color>");
                }
                else
                {
                    displayText = string.Format(turnWithNumberFormat, turnNumber, playerName);
                }
            }
            else if (showMoveCount)
            {
                // Show move count but not turn number (original behavior)
                int currentMove = turnManager.CurrentMoveCount + 1; // Show as 1-indexed
                int maxMoves = turnManager.MaxMovesPerTurn;
                
                if (useRichText)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGBA(playerColor);
                    displayText = string.Format(turnWithMovesFormat, 
                        $"<color=#{colorHex}>{playerName}</color>", 
                        currentMove, 
                        maxMoves);
                }
                else
                {
                    displayText = string.Format(turnWithMovesFormat, playerName, currentMove, maxMoves);
                }
            }
            else
            {
                // Show only player name (original simple format)
                if (useRichText)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGBA(playerColor);
                    displayText = string.Format(turnTextFormat, $"<color=#{colorHex}>{playerName}</color>");
                }
                else
                {
                    displayText = string.Format(turnTextFormat, playerName);
                }
            }
            
            turnText.text = displayText;
            
            // Set the base color (this affects non-rich text portions)
            if (!useRichText)
            {
                turnText.color = playerColor;
            }
            else
            {
                turnText.color = Color.white; // Let rich text handle the coloring
            }
        }
        
        private Color GetPlayerColor(Unit.Player player)
        {
            return player == Unit.Player.Player1 ? player1Color : player2Color;
        }
        
        private string GetPlayerDisplayName(Unit.Player player)
        {
            return player == Unit.Player.Player1 ? "Player 1" : "Player 2";
        }
        
        /// <summary>
        /// Manually set the turn text component
        /// </summary>
        public void SetTurnText(TextMeshProUGUI textComponent)
        {
            turnText = textComponent;
            if (useRichText && turnText != null)
            {
                turnText.richText = true;
            }
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Set custom player colors
        /// </summary>
        public void SetPlayerColors(Color player1Col, Color player2Col)
        {
            player1Color = player1Col;
            player2Color = player2Col;
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Toggle rich text formatting
        /// </summary>
        public void SetRichTextEnabled(bool enabled)
        {
            useRichText = enabled;
            if (turnText != null)
            {
                turnText.richText = enabled;
            }
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Toggle move count display
        /// </summary>
        public void SetShowMoveCount(bool show)
        {
            showMoveCount = show;
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Toggle turn number display
        /// </summary>
        public void SetShowTurnNumber(bool show)
        {
            showTurnNumber = show;
            UpdateTurnDisplay();
        }
        
        /// <summary>
        /// Force update the display (useful for testing)
        /// </summary>
        [ContextMenu("Update Display")]
        public void ForceUpdateDisplay()
        {
            UpdateTurnDisplay();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (turnManager != null)
            {
                turnManager.OnTurnStart -= OnTurnStart;
                turnManager.OnTurnEnd -= OnTurnEnd;
                turnManager.OnMoveCountChanged -= OnMoveCountChanged;
                turnManager.OnTurnNumberChanged -= OnTurnNumberChanged;
            }
        }
    }
} 