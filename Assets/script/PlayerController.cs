using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;

    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("跳跃参数")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private bool isGrounded;
    private Vector3 moveDir;

    [SerializeField] private float fallMultiplier = 2.5f; // 下坠加速系数（默认1，越大下坠越快）


    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    void Update()
{
    // 输入
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");

    // 获取摄像机的前和右方向（只保留水平分量）
    Vector3 camForward = Camera.main.transform.forward;
    camForward.y = 0f;
    camForward.Normalize();

    Vector3 camRight = Camera.main.transform.right;
    camRight.y = 0f;
    camRight.Normalize();

    // 把输入转换到摄像机坐标系
    moveDir = (camForward * vertical + camRight * horizontal).normalized;

    // 动画
    _animator.SetBool("isRun", moveDir != Vector3.zero);

    // 地面检测
    isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

    // 跳跃
    if (isGrounded && Input.GetButtonDown("Jump"))
    {
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _animator.SetTrigger("Jump");
    }
}


    void FixedUpdate()
    {
        // 水平移动
        if (moveDir != Vector3.zero)
        {
            // 平滑旋转
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // 用 velocity 控制移动（不会穿墙）
            Vector3 targetVelocity = moveDir * moveSpeed;
            targetVelocity.y = _rb.velocity.y; // 保留垂直速度（跳跃/下落）
            _rb.velocity = targetVelocity;
        }
        else
        {
            // 停止时只保留垂直速度
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }

        // 下坠加速
        if (_rb.velocity.y < 0)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        
    }

}
