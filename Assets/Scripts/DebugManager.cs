using UnityEngine;
using System.Collections.Generic;

namespace TurnClash.Debug
{
    /// <summary>
    /// Debug class that provides the same interface as UnityEngine.Debug
    /// This allows files that import TurnClash.Debug to use Debug.Log() normally
    /// </summary>
    public static class Debug
    {
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }
        
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }
        
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
        
        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }
        
        public static void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
        
        public static void LogException(System.Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
    }

    /// <summary>
    /// Simple logging utility for the TurnClash.Debug namespace
    /// Provides compatibility for any code that might reference TurnClash.Debug.Log
    /// </summary>
    public static class Log
    {
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }

    /// <summary>
    /// Simple logging utility that provides the missing methods
    /// </summary>
    public static class LogWarning
    {
        public static void Invoke(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
    }

    /// <summary>
    /// Simple logging utility that provides the missing methods
    /// </summary>
    public static class LogError
    {
        public static void Invoke(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }

    /// <summary>
    /// Centralized debug management system that controls debug features using scripting define symbols
    /// Inspired by Unity Debug Inspector tools for easy debug configuration
    /// </summary>
    [CreateAssetMenu(fileName = "DebugSettings", menuName = "TurnClash/Debug Settings")]
    public class DebugManager : ScriptableObject
    {
        [Header("Debug Categories")]
        [SerializeField] private bool enableUIDebugging = false;
        [SerializeField] private bool enableTurnSystemDebugging = false;
        [SerializeField] private bool enableCombatDebugging = true;
        [SerializeField] private bool enableMovementDebugging = false;
        [SerializeField] private bool enableUnitDebugging = false;
        [SerializeField] private bool enableVisualEffectsDebugging = false;
        [SerializeField] private bool enableHoverTooltipDebugging = false;
        [SerializeField] private bool enableCodexDebugging = false;
        [SerializeField] private bool enablePerformanceDebugging = false;

        [Header("Detailed Combat Debugging")]
        [SerializeField] private bool enableDetailedCombatLogging = false;
        [SerializeField] private bool enableCombatStatistics = true;

        [Header("Victory Panel Debugging")]
        [SerializeField] private bool enableVictoryPanelDebugging = true;

        // Debug defines that will be set
        private readonly Dictionary<string, bool> debugDefines = new Dictionary<string, bool>();

        // Scripting define symbols used throughout the project
        public const string DEBUG_UI = "DEBUG_UI";
        public const string DEBUG_TURNS = "DEBUG_TURNS";
        public const string DEBUG_COMBAT = "DEBUG_COMBAT";
        public const string DEBUG_MOVEMENT = "DEBUG_MOVEMENT";
        public const string DEBUG_UNITS = "DEBUG_UNITS";
        public const string DEBUG_VISUAL_EFFECTS = "DEBUG_VISUAL_EFFECTS";
        public const string DEBUG_HOVER_TOOLTIP = "DEBUG_HOVER_TOOLTIP";
        public const string DEBUG_CODEX = "DEBUG_CODEX";
        public const string DEBUG_PERFORMANCE = "DEBUG_PERFORMANCE";
        public const string DEBUG_VICTORY_PANEL = "DEBUG_VICTORY_PANEL";
        public const string DEBUG_DETAILED_COMBAT = "DEBUG_DETAILED_COMBAT";
        public const string DEBUG_COMBAT_STATS = "DEBUG_COMBAT_STATS";

        // Singleton instance
        private static DebugManager instance;
        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<DebugManager>("DebugSettings");
                    if (instance == null)
                    {
                        // Create a default instance if none exists
                        instance = CreateInstance<DebugManager>();
                        Debug.LogWarning("DebugManager: No DebugSettings asset found in Resources folder. Using default settings.");
                    }
                }
                return instance;
            }
        }

        private void OnEnable()
        {
            UpdateDebugDefines();
        }

        /// <summary>
        /// Update the debug defines dictionary based on current settings
        /// </summary>
        private void UpdateDebugDefines()
        {
            debugDefines.Clear();
            debugDefines[DEBUG_UI] = enableUIDebugging;
            debugDefines[DEBUG_TURNS] = enableTurnSystemDebugging;
            debugDefines[DEBUG_COMBAT] = enableCombatDebugging;
            debugDefines[DEBUG_MOVEMENT] = enableMovementDebugging;
            debugDefines[DEBUG_UNITS] = enableUnitDebugging;
            debugDefines[DEBUG_VISUAL_EFFECTS] = enableVisualEffectsDebugging;
            debugDefines[DEBUG_HOVER_TOOLTIP] = enableHoverTooltipDebugging;
            debugDefines[DEBUG_CODEX] = enableCodexDebugging;
            debugDefines[DEBUG_PERFORMANCE] = enablePerformanceDebugging;
            debugDefines[DEBUG_VICTORY_PANEL] = enableVictoryPanelDebugging;
            debugDefines[DEBUG_DETAILED_COMBAT] = enableDetailedCombatLogging;
            debugDefines[DEBUG_COMBAT_STATS] = enableCombatStatistics;
        }

        /// <summary>
        /// Get whether a specific debug category is enabled
        /// </summary>
        public bool IsDebugEnabled(string debugDefine)
        {
            UpdateDebugDefines();
            return debugDefines.ContainsKey(debugDefine) && debugDefines[debugDefine];
        }

        /// <summary>
        /// Get current debug settings as a formatted string
        /// </summary>
        public string GetDebugStatus()
        {
            UpdateDebugDefines();
            var status = "=== DEBUG SETTINGS STATUS ===\n";
            foreach (var define in debugDefines)
            {
                status += $"{define.Key}: {(define.Value ? "ENABLED" : "DISABLED")}\n";
            }
            status += "=============================";
            return status;
        }

        // Properties for easy access from code
        public bool UIDebugging => enableUIDebugging;
        public bool TurnSystemDebugging => enableTurnSystemDebugging;
        public bool CombatDebugging => enableCombatDebugging;
        public bool MovementDebugging => enableMovementDebugging;
        public bool UnitDebugging => enableUnitDebugging;
        public bool VisualEffectsDebugging => enableVisualEffectsDebugging;
        public bool HoverTooltipDebugging => enableHoverTooltipDebugging;
        public bool CodexDebugging => enableCodexDebugging;
        public bool PerformanceDebugging => enablePerformanceDebugging;
        public bool VictoryPanelDebugging => enableVictoryPanelDebugging;
        public bool DetailedCombatLogging => enableDetailedCombatLogging;
        public bool CombatStatistics => enableCombatStatistics;

        /// <summary>
        /// Apply debug settings at runtime (for immediate feedback)
        /// </summary>
        public void ApplyRuntimeSettings()
        {
            UpdateDebugDefines();
            
            // Update CombatManager settings
            var combatManager = FindObjectOfType<CombatManager>();
            if (combatManager != null)
            {
                combatManager.SetCombatLogging(enableCombatDebugging);
                combatManager.SetDetailedLogging(enableDetailedCombatLogging);
            }

            Debug.Log($"DebugManager: Applied runtime settings\n{GetDebugStatus()}");
        }

        /// <summary>
        /// Reset all debug settings to default (disabled)
        /// </summary>
        [ContextMenu("Reset All Debug Settings")]
        public void ResetAllSettings()
        {
            enableUIDebugging = false;
            enableTurnSystemDebugging = false;
            enableCombatDebugging = false;
            enableMovementDebugging = false;
            enableUnitDebugging = false;
            enableVisualEffectsDebugging = false;
            enableHoverTooltipDebugging = false;
            enableCodexDebugging = false;
            enablePerformanceDebugging = false;
            enableVictoryPanelDebugging = false;
            enableDetailedCombatLogging = false;
            enableCombatStatistics = true; // Keep stats enabled by default
            
            UpdateDebugDefines();
            Debug.Log("DebugManager: All debug settings reset to default");
        }

        /// <summary>
        /// Enable all debug features (for comprehensive debugging)
        /// </summary>
        [ContextMenu("Enable All Debug Features")]
        public void EnableAllDebugging()
        {
            enableUIDebugging = true;
            enableTurnSystemDebugging = true;
            enableCombatDebugging = true;
            enableMovementDebugging = true;
            enableUnitDebugging = true;
            enableVisualEffectsDebugging = true;
            enableHoverTooltipDebugging = true;
            enableCodexDebugging = true;
            enablePerformanceDebugging = true;
            enableVictoryPanelDebugging = true;
            enableDetailedCombatLogging = true;
            enableCombatStatistics = true;
            
            UpdateDebugDefines();
            Debug.Log("DebugManager: All debug features enabled");
        }

        /// <summary>
        /// Enable only essential debugging (for normal development)
        /// </summary>
        [ContextMenu("Enable Essential Debug Features")]
        public void EnableEssentialDebugging()
        {
            enableUIDebugging = false;
            enableTurnSystemDebugging = false;
            enableCombatDebugging = true; // Keep combat debugging
            enableMovementDebugging = false;
            enableUnitDebugging = false;
            enableVisualEffectsDebugging = false;
            enableHoverTooltipDebugging = false;
            enableCodexDebugging = false;
            enablePerformanceDebugging = false;
            enableVictoryPanelDebugging = true; // Keep victory panel debugging
            enableDetailedCombatLogging = false;
            enableCombatStatistics = true;
            
            UpdateDebugDefines();
            Debug.Log("DebugManager: Essential debug features enabled");
        }
    }
} 