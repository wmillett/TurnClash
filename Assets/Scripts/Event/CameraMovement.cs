using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 15f; // Increased from 5f
    [SerializeField] private float smoothTime = 0.1f; // Decreased from 0.3f for faster response
    [SerializeField] private float speedMultiplier = 10f; // Speed multiplier when holding Alt
    private Vector3 velocity = Vector3.zero;
    private bool isEnabled = true;

    private void Update()
    {
        if (!isEnabled) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 movement = Vector2.zero;
        
        // Get input
        if (Input.GetKey(KeyCode.W)) movement.y += 1;
        if (Input.GetKey(KeyCode.S)) movement.y -= 1;
        if (Input.GetKey(KeyCode.A)) movement.x -= 1;
        if (Input.GetKey(KeyCode.D)) movement.x += 1;

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
            targetPosition.x += movement.x * currentSpeed * Time.deltaTime;
            targetPosition.y += movement.y * currentSpeed * Time.deltaTime;
            
            // Smoothly move camera to target position
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

    // Call this when you want to reset the camera position
    public void ResetPosition()
    {
        transform.position = new Vector3(0, 0, transform.position.z);
    }
} 