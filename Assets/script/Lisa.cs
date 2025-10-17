using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lisa : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player;             // 玩家Transform
    public FirstPersonCamera playerCamera;
    public GameObject lookat;

    [Header("参数设置")]
    public float showPlayerXThreshold = 5f;   // 玩家X坐标阈值
    public float chaseSpeed = 3f;             // 追踪速度
    public float chaseSpeedMultiplier = 2f;   // 加速倍数（当玩家y角度在 -180~0）
    public float chaseDistance = 2f;          // 触发切换场景的距离

    private bool isHidden = false;            // 当前物体是否隐藏
    private bool isChasing = false;           // 是否开始追踪

    private Renderer[] allRenderers;          // ✅ 自身及所有子级Renderer
    private Collider[] allColliders;          // ✅ 自身及所有子级Collider

    private void Start()
    {
        // ✅ 获取自己及所有子物体上的 Renderer 和 Collider
        allRenderers = GetComponentsInChildren<Renderer>(true);
        allColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Update()
    {
        if (player == null) return;

        // ✅ 如果已经隐藏，等待玩家达到指定X坐标
        if (isHidden && !isChasing)
        {
            if (player.position.x > showPlayerXThreshold)
            {
                ShowObject();
                isChasing = true;
            }
        }

        // ✅ 开始追踪逻辑
        // ✅ 开始追踪逻辑
if (isChasing)
{
    float playerY = player.eulerAngles.y;
    if (playerY > 180f) playerY -= 360f;  // 转换为 -180 ~ 180 区间

    float currentSpeed = chaseSpeed;
    if (playerY >= -180f && playerY <= 0f)
        currentSpeed *= chaseSpeedMultiplier;

    // 向玩家移动
    Vector3 direction = (player.position - transform.position).normalized;
    Vector3 move = direction * currentSpeed * Time.deltaTime;

    // ✅ 保持在地面高度
    float groundY = transform.position.y;
    transform.position += move;
    transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

    // ✅ 只在Y轴旋转朝向玩家（防止抬头低头）
    Vector3 lookPos = player.position - transform.position;
    lookPos.y = 0;
    transform.rotation = Quaternion.LookRotation(lookPos);

    // ✅ 检测是否追上
    float distance = Vector3.Distance(transform.position, player.position);
    if (distance <= chaseDistance)
    {
        isChasing = false;  // 停止追踪

        // ✅ 保持Y轴朝向玩家
        lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookPos);

        playerCamera.LockOn(lookat.transform);
        StartCoroutine(DelayLoadScene());
    }
}

    }

    private IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneLoader.LoadSceneByIndex(1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isChasing)
            {
                HideObject();
            }
        }
    }

    // ✅ 隐藏自身和所有子级的Renderer与Collider
    private void HideObject()
    {
        isHidden = true;

        foreach (Renderer rend in allRenderers)
        {
            rend.enabled = false;
        }

        foreach (Collider col in allColliders)
        {
            col.enabled = false;
        }
    }

    // ✅ 显示自身和所有子级的Renderer与Collider
    private void ShowObject()
    {
        isHidden = false;

        foreach (Renderer rend in allRenderers)
        {
            rend.enabled = true;
        }

        foreach (Collider col in allColliders)
        {
            col.enabled = true;
        }
    }
}
