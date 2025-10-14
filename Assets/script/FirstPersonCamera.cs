using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform target;         
    public float mouseSensitivity = 120f;

    [Header("相机高度")]
    public float heightOffset = 1.7f; 
    public float crouchHeightOffset = 1.0f; 
    public float crouchLerpSpeed = 8f;      

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

    private float currentHeight;  

    // === 新增：锁定目标逻辑 ===
    private bool isLockedOn = false;          // 是否锁定目标
    private Transform lockTarget;             // 被锁定的目标

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yRotation = angles.y;
        xRotation = angles.x;

        _rb = target.GetComponent<Rigidbody>();
        _player = target.GetComponent<FirstPlayerController>();

        currentHeight = heightOffset;
    }

    void LateUpdate()
    {
        if (!target) return;

        // === 锁定模式下，相机自动朝向目标，不接受鼠标输入 ===
        if (isLockedOn && lockTarget != null)
        {
            Vector3 direction = (lockTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            return; // 跳过鼠标控制
        }

        // === 鼠标输入 ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, yMinLimit, yMaxLimit);

        // ---- 判断是否移动 ----
        Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        bool hasInput = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.05f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.05f;
        bool isMoving = hasInput && horizontalVel.magnitude > 0.1f && _player.isGrounded;

        float frequency = 0f;
        float amplitudeY = 0f;
        float amplitudeX = 0f;

        if (_player.isGrounded)
        {
            if (isMoving)
            {
                if (_player.isRunning)
                {
                    frequency = runBobFrequency;
                    amplitudeY = runBobAmplitudeY;
                    amplitudeX = runBobAmplitudeX;
                }
                else
                {
                    frequency = moveBobFrequency;
                    amplitudeY = moveBobAmplitudeY;
                    amplitudeX = moveBobAmplitudeX;
                }
            }
            else
            {
                frequency = idleBobFrequency;
                amplitudeY = idleBobAmplitudeY;
                amplitudeX = idleBobAmplitudeX;
            }
        }

        if (_player.isGrounded)
        {
            bobTimer += Time.deltaTime * frequency;
            float bobY = Mathf.Sin(bobTimer) * amplitudeY;
            float bobX = Mathf.Sin(bobTimer * 2f) * amplitudeX;
            bobOffset = new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobOffset = Vector3.zero;
        }

        // ---- 相机高度平滑过渡 ----
        float targetHeight = _player.isCrouching ? crouchHeightOffset : heightOffset;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchLerpSpeed);

        Vector3 basePos = target.position + Vector3.up * currentHeight;
        transform.position = basePos + target.right * bobOffset.x + Vector3.up * bobOffset.y;

        // 相机旋转
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        target.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    // === 新增函数 ===
    /// <summary>
    /// 锁定相机朝向某个物体（自动注视）
    /// </summary>
    public void LockOn(Transform targetObj)
    {
        if (targetObj == null) return;
        isLockedOn = true;
        lockTarget = targetObj;
    }

    /// <summary>
    /// 解锁相机，让玩家重新控制
    /// </summary>
    public void Unlock()
    {
        isLockedOn = false;
        lockTarget = null;
    }
}
