using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomSmoothTime = 0.1f;
    
    private Vector3 velocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private bool isEnabled = true;
    private float currentZoom;
    private float isometricAngle = 45f; // The Y rotation of our isometric camera
    private Vector3 targetPosition;
    private float cameraAngle = 30f; // The X rotation for isometric view

    private void Start()
    {
        currentZoom = transform.position.y;
        targetPosition = transform.position;
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
        targetPosition = new Vector3(0, currentZoom, 0);
        transform.position = targetPosition;
    }
} 