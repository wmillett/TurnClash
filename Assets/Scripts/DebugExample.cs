using UnityEngine;

namespace TurnClash.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the centralized debug system
    /// This shows best practices for implementing debug features in your scripts
    /// </summary>
    public class DebugExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private float exampleValue = 1.0f;
        
        private void Start()
        {
            // Method 1: Using conditional compilation with defines (RECOMMENDED)
            // This code will only be compiled when DEBUG_UI is defined
#if DEBUG_UI
            Debug.Log("DebugExample: UI debugging is enabled via define symbols");
#endif

#if DEBUG_PERFORMANCE
            Debug.Log("DebugExample: Performance debugging is enabled");
#endif

            // Method 2: Using DebugManager runtime checks (for dynamic control)
            // This allows toggling without recompilation but has runtime overhead
            var debugManager = DebugManager.Instance;
            if (debugManager != null)
            {
                if (debugManager.UIDebugging)
                {
                    Debug.Log("DebugExample: UI debugging enabled via runtime check");
                }
                
                if (debugManager.PerformanceDebugging)
                {
                    Debug.Log("DebugExample: Performance debugging enabled via runtime check");
                }
            }
            
            // Example of detailed logging
            LogExampleOperations();
        }
        
        private void Update()
        {
            // Example: Performance debugging that only runs when enabled
#if DEBUG_PERFORMANCE
            if (Time.frameCount % 60 == 0) // Log every 60 frames
            {
                Debug.Log($"DebugExample: FPS: {1.0f / Time.deltaTime:F1}, Frame: {Time.frameCount}");
            }
#endif
        }
        
        private void LogExampleOperations()
        {
            // Example of using different debug levels
#if DEBUG_UI
            Debug.Log("DebugExample: Basic UI operation completed");
#endif

#if DEBUG_DETAILED_COMBAT
            Debug.Log("DebugExample: This would show detailed combat information");
#endif

            // Runtime debug example with null checking
            var debugManager = DebugManager.Instance;
            if (debugManager?.CombatDebugging == true)
            {
                Debug.Log("DebugExample: Combat debugging is active");
            }
        }
        
        /// <summary>
        /// Example method showing how to implement toggleable debug features
        /// </summary>
        public void DoComplexOperation()
        {
#if DEBUG_PERFORMANCE
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            
            // Your actual operation here
            ComplexCalculation();
            
#if DEBUG_PERFORMANCE
            stopwatch.Stop();
            Debug.Log($"DebugExample: Complex operation took {stopwatch.ElapsedMilliseconds}ms");
#endif
        }
        
        private void ComplexCalculation()
        {
            // Simulate some work
            float result = 0;
            for (int i = 0; i < 1000; i++)
            {
                result += Mathf.Sin(i * exampleValue);
            }
            
#if DEBUG_PERFORMANCE
            Debug.Log($"DebugExample: Calculation result: {result}");
#endif
        }
        
        /// <summary>
        /// Example of category-specific debug logging
        /// </summary>
        public void LogCategorySpecificMessage(string category, string message)
        {
            switch (category.ToUpper())
            {
                case "UI":
#if DEBUG_UI
                    Debug.Log($"[UI] {message}");
#endif
                    break;
                    
                case "COMBAT":
#if DEBUG_COMBAT
                    Debug.Log($"[COMBAT] {message}");
#endif
                    break;
                    
                case "MOVEMENT":
#if DEBUG_MOVEMENT
                    Debug.Log($"[MOVEMENT] {message}");
#endif
                    break;
                    
                case "UNITS":
#if DEBUG_UNITS
                    Debug.Log($"[UNITS] {message}");
#endif
                    break;
                    
                default:
                    Debug.Log($"[GENERAL] {message}");
                    break;
            }
        }
        
        /// <summary>
        /// Test all debug categories
        /// </summary>
        [ContextMenu("Test All Debug Categories")]
        public void TestAllDebugCategories()
        {
            LogCategorySpecificMessage("UI", "Testing UI debug logging");
            LogCategorySpecificMessage("COMBAT", "Testing combat debug logging");
            LogCategorySpecificMessage("MOVEMENT", "Testing movement debug logging");
            LogCategorySpecificMessage("UNITS", "Testing units debug logging");
            
#if DEBUG_HOVER_TOOLTIP
            Debug.Log("[HOVER] Hover tooltip debugging is active");
#endif

#if DEBUG_CODEX
            Debug.Log("[CODEX] Codex debugging is active");
#endif

#if DEBUG_VICTORY_PANEL
            Debug.Log("[VICTORY] Victory panel debugging is active");
#endif
        }
        
        /// <summary>
        /// Example of runtime debug status checking
        /// </summary>
        [ContextMenu("Show Debug Status")]
        public void ShowDebugStatus()
        {
            var debugManager = DebugManager.Instance;
            if (debugManager != null)
            {
                Debug.Log(debugManager.GetDebugStatus());
            }
            else
            {
                Debug.LogWarning("DebugManager instance not found!");
            }
        }
    }
} 