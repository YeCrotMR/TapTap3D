using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float minDistance = 1.0f;
    public float maxDistance = 6.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 80.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float smooth = 10f;

    public float buffer = 0.5f;

    [Header("相机高度")]
    public float heightOffset = 1.5f;   // 相机默认高度
    public float heightAdjustSpeed = 2f; // 高度调节速度
    public float minHeight = 0.5f;
    public float maxHeight = 3.0f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float currentDistance;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        currentDistance = distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 鼠标旋转
        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        // 高度调节（Q下降 / E上升）
        if (Input.GetKey(KeyCode.E))
            heightOffset = Mathf.Clamp(heightOffset + heightAdjustSpeed * Time.deltaTime, minHeight, maxHeight);
        if (Input.GetKey(KeyCode.Q))
            heightOffset = Mathf.Clamp(heightOffset - heightAdjustSpeed * Time.deltaTime, minHeight, maxHeight);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 dir = rotation * Vector3.back;

        // 目标点加上高度
        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos = targetPos + dir * distance;

        // 碰撞检测
        RaycastHit hit;
        float targetDistance = distance;
        if (Physics.Raycast(targetPos, desiredPos - targetPos, out hit, distance))
        {
            targetDistance = Mathf.Clamp(hit.distance - buffer, minDistance, maxDistance);
        }

        // 平滑过渡
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smooth);

        Vector3 finalPos = targetPos + dir * currentDistance;

        transform.rotation = rotation;
        transform.position = finalPos;
    }
}
