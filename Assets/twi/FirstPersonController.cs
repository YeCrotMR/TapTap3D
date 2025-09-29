using UnityEngine;

public class FirstPlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;
    private CapsuleCollider _collider;

    [Header("移动参数")]
    public float walkSpeed = 5f;   // 走路
    public float runSpeed = 9f;    // 奔跑
    public float crouchSpeed = 2.5f; // 下蹲速度
    public float rotationSpeed = 5f;

    [Header("跳跃参数")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [SerializeField] private float fallMultiplier = 2.5f;

    public bool isGrounded;
    private Vector3 moveDir;

    // 状态
    [HideInInspector] public bool isRunning = false;  
    [HideInInspector] public bool isCrouching = false;

    // 碰撞体原始参数
    private float originalHeight;
    private Vector3 originalCenter;
    public float crouchHeight = 1f;
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();

        // 保存原始碰撞箱参数
        originalHeight = _collider.height;
        originalCenter = _collider.center;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 摄像机方向
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f; camRight.Normalize();

        moveDir = (camForward * vertical + camRight * horizontal).normalized;

        // Shift 奔跑
        isRunning = Input.GetKey(KeyCode.LeftShift) && moveDir != Vector3.zero && !isCrouching;

        // Ctrl 下蹲
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        // 碰撞箱调整
        if (isCrouching)
        {
            Debug.Log("无产阶级");
            _collider.height = crouchHeight;
            _collider.center = crouchCenter;
        }
        else
        {
            Debug.Log("傻逼");
            _collider.height = originalHeight;
            _collider.center = originalCenter;
        }

        // 动画
        // _animator.SetBool("isRun", moveDir != Vector3.zero && !isCrouching);
        // _animator.SetBool("isRunFast", isRunning);
        // _animator.SetBool("isCrouch", isCrouching);

        // 地面检测
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && Input.GetButtonDown("Jump") && !isCrouching)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
        }
    Debug.Log("isGrounded = " + isGrounded);
        
    }

    void FixedUpdate()
    {
        if (moveDir != Vector3.zero)
        { 
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            float currentSpeed;
            if (isCrouching)
                currentSpeed = crouchSpeed;
            else if (isRunning)
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed;

            Vector3 targetVelocity = moveDir * currentSpeed;
            targetVelocity.y = _rb.velocity.y;
            _rb.velocity = targetVelocity;
        }
        else
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }

        if (_rb.velocity.y < 0)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
