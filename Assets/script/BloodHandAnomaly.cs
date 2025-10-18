using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodHandAnomaly : MonoBehaviour
{
    [Header("音效设置")]
    public AudioClip hitSound;              // 撞击窗户音效
    public float triggerRadius = 4f;        // 玩家靠近的触发范围

    [Header("血手印对象")]
    public GameObject bloodHandprints;      // 血手印贴图对象（初始设为隐藏）

    [Header("玩家设置")]
    public GameObject Player = null;        // 玩家对象
    public Camera playerCamera = null;      // 玩家视角摄像机（用于朝向检测）

    private AudioSource audioSource;        
    private Transform playerTransform;      
    private bool hasTriggered = false;      // 已触发事件

    [Header("朝向检测参数")]
    public float backTriggerAngle = 60f;    // 当玩家与窗户夹角大于此值，视为“没看着窗户”

    void Start()
    {
        // 初始化
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");

        if (Player != null)
            playerTransform = Player.transform;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (bloodHandprints != null)
            bloodHandprints.SetActive(false);  // 开始时隐藏
    }

    void Update()
    {
        if (Player == null || playerCamera == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 玩家进入范围，且事件尚未触发
        if (!hasTriggered && distance <= triggerRadius)
        {
            Vector3 toWindow = (transform.position - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, toWindow);

            // 若玩家没看着窗户（背对或偏离视线）
            if (angle > backTriggerAngle)
            {
                TriggerAnomaly();
                hasTriggered = true;
            }
        }
    }

    void TriggerAnomaly()
    {
        PlayHitSound();
        RevealBloodHandprints();
        Debug.Log("【异常事件】窗户撞击 + 血手印出现！");
    }

    void PlayHitSound()
    {
        if (hitSound != null)
        {
            audioSource.clip = hitSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("未指定撞击音效！");
        }
    }

    void RevealBloodHandprints()
    {
        if (bloodHandprints != null)
        {
            bloodHandprints.SetActive(true);
        }
    }

    // 在编辑器中可视化触发范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
