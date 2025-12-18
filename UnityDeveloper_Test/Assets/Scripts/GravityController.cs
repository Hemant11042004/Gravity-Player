using UnityEngine;
using System.Collections;

public class GravityController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform headPivot;
    [SerializeField] private Transform hologram;   

    [Header("Gravity")]
    [SerializeField] private float gravitySpeed = 15f;
    [SerializeField] private float hologramDistance = 1.5f;

    public Vector3 CurrentGravityDirection { get; private set; } = Vector3.down;

    private Vector3 selectedGravity;
    private Quaternion targetRotation;
    private bool previewing;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.freezeRotation = true;

        if (hologram)
            hologram.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        rb.velocity +=
            CurrentGravityDirection.normalized *
            gravitySpeed *
            Time.fixedDeltaTime;
    }

    void Update()
    {
        HandlePreviewInput();

        if (previewing && Input.GetKeyDown(KeyCode.Return))
            ApplyGravity();
    }

    
    // PREVIEW INPUT
    
    void HandlePreviewInput()
    {
        previewing = false;

        if (Input.GetKey(KeyCode.UpArrow))
            Preview(Vector3.down);
        else if (Input.GetKey(KeyCode.DownArrow))
            Preview(Vector3.up);
        else if (Input.GetKey(KeyCode.LeftArrow))
            Preview(Vector3.right);
        else if (Input.GetKey(KeyCode.RightArrow))
            Preview(Vector3.left);
        else
            ClearPreview();
    }

    
    // PREVIEW
    
    void Preview(Vector3 gravityDir)
    {
        previewing = true;
        selectedGravity = gravityDir.normalized;

        Vector3 up = -selectedGravity;

        targetRotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(transform.forward, up),
            up
        );

        ShowHologram();
    }

    void ShowHologram()
    {
        if (!hologram) return;

        hologram.gameObject.SetActive(true);

        Vector3 dir = -selectedGravity;

        hologram.position =
            transform.position + dir * hologramDistance;

        hologram.rotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(Vector3.forward, -dir),
            -dir
        );
    }

    void ClearPreview()
    {
        previewing = false;

        if (hologram)
            hologram.gameObject.SetActive(false);
    }

    
    // APPLY GRAVITY
    
    void ApplyGravity()
    {
        CurrentGravityDirection = selectedGravity;

        rb.velocity = Vector3.zero;

        StopAllCoroutines();
        StartCoroutine(RotateAroundHead());

        ClearPreview();

        GameManager.Instance?.IgnoreFallCheck(0.4f);
    }

    IEnumerator RotateAroundHead()
    {
        Quaternion startRot = transform.rotation;
        Vector3 pivot = headPivot.position;

        float duration = 0.35f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            Quaternion newRot =
                Quaternion.Slerp(startRot, targetRotation, t);

            Vector3 delta =
                newRot * Quaternion.Inverse(transform.rotation) * Vector3.forward;

            Vector3 axis = Vector3.Cross(transform.forward, delta);

            if (axis.sqrMagnitude > 0.0001f)
            {
                transform.RotateAround(
                    pivot,
                    axis.normalized,
                    Vector3.Angle(transform.forward, delta)
                );
            }

            transform.rotation = newRot;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
