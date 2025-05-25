using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;
using System.Collections;

namespace TurnClash.UI
{
    /// <summary>
    /// Controller for the VictoryPanel UI that displays game over screen and handles exit/reset functionality
    /// </summary>
    public class VictoryPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject victoryPanel; // This should be assigned to the actual panel GameObject
        [SerializeField] private TextMeshProUGUI victoryText;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button resetButton;
        
        [Header("Victory Settings")]
        [SerializeField] private string victoryTextFormat = "Player {0} has won!";
        [SerializeField] private Color player1VictoryColor = new Color(0.2f, 0.4f, 1f, 1f); // Nice blue
        [SerializeField] private Color player2VictoryColor = new Color(1f, 0.3f, 0.3f, 1f); // Nice red
        [SerializeField] private bool useRichText = true;
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool autoFindCombatManager = true;
        [SerializeField] private bool autoFindGameManager = true;
        
        [Header("Panel Behavior")]
        [SerializeField] private bool hideOnStart = true;
        [SerializeField] private bool showStatistics = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        private CombatManager combatManager;
        private GameManager gameManager;
        private bool isGameOver = false;
        private bool isInitialized = false;
        private float initializationTimer = 0f;
        private const float MAX_INITIALIZATION_TIME = 5f;
        
        private void Start()
        {
            if (debugMode)
                Debug.Log("VictoryPanelController: Initializing on always-active GameObject...");
            
            // Auto-find components if not assigned
            if (autoFindComponents)
            {
                AutoFindUIComponents();
            }
            
            // Validate components
            if (!ValidateComponents())
            {
                enabled = false;
                return;
            }
            
            // Setup button listeners
            SetupButtonListeners();
            
            // Hide panel on start if specified
            if (hideOnStart && victoryPanel != null)
            {
                victoryPanel.SetActive(false);
                if (debugMode)
                    Debug.Log("VictoryPanelController: Victory panel hidden on start");
            }
            
            // Start initialization timer
            initializationTimer = 0f;
            isInitialized = false;
            
            if (debugMode)
                Debug.Log("VictoryPanelController: Starting manager initialization...");
        }
        
        private void Update()
        {
            // Handle manager initialization
            if (!isInitialized)
            {
                TryInitializeManagers();
            }
        }
        
        private void TryInitializeManagers()
        {
            initializationTimer += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            
            // Auto-find CombatManager
            if (autoFindCombatManager && combatManager == null)
            {
                combatManager = CombatManager.Instance;
                if (debugMode && combatManager != null)
                    Debug.Log("VictoryPanelController: Found CombatManager");
            }
            
            // Auto-find GameManager
            if (autoFindGameManager && gameManager == null)
            {
                gameManager = GameManager.Instance;
                if (debugMode && gameManager != null)
                    Debug.Log("VictoryPanelController: Found GameManager");
            }
            
            // Check if we have what we need
            if (combatManager != null)
            {
                // Subscribe to combat manager events
                combatManager.OnPlayerEliminationCheck += OnPlayerEliminated;
                
                isInitialized = true;
                if (debugMode)
                    Debug.Log("VictoryPanelController: Successfully connected to CombatManager - ready to monitor for game over");
                return;
            }
            
            // Check for timeout
            if (initializationTimer >= MAX_INITIALIZATION_TIME)
            {
                Debug.LogError("VictoryPanelController: Timeout waiting for CombatManager.Instance!");
                isInitialized = true; // Stop trying
            }
        }
        
