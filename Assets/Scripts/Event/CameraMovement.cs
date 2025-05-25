using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float speedMultiplier = 2f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomSmoothTime = 0.1f;
    
    [Header("Boundary Settings")]
    [SerializeField] private bool enableBoundaries = true;
    [SerializeField] private float boundaryPadding = 2f; // Extra space beyond grid edges
    [SerializeField] private bool autoFindGroundManager = true;
    [SerializeField] private bool debugBoundaries = false;
    [SerializeField] private bool showBoundaryGizmos = true; // Show boundaries in scene view
    
    private Vector3 velocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private bool isEnabled = true;
    private float currentZoom;
    private float isometricAngle = 45f; // The Y rotation of our isometric camera
    private Vector3 targetPosition;
    private float cameraAngle = 30f; // The X rotation for isometric view
    
    // Boundary management
    private IsometricGroundManager groundManager;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private bool boundariesInitialized = false;

    private void Start()
    {
        currentZoom = transform.position.y;
        targetPosition = transform.position;
        
        // Initialize camera boundaries
        if (enableBoundaries)
        {
            InitializeBoundaries();
            
            // Set camera to a better starting position for isometric grids
            if (boundariesInitialized && groundManager != null)
            {
                ResetPosition(); // This will now use the improved positioning logic
            }
        }
    }

    private void Update()
    {
        if (!isEnabled) return;
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 movement = Vector2.zero;
        
        // Get input
        if (Input.GetKey(KeyCode.A)) movement.y += 1;
        if (Input.GetKey(KeyCode.D)) movement.y -= 1;
        if (Input.GetKey(KeyCode.S)) movement.x -= 1;
        if (Input.GetKey(KeyCode.W)) movement.x += 1;

        // Normalize diagonal movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Apply movement to camera
        if (movement != Vector2.zero)
        {
            // Calculate current speed based on Alt key
            float currentSpeed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) 
                ? moveSpeed * speedMultiplier 
                : moveSpeed;

            // Calculate target position
            targetPosition = transform.position;
            
            // Convert the isometric angle to radians
            float angleRad = isometricAngle * Mathf.Deg2Rad;
            
            // Calculate the movement direction in isometric space
            // For isometric view, we need to rotate the movement by 45 degrees
            float rotatedX = movement.x * Mathf.Cos(angleRad) - movement.y * Mathf.Sin(angleRad);
            float rotatedZ = movement.x * Mathf.Sin(angleRad) + movement.y * Mathf.Cos(angleRad);
            
            // Apply the rotated movement
            targetPosition.x += rotatedX * currentSpeed * Time.deltaTime;
            targetPosition.z += rotatedZ * currentSpeed * Time.deltaTime;
            
            // Clamp to boundaries if enabled
            if (enableBoundaries && boundariesInitialized)
            {
                targetPosition = ClampToBounds(targetPosition);
            }
            
            // Smoothly move camera to target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Calculate new zoom level
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
            
            // Calculate new camera position based on zoom level
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float height = currentZoom;
            float distance = height / Mathf.Tan(angleRad);
            
            // Calculate the new position while maintaining the current X and Z
            Vector3 newPosition = new Vector3(
                transform.position.x,
                height,
                transform.position.z
            );
            
            // Update boundaries for new zoom level (since orthographic size affects camera view)
            UpdateBoundariesForZoom();
            
            // Clamp to boundaries if enabled
            if (enableBoundaries && boundariesInitialized)
            {
                newPosition = ClampToBounds(newPosition);
            }
            
            // Smoothly move to new position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                newPosition,
                ref velocity,
                zoomSmoothTime
            );
            
            // Update target position for movement
            targetPosition = transform.position;
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void ResetPosition()
    {
        // For isometric grids, position the camera better for viewing the whole grid
        if (groundManager != null && enableBoundaries && boundariesInitialized)
        {
            // Position camera toward the bottom-center of the diamond for better isometric viewing
            float gridCenterX = groundManager.GridOffset.x + (groundManager.GridWidth * groundManager.TileSize) / 2f;
            float gridCenterZ = groundManager.GridOffset.z + (groundManager.GridHeight * groundManager.TileSize) / 2f;
            
            // Offset toward the bottom of the isometric diamond for better view of the entire grid
            float offsetZ = -(groundManager.GridHeight * groundManager.TileSize) * 0.2f; // Move 20% toward bottom
            
            targetPosition = new Vector3(gridCenterX, currentZoom, gridCenterZ + offsetZ);
            targetPosition = ClampToBounds(targetPosition);
        }
        else
        {
            // Fallback to simple center position
            targetPosition = new Vector3(0, currentZoom, 0);
            
            // Clamp reset position to boundaries if enabled
            if (enableBoundaries && boundariesInitialized)
            {
                targetPosition = ClampToBounds(targetPosition);
            }
        }
        
        transform.position = targetPosition;
    }
    
    /// <summary>
    /// Initialize camera boundaries based on the ground grid
    /// </summary>
    private void InitializeBoundaries()
    {
        // Find the ground manager if not already found
        if (groundManager == null && autoFindGroundManager)
        {
            groundManager = FindObjectOfType<IsometricGroundManager>();
        }
        
        if (groundManager != null)
        {
            // Calculate boundaries based on grid size and camera orthographic size
            UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
            if (cam != null && cam.orthographic)
            {
                float gridWorldWidth = groundManager.GridWidth * groundManager.TileSize;
                float gridWorldHeight = groundManager.GridHeight * groundManager.TileSize;
                Vector3 gridOffset = groundManager.GridOffset;
                
                // Calculate the actual grid bounds in world space (rectangular)
                float gridMinX = gridOffset.x - boundaryPadding;
                float gridMaxX = gridOffset.x + gridWorldWidth + boundaryPadding;
                float gridMinZ = gridOffset.z - boundaryPadding;
                float gridMaxZ = gridOffset.z + gridWorldHeight + boundaryPadding;
                
                // Calculate camera viewing area - for orthographic cameras this is straightforward
                float cameraHalfWidth = cam.orthographicSize * cam.aspect;
                float cameraHalfHeight = cam.orthographicSize;
                
                // For isometric cameras, the simple approach is to use the larger of the two camera dimensions
                // to account for the rotation. This ensures we don't go beyond what's visible.
                float safeCameraExtent = Mathf.Max(cameraHalfWidth, cameraHalfHeight);
                
                // Set boundaries so camera center stays within these bounds
                // This ensures the camera view doesn't go too far off the grid
                minBounds = new Vector3(
                    gridMinX + safeCameraExtent,
                    transform.position.y, // Keep current Y (zoom level)
                    gridMinZ + safeCameraExtent
                );
                
                maxBounds = new Vector3(
                    gridMaxX - safeCameraExtent,
                    transform.position.y,
                    gridMaxZ - safeCameraExtent
                );
                
                // Ensure min bounds don't exceed max bounds (in case camera is too big for grid)
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
                
                boundariesInitialized = true;
                
                if (debugBoundaries)
                {
                    Debug.Log($"CameraMovement: Camera boundaries initialized");
                    Debug.Log($"Grid: {groundManager.GridWidth}x{groundManager.GridHeight}, Tile Size: {groundManager.TileSize}");
                    Debug.Log($"Grid Bounds: X[{gridMinX:F2} to {gridMaxX:F2}], Z[{gridMinZ:F2} to {gridMaxZ:F2}]");
                    Debug.Log($"Camera Orthographic Size: {cam.orthographicSize}, Aspect: {cam.aspect}");
                    Debug.Log($"Camera Half Size: {cameraHalfWidth:F2}x{cameraHalfHeight:F2}");
                    Debug.Log($"Safe Camera Extent: {safeCameraExtent:F2}");
                    Debug.Log($"Final Bounds: Min{minBounds}, Max{maxBounds}");
                }
            }
            else
            {
                Debug.LogWarning("CameraMovement: Camera is not orthographic or not found. Boundaries disabled.");
                enableBoundaries = false;
            }
        }
        else
        {
            Debug.LogWarning("CameraMovement: IsometricGroundManager not found. Boundaries disabled.");
            enableBoundaries = false;
        }
    }
    
    /// <summary>
    /// Clamp the camera position to stay within the defined boundaries
    /// </summary>
    private Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, minBounds.x, maxBounds.x),
            position.y, // Don't clamp Y (zoom level)
            Mathf.Clamp(position.z, minBounds.z, maxBounds.z)
        );
    }
    
    /// <summary>
    /// Update boundaries when zoom changes (since orthographic size affects camera view)
    /// </summary>
    private void UpdateBoundariesForZoom()
    {
        if (enableBoundaries && boundariesInitialized)
        {
            InitializeBoundaries(); // Recalculate boundaries with new zoom level
        }
    }
    
    /// <summary>
    /// Public method to enable/disable camera boundaries
    /// </summary>
    public void SetBoundariesEnabled(bool enabled)
    {
        enableBoundaries = enabled;
        if (enableBoundaries && !boundariesInitialized)
        {
            InitializeBoundaries();
        }
    }
    
    /// <summary>
    /// Public method to set boundary padding
    /// </summary>
    public void SetBoundaryPadding(float padding)
    {
        boundaryPadding = padding;
        if (enableBoundaries)
        {
            InitializeBoundaries(); // Recalculate with new padding
        }
    }
    
    /// <summary>
    /// Get current boundary information for debugging
    /// </summary>
    public string GetBoundaryInfo()
    {
        if (!enableBoundaries || !boundariesInitialized)
            return "Boundaries disabled or not initialized";
            
        return $"Boundaries: Min({minBounds.x:F2}, {minBounds.z:F2}) Max({maxBounds.x:F2}, {maxBounds.z:F2})";
    }
    
    /// <summary>
    /// Debug method to test boundary initialization
    /// </summary>
    [ContextMenu("Debug Boundaries")]
    public void DebugBoundaries()
    {
        Debug.Log($"=== Camera Movement Boundaries Debug ===");
        Debug.Log($"Boundaries Enabled: {enableBoundaries}");
        Debug.Log($"Boundaries Initialized: {boundariesInitialized}");
        Debug.Log($"Current Position: {transform.position}");
        Debug.Log($"Target Position: {targetPosition}");
        
        if (groundManager != null)
        {
            Debug.Log($"Ground Manager Found: {groundManager.name}");
            Debug.Log($"Grid Size: {groundManager.GridWidth}x{groundManager.GridHeight}");
            Debug.Log($"Tile Size: {groundManager.TileSize}");
            Debug.Log($"Grid Offset: {groundManager.GridOffset}");
        }
        else
        {
            Debug.Log("Ground Manager: Not Found");
        }
        
        if (boundariesInitialized)
        {
            Debug.Log($"Min Bounds: {minBounds}");
            Debug.Log($"Max Bounds: {maxBounds}");
            Debug.Log($"Boundary Padding: {boundaryPadding}");
            
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                Debug.Log($"Camera Orthographic Size: {cam.orthographicSize}");
                Debug.Log($"Camera Aspect Ratio: {cam.aspect}");
            }
        }
        
        Debug.Log($"========================================");
    }
    
    /// <summary>
    /// Force reinitialize boundaries (useful for runtime changes)
    /// </summary>
    [ContextMenu("Reinitialize Boundaries")]
    public void ReinitializeBoundaries()
    {
        boundariesInitialized = false;
        if (enableBoundaries)
        {
            InitializeBoundaries();
            Debug.Log("Camera boundaries reinitialized.");
        }
        else
        {
            Debug.Log("Boundaries are disabled.");
        }
    }
    
    /// <summary>
    /// Move camera to optimal position for viewing the entire isometric grid
    /// </summary>
    [ContextMenu("Move to Optimal Isometric View")]
    public void MoveToOptimalIsometricView()
    {
        if (groundManager != null)
        {
            // Position camera toward the bottom-center of the diamond for optimal isometric viewing
            float gridCenterX = groundManager.GridOffset.x + (groundManager.GridWidth * groundManager.TileSize) / 2f;
            float gridCenterZ = groundManager.GridOffset.z + (groundManager.GridHeight * groundManager.TileSize) / 2f;
            
            // Offset toward the bottom of the isometric diamond for better view of the entire grid
            float offsetZ = -(groundManager.GridHeight * groundManager.TileSize) * 0.25f; // Move 25% toward bottom
            
            targetPosition = new Vector3(gridCenterX, transform.position.y, gridCenterZ + offsetZ);
            
            // Clamp to boundaries if enabled
            if (enableBoundaries && boundariesInitialized)
            {
                targetPosition = ClampToBounds(targetPosition);
            }
            
            Debug.Log($"Camera moved to optimal isometric view: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("No IsometricGroundManager found for optimal positioning.");
        }
    }
    
    /// <summary>
    /// Draw boundary gizmos in scene view for debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showBoundaryGizmos || !enableBoundaries || !boundariesInitialized) return;
        
        // Draw boundary box
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) / 2f,
            transform.position.y,
            (minBounds.z + maxBounds.z) / 2f
        );
        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            0.1f,
            maxBounds.z - minBounds.z
        );
        Gizmos.DrawWireCube(center, size);
        
        // Draw grid bounds if ground manager exists
        if (groundManager != null)
        {
            Gizmos.color = Color.green;
            float gridWorldWidth = groundManager.GridWidth * groundManager.TileSize;
            float gridWorldHeight = groundManager.GridHeight * groundManager.TileSize;
            Vector3 gridOffset = groundManager.GridOffset;
            
            Vector3 gridCenter = new Vector3(
                gridOffset.x + gridWorldWidth / 2f,
                transform.position.y,
                gridOffset.z + gridWorldHeight / 2f
            );
            Vector3 gridSize = new Vector3(gridWorldWidth, 0.1f, gridWorldHeight);
            Gizmos.DrawWireCube(gridCenter, gridSize);
        }
        
        // Draw current camera position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
} 