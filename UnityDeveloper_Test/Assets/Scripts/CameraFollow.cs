using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float smoothSpeed = 10f;
    public float rotationSmoothTime = 0.1f;

    private Vector3 currentVelocity;
    
    // Mouse rotation
    private float yaw;
    private float pitch;
    public float mouseSensitivity = 2f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Mouse Input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -40, 85);

        // Calculate Target Rotation
        // The camera's "Up" must align with the Player's "Up"
        Quaternion targetUpRotation = Quaternion.LookRotation(target.forward, target.up);
        
        // Apply mouse offsets relative to the gravity alignment
        Quaternion rotation = targetUpRotation * Quaternion.Euler(pitch, yaw, 0);

        // Calculate Position
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance) + (target.up * height);
        
        // Smooth Movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Smooth LookAt
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smoothSpeed * Time.deltaTime);
    }
}