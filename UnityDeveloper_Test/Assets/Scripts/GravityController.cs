using UnityEngine;
using System.Collections;

public class GravityController : MonoBehaviour
{
    [Header("Settings")]
    public float gravityMagnitude = 9.81f;
    public float rotationSpeed = 5f;
    public GameObject hologramPrefab;
    
    private Rigidbody rb;
    private Vector3 currentGravityDir = Vector3.down;
    private Vector3 targetGravityDir = Vector3.down;
    private bool isSwitchingGravity = false;
    
    private GameObject hologramInstance;
    private bool showHologram = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Setup Hologram
        if (hologramPrefab != null)
        {
            hologramInstance = Instantiate(hologramPrefab, transform.position, transform.rotation);
            hologramInstance.SetActive(false);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateHologram();
    }

    void FixedUpdate()
    {
        // Apply custom gravity
        rb.AddForce(currentGravityDir * gravityMagnitude, ForceMode.Acceleration);

        // Smoothly rotate player to align with new Up direction
        Vector3 targetUp = -currentGravityDir;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    void HandleInput()
    {
        if (isSwitchingGravity) return;

        Vector3 newDir = Vector3.zero;
        Transform cam = Camera.main.transform;

        // Determine direction based on Camera perspective
        if (Input.GetKey(KeyCode.UpArrow)) newDir = cam.forward;
        else if (Input.GetKey(KeyCode.DownArrow)) newDir = -cam.forward;
        else if (Input.GetKey(KeyCode.LeftArrow)) newDir = -cam.right;
        else if (Input.GetKey(KeyCode.RightArrow)) newDir = cam.right;

        if (newDir != Vector3.zero)
        {
            // Snap to nearest cardinal axis relative to world or current orientation
            targetGravityDir = newDir.normalized;
            showHologram = true;
        }
        else
        {
            showHologram = false;
        }

        if (showHologram && Input.GetKeyDown(KeyCode.Return))
        {
            ChangeGravity(targetGravityDir);
        }
    }

    void UpdateHologram()
    {
        if (hologramInstance == null) return;

        if (showHologram)
        {
            hologramInstance.SetActive(true);
            hologramInstance.transform.position = transform.position;
            
            // Align hologram feet to the NEW wall (New Gravity Down)
            Vector3 holoUp = targetGravityDir;
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, holoUp) * transform.rotation;
            hologramInstance.transform.rotation = targetRot;
        }
        else
        {
            hologramInstance.SetActive(false);
        }
    }

    void ChangeGravity(Vector3 newDir)
    {
        currentGravityDir = newDir;
        showHologram = false;
    }
    
    public Vector3 GetGravityDirection()
    {
        return currentGravityDir;
    }
}