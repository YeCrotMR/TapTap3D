using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lisa : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player;             // 玩家Transform
    public FirstPersonCamera playerCamera;

    [Header("参数设置")]
    public float showPlayerXThreshold = 5f;   // 玩家X坐标阈值
    public float chaseSpeed = 3f;             // 追踪速度
    public float chaseSpeedMultiplier = 2f;   // 加速倍数（当玩家y角度在 -180~0）
    public float chaseDistance = 2f;          // 触发切换场景的距离

    private bool isHidden = false;            // 当前物体是否隐藏
    private bool isChasing = false;           // 是否开始追踪

    private Renderer objRenderer;
    private Collider objCollider;

    private void Start()
    {
        objRenderer = GetComponent<Renderer>();
        objCollider = GetComponent<Collider>();
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
        if (isChasing)
        {
            float playerY = player.eulerAngles.y;
            if (playerY > 180f) playerY -= 360f;  // 转换为 -180 ~ 180 区间

            float currentSpeed = chaseSpeed;
            if (playerY >= -180f && playerY <= 0f)
                currentSpeed *= chaseSpeedMultiplier;

            // 向玩家移动
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;

            // 面向玩家
            transform.LookAt(player);

            // ✅ 检测是否追上
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= chaseDistance)
            {
                isChasing = false;  // ✅ 停止追踪
                transform.LookAt(player); // 保持朝向玩家
                playerCamera.LockOn(transform);
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

    private void HideObject()
    {
        isHidden = true;
        objRenderer.enabled = false;
        objCollider.enabled = false;
    }

    private void ShowObject()
    {
        isHidden = false;
        objRenderer.enabled = true;
        objCollider.enabled = true;
    }
}
