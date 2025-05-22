using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float maxHeight = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    
    private Vector3 velocity = Vector3.zero;
    private bool isEnabled = true;
    private float currentHeight;
    private float isometricAngle = 45f; // The Y rotation of our isometric camera

    private void Start()
    {
        currentHeight = transform.position.y;
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
            Vector3 targetPosition = transform.position;
            
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
            currentHeight = Mathf.Clamp(currentHeight - scroll * zoomSpeed, minHeight, maxHeight);
            Vector3 targetPosition = transform.position;
            targetPosition.y = currentHeight;
            
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void ResetPosition()
    {
        Vector3 targetPosition = new Vector3(0, currentHeight, 0);
        transform.position = targetPosition;
    }
} 