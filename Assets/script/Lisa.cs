using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lisa : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player;
    public FirstPersonCamera playerCamera;
    public GameObject lookat;

    [Header("参数设置")]
    public float showPlayerXThreshold = 5f;
    public float followDistance = 1.5f;  // Lisa 与玩家的固定距离
    public float jumpScareDistance = 1.0f;
    public float jumpScareHeightOffset = -0.2f;
    public float jumpScareRotationOffset = -90f;

    [Header("玩家视线检测")]
    public float lookAngleThreshold = 30f;
    public float maxLookDistance = 15f;

    [Header("动画")]
    public Animator animator;

    [Header("特效与灯光控制")]
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    public Material distortionMaterial;
    public string distortionProperty = "_DistortionStrength";
    public float distortionPeak = 1f;
    public Light pointLight;

    [Header("音效")]
    public AudioSource disappearAudio;
    public AudioSource chaseAudio;
    public AudioSource jumpScareAudio;
    public AudioSource flashAudio;

    private bool isHidden = false;
    private bool isChasing = false;
    private bool hasTriggeredJumpScare = false;

    private Renderer[] allRenderers;
    private Collider[] allColliders;
    private Light[] allLights;

    // 记录 Lisa 出现时与玩家的相对方向
    private Vector3 relativeDirection;

    private void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);
        allColliders = GetComponentsInChildren<Collider>(true);
        allLights = FindObjectsOfType<Light>();

        if (distortionMaterial != null)
            distortionMaterial.SetFloat(distortionProperty, 0f);

        if (pointLight != null)
            pointLight.enabled = false;
    }

    private void Update()
    {
        if (player == null) return;

        // Lisa 隐藏状态 → 玩家通过阈值 → 出现
        if (isHidden && !isChasing)
        {
            if (player.position.x > showPlayerXThreshold)
            {
                ShowObject();
                isChasing = true;

                if (chaseAudio != null && !chaseAudio.isPlaying)
                    chaseAudio.Play();

                StartCoroutine(DistortionFlash());
            }
        }

        // Lisa 出现后，持续保持在玩家背后（相对方向固定）
        if (isChasing && !hasTriggeredJumpScare)
        {
            FollowPlayer();

            if (IsLisaVisibleToPlayer())
            {
                TriggerJumpScare();
                return;
            }
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        // 根据记录的相对方向维持位置
        Vector3 targetPos = player.position + relativeDirection * followDistance;

        // 锁定 Lisa 的高度，不跟随玩家上下移动
        targetPos.y = transform.position.y;

        // 平滑移动（仅XZ平面）
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

        // 始终面向玩家（仅XZ平面）
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private bool IsLisaVisibleToPlayer()
    {
        if (playerCamera == null) return false;

        Vector3 toLisa = transform.position - playerCamera.transform.position;

        // 只检测水平距离
        Vector3 toLisaXZ = new Vector3(toLisa.x, 0f, toLisa.z);
        float distance = toLisaXZ.magnitude;

        if (distance > maxLookDistance)
            return false;

        float angle = Vector3.Angle(playerCamera.transform.forward, toLisaXZ);
        if (angle > lookAngleThreshold)
            return false;

        // ✅ 修复：只要在视角范围内就算看到了Lisa，不依赖Raycast
        return true;
    }

    private void TriggerJumpScare()
    {
        if (hasTriggeredJumpScare) return;
        hasTriggeredJumpScare = true;
        isChasing = false;

        if (playerCamera != null)
        {
            // Lisa 突脸位置
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * jumpScareDistance;
            targetPos.y = playerCamera.transform.position.y + jumpScareHeightOffset;
            transform.position = targetPos;

            // 朝向摄像机
            Vector3 toCamera = playerCamera.transform.position - transform.position;
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(toCamera);
                transform.rotation = lookRotation * Quaternion.Euler(0f, jumpScareRotationOffset, 0f);
            }
        }

        if (chaseAudio != null && chaseAudio.isPlaying)
            chaseAudio.Stop();

        if (jumpScareAudio != null)
            jumpScareAudio.Play();

        if (animator != null)
            animator.SetTrigger("fuck");

        if (playerCamera != null && lookat != null)
            playerCamera.LockOn(lookat.transform);

        StartCoroutine(DelayLoadScene());
    }

    private IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneLoader.LoadSceneByIndex(1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggeredJumpScare) return;

        if (other.CompareTag("Player") && !isChasing)
        {
            HideObject();
        }
    }

    private void HideObject()
    {
        if (hasTriggeredJumpScare) return;

        isHidden = true;

        foreach (Renderer rend in allRenderers) rend.enabled = false;
        foreach (Collider col in allColliders) col.enabled = false;

        foreach (Light l in allLights)
        {
            if (l != null)
                l.color = flashColor;
        }

        if (pointLight != null)
            pointLight.enabled = true;

        if (disappearAudio != null)
            disappearAudio.Play();

        StartCoroutine(DistortionFlash());
    }

    private void ShowObject()
    {
        isHidden = false;

        foreach (Renderer rend in allRenderers) rend.enabled = true;
        foreach (Collider col in allColliders) col.enabled = true;

        // 以玩家朝向为参考生成位置（记录相对方向）
        if (playerCamera != null)
        {
            relativeDirection = -playerCamera.transform.forward.normalized; // 出现在背后
            Vector3 appearPos = player.position + relativeDirection * followDistance;

            // ✅ 保持 Lisa 原来的高度
            appearPos.y = transform.position.y;

            transform.position = appearPos;

            // 面向玩家
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            transform.rotation = Quaternion.LookRotation(toPlayer);
        }

        foreach (Light l in allLights)
        {
            if (l != null)
                l.color = flashColor;
        }

        if (pointLight != null)
            pointLight.enabled = true;
    }

    private IEnumerator DistortionFlash()
    {
        if (hasTriggeredJumpScare) yield break;

        if (flashAudio != null)
            flashAudio.Play();

        if (distortionMaterial != null)
        {
            distortionMaterial.SetFloat(distortionProperty, distortionPeak);
            yield return new WaitForSeconds(flashDuration);
            distortionMaterial.SetFloat(distortionProperty, 0f);
        }
    }
}
