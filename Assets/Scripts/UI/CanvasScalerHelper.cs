using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TurnClash.UI
{
    /// <summary>
    /// Helper script to ensure proper Canvas Scaler configuration for responsive UI
    /// Automatically configures Canvas Scaler for optimal scaling across different aspect ratios
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerHelper : MonoBehaviour
    {
        [Header("Canvas Scaler Settings")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] [Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool logAspectRatioChanges = false;
        
        private CanvasScaler canvasScaler;
        private float lastAspectRatio;
        
        private void Awake()
        {
            SetupCanvasScaler();
        }
        
        private void Start()
        {
            lastAspectRatio = GetCurrentAspectRatio();
            if (logAspectRatioChanges)
            {
                Debug.Log($"CanvasScalerHelper: Initial aspect ratio: {lastAspectRatio:F2}");
            }
        }
        
        private void Update()
        {
            // Check for aspect ratio changes (useful for testing different resolutions)
            float currentAspectRatio = GetCurrentAspectRatio();
            if (Mathf.Abs(currentAspectRatio - lastAspectRatio) > 0.01f)
            {
                lastAspectRatio = currentAspectRatio;
                if (logAspectRatioChanges)
                {
                    Debug.Log($"CanvasScalerHelper: Aspect ratio changed to: {currentAspectRatio:F2}");
                }
                
                // Optionally refresh UI layouts when aspect ratio changes
                RefreshUILayouts();
            }
        }
        
        private void SetupCanvasScaler()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogError("CanvasScalerHelper: CanvasScaler component not found!");
                return;
            }
            
            // Configure Canvas Scaler for responsive scaling
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.screenMatchMode = screenMatchMode;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            
            // Additional settings for better scaling
            canvasScaler.referencePixelsPerUnit = 100f;
            
            if (logAspectRatioChanges)
            {
                Debug.Log($"CanvasScalerHelper: Canvas Scaler configured with reference resolution {referenceResolution}, match: {matchWidthOrHeight}");
            }
        }
        
        private float GetCurrentAspectRatio()
        {
            return (float)Screen.width / Screen.height;
        }
        
        private void RefreshUILayouts()
        {
            // Find and refresh any CodexPanelManager instances
            CodexPanelManager[] codexManagers = FindObjectsOfType<CodexPanelManager>();
            foreach (var manager in codexManagers)
            {
                // Only refresh if the panel is visible and not animating
                if (manager != null && manager.gameObject.activeInHierarchy)
                {
                    // Add a small delay to avoid interfering with any ongoing animations
                    StartCoroutine(DelayedRefresh(manager));
                    if (logAspectRatioChanges)
                    {
                        Debug.Log("CanvasScalerHelper: Scheduled delayed refresh for CodexPanelManager UI layout");
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedRefresh(CodexPanelManager manager)
        {
            // Wait a short time to avoid interfering with animations
            yield return new WaitForSeconds(0.1f);
            
            if (manager != null)
            {
                manager.RefreshUILayout();
            }
        }
        
        /// <summary>
        /// Get recommended match value based on current aspect ratio
        /// </summary>
        public float GetRecommendedMatch()
        {
            float aspectRatio = GetCurrentAspectRatio();
            float referenceAspectRatio = referenceResolution.x / referenceResolution.y;
            
            // For wider screens (like 21:9), favor width matching
            // For taller screens (like 4:3), favor height matching
            if (aspectRatio > referenceAspectRatio)
            {
                return 0.0f; // Match width for wider screens
            }
            else if (aspectRatio < referenceAspectRatio)
            {
                return 1.0f; // Match height for taller screens
            }
            else
            {
                return 0.5f; // Balanced for reference aspect ratio
            }
        }
        
        /// <summary>
        /// Apply recommended match value based on current aspect ratio
        /// </summary>
        [ContextMenu("Apply Recommended Match")]
        public void ApplyRecommendedMatch()
        {
            if (canvasScaler != null)
            {
                float recommendedMatch = GetRecommendedMatch();
                canvasScaler.matchWidthOrHeight = recommendedMatch;
                matchWidthOrHeight = recommendedMatch;
                
                if (logAspectRatioChanges)
                {
                    Debug.Log($"CanvasScalerHelper: Applied recommended match value: {recommendedMatch:F2}");
                }
            }
        }
        
        private void OnValidate()
        {
            // Update Canvas Scaler when values change in inspector
            if (canvasScaler != null)
            {
                SetupCanvasScaler();
            }
        }
    }
} 