using UnityEngine;

namespace TurnClash.CameraSystem
{
    /// <summary>
    /// Visualizes camera boundaries in the scene view for debugging purposes
    /// </summary>
    public class CameraBoundaryVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showBoundaries = true;
        [SerializeField] private Color boundaryColor = Color.red;
        [SerializeField] private bool showGridOutline = true;
        [SerializeField] private Color gridOutlineColor = Color.green;
        [SerializeField] private bool showCameraViewport = true;
        [SerializeField] private Color viewportColor = Color.blue;
        
        private CameraMovement cameraMovement;
        private IsometricGroundManager groundManager;
        
        private void Start()
        {
            cameraMovement = GetComponent<CameraMovement>();
            groundManager = FindObjectOfType<IsometricGroundManager>();
        }
        
        private void OnDrawGizmos()
        {
            if (!showBoundaries) return;
            
            if (cameraMovement != null && groundManager != null)
            {
                DrawBoundaryVisualization();
            }
        }
        
        private void DrawBoundaryVisualization()
        {
            // Get camera component
            UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
            if (cam == null || !cam.orthographic) return;
            
            // Calculate grid bounds
            float gridWorldWidth = groundManager.GridWidth * groundManager.TileSize;
            float gridWorldHeight = groundManager.GridHeight * groundManager.TileSize;
            Vector3 gridOffset = groundManager.GridOffset;
            
            // Show grid outline
            if (showGridOutline)
            {
                Gizmos.color = gridOutlineColor;
                Vector3 gridCenter = new Vector3(
                    gridOffset.x + gridWorldWidth / 2f,
                    0f,
                    gridOffset.z + gridWorldHeight / 2f
                );
                Vector3 gridSize = new Vector3(gridWorldWidth, 0.1f, gridWorldHeight);
                Gizmos.DrawWireCube(gridCenter, gridSize);
            }
            
            // Show camera viewport
            if (showCameraViewport)
            {
                Gizmos.color = viewportColor;
                float cameraHalfWidth = cam.orthographicSize * cam.aspect;
                float cameraHalfHeight = cam.orthographicSize;
                
                Vector3 viewportCenter = new Vector3(
                    transform.position.x,
                    0f,
                    transform.position.z
                );
                Vector3 viewportSize = new Vector3(cameraHalfWidth * 2f, 0.1f, cameraHalfHeight * 2f);
                Gizmos.DrawWireCube(viewportCenter, viewportSize);
            }
            
            // Show camera movement boundaries
            if (showBoundaries)
            {
                Gizmos.color = boundaryColor;
                
                // Calculate boundaries (similar to CameraMovement logic)
                float cameraHalfWidth = cam.orthographicSize * cam.aspect;
                float cameraHalfHeight = cam.orthographicSize;
                
                float boundaryPadding = 2f; // Should match CameraMovement.boundaryPadding
                float gridMinX = gridOffset.x - boundaryPadding;
                float gridMaxX = gridOffset.x + gridWorldWidth + boundaryPadding;
                float gridMinZ = gridOffset.z - boundaryPadding;
                float gridMaxZ = gridOffset.z + gridWorldHeight + boundaryPadding;
                
                Vector3 minBounds = new Vector3(
                    gridMinX + cameraHalfWidth,
                    0f,
                    gridMinZ + cameraHalfHeight
                );
                
                Vector3 maxBounds = new Vector3(
                    gridMaxX - cameraHalfWidth,
                    0f,
                    gridMaxZ - cameraHalfHeight
                );
                
                // Ensure min bounds don't exceed max bounds
                if (minBounds.x > maxBounds.x)
                {
                    float centerX = (gridMinX + gridMaxX) / 2f;
                    minBounds.x = maxBounds.x = centerX;
                }
                
                if (minBounds.z > maxBounds.z)
                {
                    float centerZ = (gridMinZ + gridMaxZ) / 2f;
                    minBounds.z = maxBounds.z = centerZ;
                }
                
                // Draw boundary rectangle
                Vector3 boundaryCenter = new Vector3(
                    (minBounds.x + maxBounds.x) / 2f,
                    0f,
                    (minBounds.z + maxBounds.z) / 2f
                );
                Vector3 boundarySize = new Vector3(
                    maxBounds.x - minBounds.x,
                    0.1f,
                    maxBounds.z - minBounds.z
                );
                
                Gizmos.DrawWireCube(boundaryCenter, boundarySize);
                
                // Draw corner markers
                float markerSize = 0.5f;
                Gizmos.DrawWireCube(minBounds, Vector3.one * markerSize);
                Gizmos.DrawWireCube(maxBounds, Vector3.one * markerSize);
                Gizmos.DrawWireCube(new Vector3(minBounds.x, 0f, maxBounds.z), Vector3.one * markerSize);
                Gizmos.DrawWireCube(new Vector3(maxBounds.x, 0f, minBounds.z), Vector3.one * markerSize);
            }
        }
        
        /// <summary>
        /// Toggle boundary visualization
        /// </summary>
        [ContextMenu("Toggle Boundary Visualization")]
        public void ToggleBoundaryVisualization()
        {
            showBoundaries = !showBoundaries;
            Debug.Log($"Camera boundary visualization: {(showBoundaries ? "ON" : "OFF")}");
        }
    }
} 