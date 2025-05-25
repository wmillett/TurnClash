using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TurnClash.Debug;

namespace TurnClash.Editor
{
    /// <summary>
    /// Custom editor window for managing debug settings and scripting define symbols
    /// Inspired by Unity Define Inspector tools from the web search results
    /// </summary>
    public class DebugManagerEditor : EditorWindow
    {
        private DebugManager debugSettings;
        private Vector2 scrollPosition;
        private bool showDefineSymbols = false;
        private static readonly Color enabledColor = new Color(0.3f, 0.8f, 0.3f);
        private static readonly Color disabledColor = new Color(0.8f, 0.3f, 0.3f);

        [MenuItem("Window/TurnClash/Debug Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<DebugManagerEditor>("Debug Manager");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadDebugSettings();
        }

        private void LoadDebugSettings()
        {
            // Try to find existing debug settings
            string[] guids = AssetDatabase.FindAssets("t:DebugManager");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                debugSettings = AssetDatabase.LoadAssetAtPath<DebugManager>(path);
            }

            if (debugSettings == null)
            {
                // Create a new one if none exists
                CreateDebugSettings();
            }
        }

        private void CreateDebugSettings()
        {
            debugSettings = ScriptableObject.CreateInstance<DebugManager>();
            
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string assetPath = "Assets/Resources/DebugSettings.asset";
            AssetDatabase.CreateAsset(debugSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"DebugManager: Created new DebugSettings asset at {assetPath}");
        }

        private void OnGUI()
        {
            if (debugSettings == null)
            {
                LoadDebugSettings();
                if (debugSettings == null)
                {
                    EditorGUILayout.HelpBox("Debug Settings asset not found. Creating a new one...", MessageType.Warning);
                    if (GUILayout.Button("Create Debug Settings"))
                    {
                        CreateDebugSettings();
                    }
                    return;
                }
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawDebugCategories();
            DrawActions();
            DrawDefineSymbolsSection();
            DrawCurrentStatus();

            EditorGUILayout.EndScrollView();

            // Save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(debugSettings);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            // Title
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("TurnClash Debug Manager", titleStyle);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Centralized debug control for the entire project", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(10);

            // Info box
            EditorGUILayout.HelpBox(
                "This tool manages debug features across all scripts in the project. " +
                "Changes will be applied when you click 'Apply Settings' and may require recompilation.",
                MessageType.Info
            );
        }

        private void DrawDebugCategories()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug Categories", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            SerializedObject serializedObject = new SerializedObject(debugSettings);
            
            // Main debug categories
            DrawDebugToggle(serializedObject, "enableUIDebugging", "UI Debugging", 
                "Enables debug logging for selection UI, tooltips, and other UI systems");
            
            DrawDebugToggle(serializedObject, "enableTurnSystemDebugging", "Turn System Debugging", 
                "Enables debug logging for turn management and player actions");
            
            DrawDebugToggle(serializedObject, "enableCombatDebugging", "Combat Debugging", 
                "Enables basic combat logging and events");
            
            DrawDebugToggle(serializedObject, "enableMovementDebugging", "Movement Debugging", 
                "Enables debug logging for unit movement and movement preview");
            
            DrawDebugToggle(serializedObject, "enableUnitDebugging", "Unit Debugging", 
                "Enables debug logging for unit actions, defending, and stat changes");
            
            DrawDebugToggle(serializedObject, "enableVisualEffectsDebugging", "Visual Effects Debugging", 
                "Enables debug logging for visual effects and highlighting");
            
            DrawDebugToggle(serializedObject, "enableHoverTooltipDebugging", "Hover Tooltip Debugging", 
                "Enables debug logging for hover tooltip system");
            
            DrawDebugToggle(serializedObject, "enableCodexDebugging", "Codex Debugging", 
                "Enables debug logging for codex system");
            
            DrawDebugToggle(serializedObject, "enablePerformanceDebugging", "Performance Debugging", 
                "Enables performance monitoring and optimization logging");

            EditorGUILayout.Space(10);
            
            // Detailed categories
            EditorGUILayout.LabelField("Detailed Debug Options", EditorStyles.boldLabel);
            
            DrawDebugToggle(serializedObject, "enableDetailedCombatLogging", "Detailed Combat Logging", 
                "Enables comprehensive combat logging with damage calculations");
            
            DrawDebugToggle(serializedObject, "enableCombatStatistics", "Combat Statistics", 
                "Enables combat statistics tracking and display");
            
            DrawDebugToggle(serializedObject, "enableVictoryPanelDebugging", "Victory Panel Debugging", 
                "Enables debug logging for victory panel and game over states");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDebugToggle(SerializedObject serializedObject, string propertyName, string label, string tooltip)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Color indicator
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = property.boolValue ? enabledColor : disabledColor;
                GUILayout.Button("", GUILayout.Width(10), GUILayout.Height(18));
                GUI.backgroundColor = originalColor;
                
                // Toggle
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawActions()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            
            // Apply Settings button
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Apply Settings", GUILayout.Height(30)))
            {
                ApplyDebugSettings();
            }
            
            // Reset button
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.3f);
            if (GUILayout.Button("Reset All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Debug Settings", 
                    "Are you sure you want to reset all debug settings to default?", 
                    "Yes", "No"))
                {
                    debugSettings.ResetAllSettings();
                    EditorUtility.SetDirty(debugSettings);
                }
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            // Enable All button
            GUI.backgroundColor = new Color(0.3f, 0.6f, 0.8f);
            if (GUILayout.Button("Enable All Debug", GUILayout.Height(25)))
            {
                debugSettings.EnableAllDebugging();
                EditorUtility.SetDirty(debugSettings);
            }
            
