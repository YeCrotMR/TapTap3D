using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPlayerController : MonoBehaviour
{
    // Components
    private CharacterController controller;
    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    // Movement params
    [Header("移动参数")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float rotationSpeed = 10f;

    [Header("跳跃/重力")]
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float groundedGravity = -2f; // 用于保持贴地状态

    [Header("CharacterController 设置")]
    public float slopeLimit = 50f;   // 允许的最大坡度（角度）
    public float stepOffset = 0.3f;  // 小台阶可跨越高度

    // Crouch
    [Header("下蹲设置")]
    public float crouchHeight = 1f;
    public Vector3 crouchCenter = new Vector3(0f, 0.5f, 0f);
    private float originalHeight;
    private Vector3 originalCenter;

    // state
    [HideInInspector] public bool isRunning = false;
    [HideInInspector] public bool isCrouching = false;
    private Vector3 inputDir;
    private Vector3 velocity; // 包含垂直分量
    public bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 保存原始 collider 参数
        originalHeight = controller.height;
        originalCenter = controller.center;

        // 设置 controller 参数
        controller.slopeLimit = slopeLimit;
        controller.stepOffset = stepOffset;
        controller.skinWidth = 0.08f;
    }

    void Update()
    {
        HandleInput();
        HandleCrouch();
        HandleFootsteps();
        if (Input.GetKey(KeyCode.LeftControl))
                {
                    isCrouching = true;
                }
                else
                {
                    // 松开 Ctrl 的时候才考虑起身
                    if (isCrouching)
                    {
                        // 头顶有障碍，就继续保持下蹲
                        if (RaycastUpCheckCrouch(transform.position, 10f) 
                        || RaycastUpCheckCrouch(transform.position + new Vector3(1f, 0, 0), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(-1f, 0, 0), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(0, 0, 1f), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(0, 0, -1f), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(-0.71f, 0, -0.71f), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(0.71f, 0, -0.71f), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(-0.71f, 0, 0.71f), 10f)
                        || RaycastUpCheckCrouch(transform.position + new Vector3(0.71f, 0, 0.71f), 10f)
                        )
                        {
                            //Debug.Log("ttt");
                            isCrouching = true;
                        }
                        else
                        {
                            // 没障碍就起身
                            isCrouching = false;
                        }
                    }
                }
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 基于摄像机方向移动
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        inputDir = (camForward * vertical + camRight * horizontal).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift) && inputDir != Vector3.zero && !isCrouching;
        //Debug.Log("上面有东西：" + RaycastUpCheckCrouch(transform.position, 10f));

        // 跳跃（只在落地且非下蹲时）
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
        {
            // 轻微向下保持贴地
            velocity.y = groundedGravity;
        }

        if (isGrounded && Input.GetButtonDown("Jump") && !isCrouching)
        {
            // v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        // 旋转朝向和水平移动在 Fixed style (这里用 Update 做移动，便于更灵活)
        HandleMove();
    }

    private void HandleMove()
    {
        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        else if (isRunning) currentSpeed = runSpeed;

        // 平滑旋转（仅在有输入时）
        if (inputDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 当在可行走坡面并且有输入时，允许正常沿 inputDir 移动
        Vector3 move = inputDir * currentSpeed;

        // 重力应用（手动）
        if (!isGrounded)
        {
            // 更自然的下落：在下落时加速
            velocity.y += gravity * (fallMultiplier) * Time.deltaTime;
        }
        else
        {
            // 在地面时仅保留小向下速度，避免被判为“空中”
            // (jump 时 velocity.y 已由上方设置)
            // velocity.y = groundedGravity; // 已在上面处理
        }

        // 把水平与垂直合并后交给 CharacterController.Move
        Vector3 finalMove = move + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);

        // 防止 CharacterController 在极陡坡上产生滑动：
        // 当无输入且站在坡上时，手动抵消由 CharacterController 产生的滑动（通常不会大）
        if (inputDir == Vector3.zero && isGrounded)
        {
            // 检测坡面法线
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1.5f))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle <= controller.slopeLimit)
                {
                    // 在允许的坡度范围内且没有输入时，不让产生任何水平位移
                    // CharacterController.Move 可能会造成微小穿透式位移，直接修正：
                    Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
                    if (horizontalVel.magnitude > 0.01f)
                    {
                        // 把其清零（仅修正微小抖动，不影响主动移动）
                        controller.Move(-horizontalVel * Time.deltaTime);
                    }
                }
            }
        }

        // 当着地并且不是跳跃状态，确保 velocity.y 不累积过大正值
        if (isGrounded && velocity.y > 0f && !Input.GetButton("Jump"))
            velocity.y = groundedGravity;
    }

    private void HandleCrouch()
    {
        if (isCrouching)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * 10f);
            controller.center = Vector3.Lerp(controller.center, crouchCenter, Time.deltaTime * 10f);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, originalHeight, Time.deltaTime * 10f);
            controller.center = Vector3.Lerp(controller.center, originalCenter, Time.deltaTime * 10f);
        }
    }

    private void HandleFootsteps()
    {
        if (footstepSource == null) return;

        // 不在地面、静止或下蹲时停止
        if (!isGrounded || inputDir == Vector3.zero || isCrouching)
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        AudioClip targetClip = isRunning ? runClip : walkClip;
        if (!footstepSource.isPlaying || footstepSource.clip != targetClip)
        {
            footstepSource.clip = targetClip;
            footstepSource.loop = true;
            footstepSource.Play();
        }
    }

    private bool CanStandUp()
    {
        // 用 CharacterController 的 center/height 做判断：从头顶发射小球检查
        float radius = controller.radius;
        float standHeight = originalHeight;
        Vector3 worldCenter = transform.TransformPoint(originalCenter);
        Vector3 up = transform.up;
        // 顶部位置
        Vector3 top = worldCenter + up * (standHeight / 2f);
        // 检查从胸部到头顶上方是否有阻挡
        return !Physics.SphereCast(transform.position, radius, Vector3.up, out _, standHeight - controller.radius, ~0, QueryTriggerInteraction.Ignore);
    }

    public static bool RaycastUpCheckCrouch(Vector3 origin, float distance)
    {
        RaycastHit hit;
        // 发射射线，方向为Vector3.up
        if (Physics.Raycast(origin, Vector3.up, out hit, distance))
        {
            // 检查碰到的物体是否带Crouch标签
            if (hit.collider.CompareTag("Crouch"))
            {
                return true;
            }
        }
        return false;
    }
    
}