        private void AutoFindUIComponents()
        {
            if (debugMode)
                Debug.Log("VictoryPanelController: Starting auto-find of UI components...");
            
            // Try to find VictoryPanel if not assigned
            if (victoryPanel == null)
            {
                // First try to find by name in children or siblings
                Transform panelTransform = transform.Find("VictoryPanel");
                if (panelTransform == null)
                {
                    // Try to find in parent's children (if this script is on Canvas, look for panel in Canvas)
                    if (transform.parent != null)
                    {
                        panelTransform = transform.parent.GetComponentInChildren<Transform>(true)
                            ?.Find("VictoryPanel");
                    }
                }
                
                // Try finding by searching all objects with "Victory" in the name
                if (panelTransform == null)
                {
                    GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.ToLower().Contains("victory") && obj.scene.isLoaded)
                        {
                            victoryPanel = obj;
                            break;
                        }
                    }
                }
                else
                {
                    victoryPanel = panelTransform.gameObject;
                }
            }
            
            // Try to find VictoryText
            if (victoryText == null)
            {
                if (victoryPanel != null)
                {
                    // Look for TextMeshProUGUI in the victory panel
                    victoryText = victoryPanel.GetComponentInChildren<TextMeshProUGUI>(true);
                }
                
                if (victoryText == null)
                {
                    // Try to find by name
                    Transform textTransform = null;
                    if (victoryPanel != null)
                    {
                        textTransform = victoryPanel.transform.Find("VictoryText");
                    }
                    
                    if (textTransform != null)
                    {
                        victoryText = textTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
            }
            
            // Try to find buttons
            Button[] buttons = null;
            if (victoryPanel != null)
            {
                buttons = victoryPanel.GetComponentsInChildren<Button>(true); // Include inactive buttons
            }
            
            if (buttons != null)
            {
                if (exitButton == null)
                {
                    foreach (Button button in buttons)
                    {
                        if (button.name.ToLower().Contains("exit"))
                        {
                            exitButton = button;
                            break;
                        }
                    }
                }
                
                if (resetButton == null)
                {
                    foreach (Button button in buttons)
                    {
                        if (button.name.ToLower().Contains("reset"))
                        {
                            resetButton = button;
                            break;
                        }
                    }
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"VictoryPanelController: Auto-found components - Panel: {victoryPanel != null}, Text: {victoryText != null}, Exit: {exitButton != null}, Reset: {resetButton != null}");
                if (victoryPanel != null) Debug.Log($"VictoryPanelController: Found panel: {victoryPanel.name}");
            }
        }
        
        private bool ValidateComponents()
        {
            bool allValid = true;
            
            if (victoryPanel == null)
            {
                Debug.LogError("VictoryPanelController: VictoryPanel is null! Please assign the panel GameObject.");
                allValid = false;
            }
            
            if (victoryText == null)
            {
                Debug.LogError("VictoryPanelController: VictoryText is null! Please assign the TextMeshProUGUI component.");
                allValid = false;
            }
            
            if (exitButton == null)
            {
                Debug.LogWarning("VictoryPanelController: ExitButton is null! Exit functionality will not work.");
            }
            
            if (resetButton == null)
            {
                Debug.LogWarning("VictoryPanelController: ResetButton is null! Reset functionality will not work.");
            }
            
            return allValid;
        }
        
        private void SetupButtonListeners()
        {
            // Setup exit button
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners(); // Clear any existing listeners
                exitButton.onClick.AddListener(OnExitButtonClicked);
                Debug.Log("VictoryPanelController: Exit button listener setup");
            }
            
            // Setup reset button
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners(); // Clear any existing listeners
                resetButton.onClick.AddListener(OnResetButtonClicked);
                Debug.Log("VictoryPanelController: Reset button listener setup");
            }
        }
        
        private void OnPlayerEliminated(Unit.Player eliminatedPlayer)
        {
            Debug.Log($"🚨🚨🚨 VICTORY PANEL: OnPlayerEliminated called for {eliminatedPlayer} 🚨🚨🚨");
            
            if (isGameOver)
            {
                Debug.LogWarning("🔴 VICTORY PANEL: Game already over, ignoring elimination event");
                return;
            }

            isGameOver = true;
            Unit.Player winner = (eliminatedPlayer == Unit.Player.Player1) ? Unit.Player.Player2 : Unit.Player.Player1;
            
            Debug.Log($"🏆🏆🏆 VICTORY PANEL: Determined winner is {winner} 🏆🏆🏆");
            
            ShowVictoryScreen(winner);
        }
        
        public void ShowVictoryScreen(Unit.Player winner)
        {
            Debug.Log($"🎉🎉🎉 VICTORY PANEL: ShowVictoryScreen called for {winner} 🎉🎉🎉");
            
            if (!ValidateComponents())
            {
                Debug.LogError("❌ VICTORY PANEL: Cannot show victory screen - components not valid!");
                return;
            }

            isGameOver = true;
            
            // Auto-close codex if it's open when victory screen appears
            CloseCodexIfOpen();
            
            Debug.Log($"✅ VICTORY PANEL: Components validated, showing panel for {winner}");

            // Show the victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                Debug.Log($"🔥 VICTORY PANEL: Panel activated! Panel active state: {victoryPanel.activeSelf}");
            }
            else
            {
                Debug.LogError("❌ VICTORY PANEL: victoryPanel is NULL!");
            }
            
            // Update victory text
            string winnerName = GetPlayerDisplayName(winner);
            Color winnerColor = GetPlayerColor(winner);
            
            string displayText;
            if (useRichText)
            {
                string colorHex = ColorUtility.ToHtmlStringRGBA(winnerColor);
                displayText = string.Format(victoryTextFormat, $"<color=#{colorHex}>{winnerName}</color>");
            }
            else
            {
                displayText = string.Format(victoryTextFormat, winnerName);
            }
            
            victoryText.text = displayText;
            
            if (!useRichText)
            {
                victoryText.color = winnerColor;
            }
            else
            {
                victoryText.color = Color.white; // Let rich text handle the coloring
            }
            
            // Add statistics if enabled
            if (showStatistics && combatManager != null)
            {
                displayText += "\n\n" + combatManager.GetCombatStatistics();
                victoryText.text = displayText;
            }
            
            Debug.Log($"🏆 Victory screen displayed for {winner}!");
            
            if (debugMode)
                Debug.Log($"VictoryPanelController: Panel '{victoryPanel.name}' is now active: {victoryPanel.activeInHierarchy}");
        }
        
        /// <summary>
        /// Automatically close the codex if it's currently open
        /// </summary>
        private void CloseCodexIfOpen()
        {
            try
            {
                // Find the codex panel manager
                var codexPanelManager = FindObjectOfType<CodexPanelManager>();
                if (codexPanelManager != null)
                {
                    // Check if the codex panel is currently visible
                    var codexPanel = codexPanelManager.GetComponent<RectTransform>();
                    if (codexPanel != null && codexPanel.gameObject.activeInHierarchy)
                    {
                        Debug.Log("🔄 VICTORY PANEL: Codex is open, auto-closing it for victory screen");
                        
                        // Force close the codex by calling TogglePanel (which will close it if open)
                        // We need to use reflection since TogglePanel might be private or we need to access the isPanelVisible field
                        var isPanelVisibleField = typeof(CodexPanelManager).GetField("isPanelVisible", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (isPanelVisibleField != null)
                        {
                            bool isPanelVisible = (bool)isPanelVisibleField.GetValue(codexPanelManager);
                            if (isPanelVisible)
                            {
                                // Call TogglePanel to close it
                                var togglePanelMethod = typeof(CodexPanelManager).GetMethod("TogglePanel");
                                if (togglePanelMethod != null)
                                {
                                    togglePanelMethod.Invoke(codexPanelManager, null);
                                    Debug.Log("✅ VICTORY PANEL: Successfully closed codex");
                                }
                                else
                                {
                                    Debug.LogWarning("⚠️ VICTORY PANEL: Could not find TogglePanel method");
                                }
                            }
                            else
                            {
                                Debug.Log("ℹ️ VICTORY PANEL: Codex was already closed");
                            }
                        }
                        else
                        {
                            // Fallback: try to disable the codex panel directly
                            codexPanel.gameObject.SetActive(false);
                            Debug.Log("✅ VICTORY PANEL: Codex panel disabled directly as fallback");
                        }
                    }
                    else
                    {
                        Debug.Log("ℹ️ VICTORY PANEL: Codex panel is not active, no need to close");
                    }
                }
                else
                {
                    Debug.Log("ℹ️ VICTORY PANEL: No CodexPanelManager found in scene");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ VICTORY PANEL: Error while trying to close codex: {ex.Message}");
            }
        }
        
        private void OnExitButtonClicked()
        {
            Debug.Log("VictoryPanelController: Exit button clicked - closing game");
            
            // Close the game
            #if UNITY_EDITOR
                // In editor, stop play mode
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                // In build, quit application
                Application.Quit();
            #endif
        }
        
        private void OnResetButtonClicked()
        {
            Debug.Log("VictoryPanelController: Reset button clicked - restarting game");
            
            // Hide victory panel first
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
            
            isGameOver = false;
            
            // Reset the game using GameManager
            if (gameManager != null)
            {
                gameManager.ResetGame();
            }
            else
            {
                Debug.LogWarning("VictoryPanelController: GameManager not found, cannot reset game");
            }
        }
        
        private Color GetPlayerColor(Unit.Player player)
        {
            return player == Unit.Player.Player1 ? player1VictoryColor : player2VictoryColor;
        }
        
        private string GetPlayerDisplayName(Unit.Player player)
        {
            return player == Unit.Player.Player1 ? "Player 1" : "Player 2";
        }
        
        /// <summary>
        /// Manual test method to verify the victory screen works
        /// </summary>
        [ContextMenu("🧪 Test Victory Screen Player 1 Wins")]
        public void TestVictoryScreenPlayer1()
        {
            Debug.Log("🧪 VictoryPanelController: Manual test - Player 1 wins");
            ShowVictoryScreen(Unit.Player.Player1);
        }
        
        /// <summary>
        /// Manual test method to verify the victory screen works
        /// </summary>
        [ContextMenu("🧪 Test Victory Screen Player 2 Wins")]
        public void TestVictoryScreenPlayer2()
        {
            Debug.Log("🧪 VictoryPanelController: Manual test - Player 2 wins");
            ShowVictoryScreen(Unit.Player.Player2);
        }
        
        /// <summary>
        /// Debug method to check current state
        /// </summary>
        [ContextMenu("🔍 Debug Current State")]
        public void DebugCurrentState()
        {
            Debug.Log("=== VictoryPanelController Debug State ===");
            Debug.Log($"Script GameObject: {gameObject.name}");
            Debug.Log($"Script Active: {gameObject.activeInHierarchy}");
            Debug.Log($"Is Initialized: {isInitialized}");
            Debug.Log($"Victory Panel: {(victoryPanel != null ? victoryPanel.name : "NULL")}");
            Debug.Log($"Victory Text: {(victoryText != null ? victoryText.name : "NULL")}");
            Debug.Log($"Exit Button: {(exitButton != null ? exitButton.name : "NULL")}");
            Debug.Log($"Reset Button: {(resetButton != null ? resetButton.name : "NULL")}");
            Debug.Log($"Combat Manager: {(combatManager != null ? "Connected" : "NULL")}");
            Debug.Log($"Game Manager: {(gameManager != null ? "Connected" : "NULL")}");
            Debug.Log($"Is Game Over: {isGameOver}");
            
            if (combatManager != null)
            {
                Debug.Log($"Combat Manager Instance: {combatManager.GetType().Name}");
                Debug.Log($"Player 1 Units: {combatManager.GetRemainingUnitsForPlayer(Unit.Player.Player1)}");
                Debug.Log($"Player 2 Units: {combatManager.GetRemainingUnitsForPlayer(Unit.Player.Player2)}");
            }
            Debug.Log("==========================================");
        }
        
        /// <summary>
        /// Debug method to manually test player elimination
        /// </summary>
        [ContextMenu("🧪 Simulate Player 1 Elimination")]
        public void SimulatePlayer1Elimination()
        {
            Debug.Log("🧪 VictoryPanelController: Simulating Player 1 elimination...");
            
            if (combatManager != null)
            {
                // Manually trigger the elimination event
                Debug.Log("🧪 Manually triggering OnPlayerEliminationCheck for Player1");
                OnPlayerEliminated(Unit.Player.Player1);
            }
            else
            {
                Debug.LogError("🧪 Cannot simulate - CombatManager not connected!");
            }
        }
        
        /// <summary>
        /// Debug method to manually test player elimination
        /// </summary>
        [ContextMenu("🧪 Simulate Player 2 Elimination")]
        public void SimulatePlayer2Elimination()
        {
            Debug.Log("🧪 VictoryPanelController: Simulating Player 2 elimination...");
            
            if (combatManager != null)
            {
                // Manually trigger the elimination event
                Debug.Log("🧪 Manually triggering OnPlayerEliminationCheck for Player2");
                OnPlayerEliminated(Unit.Player.Player2);
            }
            else
            {
                Debug.LogError("🧪 Cannot simulate - CombatManager not connected!");
            }
        }
        
        /// <summary>
        /// Debug method to check all units in the scene
        /// </summary>
        [ContextMenu("🔍 List All Units")]
        public void ListAllUnits()
        {
            Debug.Log("=== All Units in Scene ===");
            Unit[] allUnits = FindObjectsOfType<Unit>();
            Debug.Log($"Total units found: {allUnits.Length}");
            
            int player1Count = 0;
            int player2Count = 0;
            
            for (int i = 0; i < allUnits.Length; i++)
            {
                Unit unit = allUnits[i];
                Debug.Log($"Unit {i + 1}: {unit.UnitName} ({unit.player}) - Health: {unit.health}/{unit.maxHealth} - Alive: {unit.IsAlive()}");
                
                if (unit.player == Unit.Player.Player1)
                    player1Count++;
                else
                    player2Count++;
            }
            
            Debug.Log($"Player 1 units: {player1Count}");
            Debug.Log($"Player 2 units: {player2Count}");
            Debug.Log("=============================");
        }
        
        /// <summary>
        /// Hide the victory screen
        /// </summary>
        public void HideVictoryScreen()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
            isGameOver = false;
        }
        
        /// <summary>
        /// Set custom victory text format
        /// </summary>
        public void SetVictoryTextFormat(string format)
        {
            victoryTextFormat = format;
        }
        
        /// <summary>
        /// Set custom player colors
        /// </summary>
        public void SetPlayerColors(Color player1Color, Color player2Color)
        {
            player1VictoryColor = player1Color;
            player2VictoryColor = player2Color;
        }
        
        /// <summary>
        /// Check if game is currently over
        /// </summary>
        public bool IsGameOver => isGameOver;
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (combatManager != null)
            {
                combatManager.OnPlayerEliminationCheck -= OnPlayerEliminated;
            }
            
            // Clear button listeners
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
            }
            
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
            }
        }
    }
} 