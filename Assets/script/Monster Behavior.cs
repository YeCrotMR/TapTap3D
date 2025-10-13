using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour
{
    [Header("玩家检测")]
    public Transform player;
    public float detectionRange = 50f;   // 玩家靠近多少距离怪物出现

    [Header("闪现设置")]
    public float teleportDistance = 5f;  // 每次闪现距离
    public float teleportCooldown = 2f;  // 每2秒闪现一次
    public float teleportEffectTime = 1f; // 闪现特效持续时间（可以播放动画或音效）

    [Header("音效")]
    public AudioClip teleportClip;

    private bool hasAppeared = false;    // 怪物是否出现
    private float teleportTimer = 0f;    // 闪现计时器
    private Renderer[] renderers;        // 控制显示/隐藏
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // 获取所有 Renderer，包括禁用的
        renderers = GetComponentsInChildren<Renderer>(true);

        // 初始隐藏怪物
        SetVisible(false);

        // 确保 Rigidbody 不阻止移动
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    void Start()
    {
        // 如果 Player 没设置，按标签查找
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 玩家靠近出现
        if (!hasAppeared && distance <= detectionRange)
        {
            hasAppeared = true;
            SetVisible(true);
            Debug.Log("⚠️ 剪刀手出现，开始沿 X 轴正方向闪现！");
        }

        if (!hasAppeared) return;

        // 闪现计时
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= teleportCooldown)
        {
            teleportTimer = 0f;
            StartCoroutine(FlashTeleport());
        }
    }

    private IEnumerator FlashTeleport()
    {
        // 播放瞬移音效
        if (teleportClip != null && audioSource != null)
            audioSource.PlayOneShot(teleportClip);

        // 可以在这里播放闪现动画或特效
        // （比如淡入淡出、粒子效果等）
        // 假设特效播放时间为 teleportEffectTime
        yield return new WaitForSeconds(teleportEffectTime);

        // 闪现位置移动
        transform.position += Vector3.right * teleportDistance; // 👈 世界 X 轴正方向
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}