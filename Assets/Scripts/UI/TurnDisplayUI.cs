using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;
using System.Collections;

namespace TurnClash.UI
{
    /// <summary>
    /// UI component that displays current turn information
    /// </summary>
    public class TurnDisplayUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI movesRemainingText;
        [SerializeField] private TextMeshProUGUI turnInfoText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        [Header("Player Colors")]
        [SerializeField] private Color player1Color = Color.blue;
        [SerializeField] private Color player2Color = Color.red;
        
        [Header("Settings")]
        [SerializeField] private bool autoFindTurnManager = true;
        [SerializeField] private bool showInstructions = true;
        
        private TurnManager turnManager;
        
        private void Start()
        {
            // Start coroutine to wait for TurnManager to be ready
            StartCoroutine(WaitForTurnManagerAndSubscribe());
            
            // Set up instructions
            if (showInstructions && instructionsText != null)
            {
                instructionsText.text = "Arrow Keys: Move Unit | X: End Turn | R: Reset Game";
            }
        }
        
        private System.Collections.IEnumerator WaitForTurnManagerAndSubscribe()
        {
            // Wait up to 5 seconds for TurnManager to be ready
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                // Find turn manager
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
                    turnManager.OnMoveUsed += OnMoveUsed;
                    
                    // Initialize display
                    UpdateDisplay();
                    Debug.Log("TurnDisplayUI: Successfully subscribed to turn events");
                    yield break; // Exit the coroutine
                }
                
                // Wait a frame and try again
                yield return null;
                elapsed += Time.deltaTime;
            }
            
            // If we reach here, we timed out
            Debug.LogWarning("TurnDisplayUI: Timeout waiting for TurnManager.Instance! Events not subscribed.");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (turnManager != null)
            {
                turnManager.OnTurnStart -= OnTurnStart;
                turnManager.OnTurnEnd -= OnTurnEnd;
                turnManager.OnMoveCountChanged -= OnMoveCountChanged;
                turnManager.OnMoveUsed -= OnMoveUsed;
            }
        }
        
        private void OnTurnStart(Unit.Player player)
        {
            UpdateDisplay();
        }
        
        private void OnTurnEnd(Unit.Player player)
        {
            UpdateDisplay();
        }
        
        private void OnMoveCountChanged(Unit.Player player, int currentMoves, int maxMoves)
        {
            UpdateDisplay();
        }
        
        private void OnMoveUsed(Unit.Player player, int remainingMoves)
        {
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (turnManager == null || !turnManager.IsGameStarted)
            {
                SetDefaultDisplay();
                return;
            }
            
            // Update current player text
            if (currentPlayerText != null)
            {
                currentPlayerText.text = $"{turnManager.CurrentPlayer}'s Turn";
                currentPlayerText.color = GetPlayerColor(turnManager.CurrentPlayer);
            }
            
            // Update moves remaining text
            if (movesRemainingText != null)
            {
                movesRemainingText.text = $"Moves: {turnManager.GetRemainingMoves()}/{turnManager.MaxMovesPerTurn}";
            }
            
            // Update combined turn info text
            if (turnInfoText != null)
            {
                turnInfoText.text = turnManager.GetTurnInfo();
                turnInfoText.color = GetPlayerColor(turnManager.CurrentPlayer);
            }
        }
        
        private void SetDefaultDisplay()
        {
            if (currentPlayerText != null)
                currentPlayerText.text = "Game Starting...";
                
            if (movesRemainingText != null)
                movesRemainingText.text = "Moves: -/-";
                
            if (turnInfoText != null)
                turnInfoText.text = "Waiting for game to start...";
        }
        
        private Color GetPlayerColor(Unit.Player player)
        {
            return player == Unit.Player.Player1 ? player1Color : player2Color;
        }
        
        /// <summary>
        /// Manually set the turn manager reference
        /// </summary>
        public void SetTurnManager(TurnManager manager)
        {
            if (turnManager != null)
            {
                // Unsubscribe from old manager
                turnManager.OnTurnStart -= OnTurnStart;
                turnManager.OnTurnEnd -= OnTurnEnd;
                turnManager.OnMoveCountChanged -= OnMoveCountChanged;
                turnManager.OnMoveUsed -= OnMoveUsed;
            }
            
            turnManager = manager;
            
            if (turnManager != null)
            {
                // Subscribe to new manager
                turnManager.OnTurnStart += OnTurnStart;
                turnManager.OnTurnEnd += OnTurnEnd;
                turnManager.OnMoveCountChanged += OnMoveCountChanged;
                turnManager.OnMoveUsed += OnMoveUsed;
                
                UpdateDisplay();
            }
        }
        
        /// <summary>
        /// Set player colors
        /// </summary>
        public void SetPlayerColors(Color player1Col, Color player2Col)
        {
            player1Color = player1Col;
            player2Color = player2Col;
            UpdateDisplay();
        }
    }
} 