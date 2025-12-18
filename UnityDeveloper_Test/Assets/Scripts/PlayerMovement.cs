using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Rotation")]
    public float rotationSpeed = 180f; // degrees per second

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;

    private float horizontalInput;
    private float verticalInput;
    private bool rotatingBackward = false;
    private Quaternion backwardTargetRotation;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private GravityController gravityController;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    private float groundIgnoreTimer = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.freezeRotation = true; // we rotate manually
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (!gravityController)
        gravityController = GetComponent<GravityController>();

    }

    void Update()
    {
        ReadInput();
        CheckGrounded();
        Jump();
        Rotate();
        UpdateAnimator(); 
        Debug.Log($"Grounded: {isGrounded}");
        if (groundIgnoreTimer > 0f)
        groundIgnoreTimer -= Time.deltaTime;

    }

    void FixedUpdate()
    {
        Move();
    }

    // --------------------
    // INPUT
    // --------------------
    void ReadInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;
    }

    public void IgnoreGround(float duration)
    {
    groundIgnoreTimer = duration;
    }



    // --------------------
    // MOVEMENT (W / S)
    // --------------------
   void Move()
    {
    if (gravityController == null) return;

    Vector3 gravityDir = gravityController.CurrentGravityDirection.normalized;

    Vector3 currentVelocity = rb.velocity;

    // Separate gravity and planar velocity
    Vector3 gravityVelocity = Vector3.Project(currentVelocity, gravityDir);
    Vector3 planarVelocity  = currentVelocity - gravityVelocity;

    Vector3 desiredPlanar = Vector3.zero;

    // BACKWARD (S) → MOVE TOWARD CAMERA POSITION
    if (verticalInput < -0.1f && cameraTransform != null)
        {
        Vector3 dirToCamera = cameraTransform.position - transform.position;
        dirToCamera -= Vector3.Project(dirToCamera, gravityDir);
        dirToCamera.Normalize();

        desiredPlanar = dirToCamera * moveSpeed;
        }
    else
        {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, gravityDir).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(transform.right, gravityDir).normalized;

        Vector3 inputDir = forward * verticalInput + right * horizontalInput;

        if (inputDir.sqrMagnitude > 0.001f)
            desiredPlanar = inputDir.normalized * moveSpeed;
        }

    // Smoothly apply planar movement
    planarVelocity = Vector3.Lerp(planarVelocity, desiredPlanar, 12f * Time.fixedDeltaTime);

    rb.velocity = planarVelocity + gravityVelocity;
    }






    // --------------------
    // ROTATION (A / D)
    // --------------------
    void Rotate()
    {
    // A / D rotation (UNCHANGED)
    if (Mathf.Abs(horizontalInput) > 0.01f)
    {
        rotatingBackward = false;

        float rotationAmount = horizontalInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount);
        return;
    }

    // S → rotate backward (FIXED)
    if (verticalInput < -0.1f)
    {
        // Capture backward rotation ONCE
            if (!rotatingBackward)
            {
                backwardTargetRotation =
                Quaternion.LookRotation(-transform.forward, Vector3.up);

                rotatingBackward = true;
            }

            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            backwardTargetRotation,
            rotationSpeed * Time.deltaTime
            );
        }
    else
        {
        rotatingBackward = false;
        }
    }


    // --------------------
    // JUMP (SPACE)
    // --------------------
   void Jump()
    {
    if (!isGrounded) return;              // ❗ hard lock
    if (!Input.GetKeyDown(KeyCode.Space)) return;

    isGrounded = false;                   // lock jump immediately
    groundIgnoreTimer = 0.2f;             // ignore ground check briefly

    Vector3 jumpDir = Vector3.up;

    if (gravityController != null)
        jumpDir = -gravityController.CurrentGravityDirection.normalized;

    // Remove velocity in jump direction (prevents stacking)
    rb.velocity = Vector3.ProjectOnPlane(rb.velocity, jumpDir);

    rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);

    if (animator)
        {
        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");
        }
    }




    // --------------------
    // GROUND CHECK
    // --------------------
    void CheckGrounded()
    {
        if (groundIgnoreTimer > 0f)
        {
        isGrounded = false;
        return;
        }
        isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundCheckRadius,
        groundLayer,
        QueryTriggerInteraction.Ignore
        );

        Debug.DrawLine(
        groundCheck.position,
        groundCheck.position + Vector3.down * 0.05f,
        isGrounded ? Color.green : Color.red
        );
    }

   void UpdateAnimator()
    {
    if (!animator || gravityController == null) return;

    Vector3 gravityDir = gravityController.CurrentGravityDirection.normalized;

    // Velocity relative to gravity (movement speed)
    Vector3 planarVelocity =
        rb.velocity - Vector3.Project(rb.velocity, gravityDir);

    float speed = planarVelocity.magnitude;

    // Dead-zone to prevent flicker
    if (speed < 0.1f) speed = 0f;

    animator.SetFloat("Speed", speed);

    // IsGrounded is ONLY for jump transitions
    animator.SetBool("IsGrounded", isGrounded);
    }
}
