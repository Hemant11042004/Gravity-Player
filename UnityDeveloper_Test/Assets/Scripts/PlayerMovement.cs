using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public LayerMask groundLayer;
    
    private Rigidbody rb;
    private bool isGrounded;
    private float groundCheckDistance = 1.1f; // Slightly more than half player height
    private Transform cameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // We will apply custom gravity in GravityController
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Apply jump force in the local Up direction
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to camera and current player orientation
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Project camera vectors onto the plane defined by the player's up vector
        camForward = Vector3.ProjectOnPlane(camForward, transform.up).normalized;
        camRight = Vector3.ProjectOnPlane(camRight, transform.up).normalized;

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            // Move the player
            Vector3 targetVelocity = moveDir * moveSpeed;
            
            // Preserve vertical velocity (falling/jumping)
            float verticalVelocity = Vector3.Dot(rb.velocity, transform.up);
            Vector3 verticalComponent = transform.up * verticalVelocity;
            rb.velocity = targetVelocity + verticalComponent;

            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }
}