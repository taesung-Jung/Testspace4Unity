using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class UserMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    public Transform cameraTransform;
    public float mouseSensitivity = 200f;

    public float bobFrequency = 8f;      // 움직임 빈도
    public float bobAmplitudeY = 0.05f;  // 카메라 위치 Y 변화
    public float bobAmplitudePitch = 1f; // 시점 pitch 흔들림 크기

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private float bobTimer = 0f;
    private Vector3 cameraInitialLocalPos;

    private float basePitch = 0f;     // 마우스 기반 pitch
    private float currentPitch = 0f;  // 흔들림 포함 최종 pitch

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform != null)
            cameraInitialLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        // === 마우스 시점 회전 ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        basePitch -= mouseY;
        basePitch = Mathf.Clamp(basePitch, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);

        // === 지면 체크 및 중력 ===
        isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // === 이동 입력 ===
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        if (dir.magnitude > 1f) dir.Normalize();

        cc.Move(dir * speed * Time.deltaTime);

        // === 점프 ===
        if (isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        // === 카메라 위치 흔들림 + 시점 흔들림 (위아래 시야 변화) ===
        if (cameraTransform != null)
        {
            if (isGrounded && dir.magnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency;

                // 위치 흔들림
                float offsetY = Mathf.Sin(bobTimer) * bobAmplitudeY;
                cameraTransform.localPosition = cameraInitialLocalPos + new Vector3(0, offsetY, 0);

                // pitch 흔들림 추가 (위아래로 시야 튐)
                float pitchOffset = Mathf.Sin(bobTimer * 2f) * bobAmplitudePitch;
                currentPitch = basePitch + pitchOffset;
            }
            else
            {
                bobTimer = 0f;
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition,
                    cameraInitialLocalPos,
                    Time.deltaTime * 5f
                );

                currentPitch = Mathf.Lerp(currentPitch, basePitch, Time.deltaTime * 5f);
            }

            // 적용된 최종 시점 설정
            cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }
}