            // Essential Debug button
            GUI.backgroundColor = new Color(0.6f, 0.3f, 0.8f);
            if (GUILayout.Button("Essential Debug Only", GUILayout.Height(25)))
            {
                debugSettings.EnableEssentialDebugging();
                EditorUtility.SetDirty(debugSettings);
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefineSymbolsSection()
        {
            EditorGUILayout.Space(15);
            showDefineSymbols = EditorGUILayout.Foldout(showDefineSymbols, "Current Scripting Define Symbols", true);
            
            if (showDefineSymbols)
            {
                EditorGUILayout.Space(5);
                var currentDefines = GetCurrentDefines();
                
                if (currentDefines.Count == 0)
                {
                    EditorGUILayout.HelpBox("No debug defines currently set", MessageType.Info);
                }
                else
                {
                    foreach (string define in currentDefines)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        EditorGUILayout.LabelField($"• {define}", EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Clear All Define Symbols"))
                {
                    if (EditorUtility.DisplayDialog("Clear Define Symbols", 
                        "This will remove all TurnClash debug define symbols. Continue?", 
                        "Yes", "No"))
                    {
                        ClearAllDefines();
                    }
                }
            }
        }

        private void DrawCurrentStatus()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Current Status", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            string status = debugSettings.GetDebugStatus();
            GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField(status, statusStyle);
            
            EditorGUILayout.EndVertical();
        }

        private void ApplyDebugSettings()
        {
            List<string> definesToAdd = new List<string>();
            
            // Add defines based on current settings
            if (debugSettings.UIDebugging) definesToAdd.Add(DebugManager.DEBUG_UI);
            if (debugSettings.TurnSystemDebugging) definesToAdd.Add(DebugManager.DEBUG_TURNS);
            if (debugSettings.CombatDebugging) definesToAdd.Add(DebugManager.DEBUG_COMBAT);
            if (debugSettings.MovementDebugging) definesToAdd.Add(DebugManager.DEBUG_MOVEMENT);
            if (debugSettings.UnitDebugging) definesToAdd.Add(DebugManager.DEBUG_UNITS);
            if (debugSettings.VisualEffectsDebugging) definesToAdd.Add(DebugManager.DEBUG_VISUAL_EFFECTS);
            if (debugSettings.HoverTooltipDebugging) definesToAdd.Add(DebugManager.DEBUG_HOVER_TOOLTIP);
            if (debugSettings.CodexDebugging) definesToAdd.Add(DebugManager.DEBUG_CODEX);
            if (debugSettings.PerformanceDebugging) definesToAdd.Add(DebugManager.DEBUG_PERFORMANCE);
            if (debugSettings.VictoryPanelDebugging) definesToAdd.Add(DebugManager.DEBUG_VICTORY_PANEL);
            if (debugSettings.DetailedCombatLogging) definesToAdd.Add(DebugManager.DEBUG_DETAILED_COMBAT);
            if (debugSettings.CombatStatistics) definesToAdd.Add(DebugManager.DEBUG_COMBAT_STATS);

            // Apply the defines
            SetDefineSymbols(definesToAdd);
            
            // Also apply runtime settings
            debugSettings.ApplyRuntimeSettings();
            
            Debug.Log($"DebugManager: Applied {definesToAdd.Count} debug define symbols");
        }

        private List<string> GetCurrentDefines()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            
            if (string.IsNullOrEmpty(definesString))
                return new List<string>();
                
            return definesString.Split(';')
                .Where(d => d.StartsWith("DEBUG_"))
                .ToList();
        }

        private void SetDefineSymbols(List<string> newDefines)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            
            // Remove all existing debug defines
            var existingDefines = currentDefines.Split(';')
                .Where(d => !string.IsNullOrEmpty(d) && !d.StartsWith("DEBUG_"))
                .ToList();
            
            // Add new debug defines
            existingDefines.AddRange(newDefines);
            
            // Set the new defines
            string newDefinesString = string.Join(";", existingDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, newDefinesString);
            
            AssetDatabase.Refresh();
        }

        private void ClearAllDefines()
        {
            SetDefineSymbols(new List<string>());
            Debug.Log("DebugManager: Cleared all debug define symbols");
        }
    }

    /// <summary>
    /// Custom property drawer for the DebugManager ScriptableObject
    /// </summary>
    [CustomEditor(typeof(DebugManager))]
    public class DebugManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DebugManager debugManager = (DebugManager)target;
            
            EditorGUILayout.Space(10);
            
            // Header
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("TurnClash Debug Settings", headerStyle);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Use the Debug Manager window (Window → TurnClash → Debug Manager) for better control and define symbol management.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            // Quick actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Debug Manager Window"))
            {
                DebugManagerEditor.ShowWindow();
            }
            
            if (GUILayout.Button("Apply Runtime Settings"))
            {
                debugManager.ApplyRuntimeSettings();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Status display
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Status:", EditorStyles.miniLabel);
            string status = debugManager.GetDebugStatus();
            EditorGUILayout.LabelField(status, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
    }
} 