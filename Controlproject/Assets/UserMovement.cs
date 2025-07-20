using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class UserMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    public Transform cameraTransform;
    public float mouseSensitivity = 100f;

    public float bobFrequency = 6f;
    public float bobAmplitude = 0.05f;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private float bobTimer = 0f;
    private Vector3 cameraInitialLocalPos;

    private float xRotation = 0f; // 카메라 pitch

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 숨김 및 고정

        if (cameraTransform != null)
            cameraInitialLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        // === 마우스 시점 회전 ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 위아래 제한

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // === 이동 및 점프 ===
        isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        if (dir.magnitude > 1f) dir.Normalize();

        cc.Move(dir * speed * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        // === 카메라 흔들림 ===
        if (cameraTransform != null)
        {
            if (isGrounded && dir.magnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float offsetY = Mathf.Sin(bobTimer) * bobAmplitude;
                cameraTransform.localPosition = cameraInitialLocalPos + new Vector3(0, offsetY, 0);
            }
            else
            {
                bobTimer = 0f;
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition,
                    cameraInitialLocalPos,
                    Time.deltaTime * 5f
                );
            }
        }
    }
}