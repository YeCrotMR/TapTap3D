using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform target;         
    public float mouseSensitivity = 120f;

    [Header("相机高度")]
    public float heightOffset = 1.7f; 
    public float crouchHeightOffset = 1.0f; // 下蹲高度
    public float crouchLerpSpeed = 8f;      // 平滑过渡速度

    public float yMinLimit = -80f;    
    public float yMaxLimit = 80f;     

    [Header("移动摇晃参数 (走路)")]
    public float moveBobFrequency = 6f;    
    public float moveBobAmplitudeY = 0.05f; 
    public float moveBobAmplitudeX = 0.03f; 

    [Header("奔跑摇晃参数")]
    public float runBobFrequency = 9f;    
    public float runBobAmplitudeY = 0.09f; 
    public float runBobAmplitudeX = 0.06f; 

    [Header("静止呼吸参数")]
    public float idleBobFrequency = 1.5f;  
    public float idleBobAmplitudeY = 0.015f; 
    public float idleBobAmplitudeX = 0.01f;  

    private float xRotation = 0f;     
    private float yRotation = 0f;     
    private float bobTimer = 0f;      
    private Vector3 bobOffset;        

    private Rigidbody _rb;            
    private FirstPlayerController _player; 

    private float currentHeight;  // 平滑过渡中的实际高度

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yRotation = angles.y;
        xRotation = angles.x;

        _rb = target.GetComponent<Rigidbody>();
        _player = target.GetComponent<FirstPlayerController>();

        currentHeight = heightOffset; // 初始高度
    }

    void LateUpdate()
    {
        if (!target) return;

        // 鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, yMinLimit, yMaxLimit);

        // ---- 晃动计算 ----
        Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        bool isMoving = horizontalVel.magnitude > 0.1f;

        float frequency, amplitudeY, amplitudeX;

        if (isMoving)
        {
            if (_player.isRunning) // 奔跑时
            {
                frequency = runBobFrequency;
                amplitudeY = runBobAmplitudeY;
                amplitudeX = runBobAmplitudeX;
            }
            else // 走路
            {
                frequency = moveBobFrequency;
                amplitudeY = moveBobAmplitudeY;
                amplitudeX = moveBobAmplitudeX;
            }
        }
        else // 静止呼吸
        {
            frequency = idleBobFrequency;
            amplitudeY = idleBobAmplitudeY;
            amplitudeX = idleBobAmplitudeX;
        }

        bobTimer += Time.deltaTime * frequency;

        float bobY = Mathf.Sin(bobTimer) * amplitudeY;
        float bobX = Mathf.Sin(bobTimer * 2f) * amplitudeX;

        bobOffset = new Vector3(bobX, bobY, 0);

        // ---- 相机高度平滑过渡 ----
        float targetHeight = _player.isCrouching ? crouchHeightOffset : heightOffset;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchLerpSpeed);

        Vector3 basePos = target.position + Vector3.up * currentHeight;

        // ✅ 世界坐标防止放大偏移
        transform.position = basePos + target.right * bobOffset.x + Vector3.up * bobOffset.y;

        // 相机旋转
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // 控制角色水平旋转
        target.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
