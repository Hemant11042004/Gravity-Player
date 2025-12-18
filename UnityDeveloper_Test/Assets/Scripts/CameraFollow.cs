using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float smoothSpeed = 10f;
    public float mouseSensitivity = 2f;

    private float yaw;
    private float pitch;

    // 🔹 ADDED: cache camera transform when moving backward
    private Vector3 cachedPosition;
    private Quaternion cachedRotation;
    private bool freezeCamera = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 🔹 ADDED: detect backward movement (S key)
        bool movingBackward = Input.GetKey(KeyCode.S);

        // 🔹 ADDED: freeze camera while moving backward
        if (movingBackward)
        {
            if (!freezeCamera)
            {
                cachedPosition = transform.position;
                cachedRotation = transform.rotation;
                freezeCamera = true;
            }

            transform.position = cachedPosition;
            transform.rotation = cachedRotation;
            return; // ❗ skip normal follow
        }

        // 🔹 ADDED: resume camera follow
        freezeCamera = false;

        // ----------------------------
        // ORIGINAL CAMERA LOGIC (UNCHANGED)
        // ----------------------------
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -40f, 80f);

        Vector3 up = target.up;

        Quaternion rotation =
            Quaternion.LookRotation(target.forward, up) *
            Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPos =
            target.position -
            rotation * Vector3.forward * distance +
            up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotation,
            smoothSpeed * Time.deltaTime
        );
    }
}
