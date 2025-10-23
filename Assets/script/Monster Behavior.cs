using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class MonsterBehavior : MonoBehaviour
{
    [Header("玩家检测")]
    public Transform player;
    public float detectionRange = 50f;   // 玩家靠近多少距离怪物开始追踪
    public FirstPersonCamera camera;
    public GameObject lookat;

    [Header("移动设置")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("闪现技能设置")]
    public float teleportDistance = 5f;
    public float teleportEffectTime = 1f;
    public float reappearDelay = 1.5f;
    public float teleportCooldown = 2f;
    [Range(0f, 1f)] public float teleportChance = 0.002f; 
    public AudioClip teleportClip;

    [Header("攻击音效设置")]
    public AudioClip attackClip; // ✅ 攻击音效

    [Header("全局灯光控制")]
    private Light[] allLights;
    private float[] originalIntensities;
    private Color[] originalColors;
    public float flashLightIntensity = 5f;
    public Color flashLightColor = Color.red;

    [Header("攻击设置")]
    public float attackRange = 2f;
    public float attackDuration = 1.2f;

    [Header("色差设置")]
    public Volume globalVolume;          // Global Volume 引用
    public float chromaticDuration = 5f; // 色差从1返回0的时间
    public float chromaticPeak = 1f;     // 色差峰值

    [Header("组件")]
    private AudioSource audioSource;
    private Animator animator;
    private Renderer[] renderers;
    private Collider[] colliders;

    [Header("闪烁材质控制")]
    public Material manualMaterial; // 手动关联的材质
    public string intensityProperty = "Intensity"; // 材质的float属性名
    public float hiddenIntensity = 0.2f;
    public float intensityDuration = 0.2f;
    private float originalIntensity = 1f;
    private bool currentVisible = true;
    private Coroutine intensityCoroutine;

    // 状态控制
    private bool hasAppeared = false;
    private bool isTeleporting = false;
    private bool isAttacking = false;
    private bool sceneLoading = false;
    private float teleportTimer = 0f;
    private float baseY;

    // 色差组件
    private ChromaticAberration chroma;
    private Coroutine chromaticCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        baseY = transform.position.y;

        // 初始化色差
        if (globalVolume != null && !globalVolume.profile.TryGet<ChromaticAberration>(out chroma))
        {
            Debug.LogError("Global Volume 中没有 Chromatic Aberration 组件");
        }
        else if (chroma != null)
        {
            chroma.intensity.value = 0f;
        }

        // 获取场景所有灯光
        allLights = FindObjectsOfType<Light>();
        originalIntensities = new float[allLights.Length];
        originalColors = new Color[allLights.Length];

        for (int i = 0; i < allLights.Length; i++)
        {
            originalIntensities[i] = allLights[i].intensity;
            originalColors[i] = allLights[i].color;
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!hasAppeared && distance <= detectionRange)
            hasAppeared = true;

        if (!hasAppeared) return;

        // 闪现冷却计时
        if (teleportTimer > 0f)
            teleportTimer -= Time.deltaTime;

        // 🔹 在攻击或闪现状态时不移动，但可以朝向玩家
        if (isTeleporting)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 🔹 平滑旋转面向玩家（攻击时也执行）
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 攻击状态下不执行后续逻辑
        if (isAttacking)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 攻击检测
        if (distance <= attackRange)
        {
            StartCoroutine(Attack());
            camera.LockOn(lookat.transform);
            StartCoroutine(DelayedSceneLoad(1, 2.5f));
            return;
        }

        // 闪现检测（攻击时不触发）
        if (!isAttacking && teleportTimer <= 0f && Random.value < teleportChance * Time.deltaTime)
        {
            StartCoroutine(FlashTeleport());
            return;
        }

        // 普通移动
        MoveTowardsPlayer(distance);
    }

    private void MoveTowardsPlayer(float distance)
    {
        float speed = 0f;

        if (distance > attackRange + 3f)
            speed = runSpeed;
        else if (distance > attackRange)
            speed = walkSpeed;

        if (speed > 0f)
        {
            Vector3 moveDir = (player.position - transform.position).normalized;
            moveDir.y = 0;
            transform.position += moveDir * speed * Time.deltaTime;
        }

        animator.SetFloat("Speed", speed / runSpeed);
    }

    private IEnumerator FlashTeleport()
    {
        isTeleporting = true;
        teleportTimer = teleportCooldown;

        // 怪物消失瞬间播放传送音效
        if (teleportClip && audioSource)
            audioSource.PlayOneShot(teleportClip);

        // 🔹 怪物消失瞬间触发色差
        TriggerChromaticEffect();

        // 🔹 怪物闪现时，全场灯光变亮红色
        SetAllLights(flashLightIntensity, flashLightColor);

        SetVisible(false);
        yield return new WaitForSeconds(teleportEffectTime);

        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * teleportDistance;
            transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
        }

        SetVisible(true);

        // ✅ 新增：怪物重新出现时播放传送音效
        if (teleportClip && audioSource)
            audioSource.PlayOneShot(teleportClip);

        // 🔹 怪物出现时触发色差
        TriggerChromaticEffect();

        // 🔹 恢复所有灯光原本状态
        for (int i = 0; i < allLights.Length; i++)
        {
            allLights[i].intensity = originalIntensities[i];
            allLights[i].color = originalColors[i];
        }

        isTeleporting = false;
    }

    private IEnumerator Attack()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        animator.SetTrigger("Attack");
        animator.SetFloat("Speed", 0f);

        // 播放攻击音效
        if (attackClip && audioSource)
            audioSource.PlayOneShot(attackClip);

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    private void SetVisible(bool visible)
    {
        if (visible == currentVisible) return;

        currentVisible = visible;

        foreach (var r in renderers)
            if (r != null) r.enabled = visible;

        foreach (var c in colliders)
            if (c != null) c.enabled = visible;

        if (manualMaterial != null && manualMaterial.HasProperty(intensityProperty))
        {
            Debug.Log("cnm");
            if (intensityCoroutine != null)
                StopCoroutine(intensityCoroutine);

            intensityCoroutine = StartCoroutine(TemporaryIntensity());
        }
    }

    private IEnumerator TemporaryIntensity()
    {
        originalIntensity = manualMaterial.GetFloat(intensityProperty);
        manualMaterial.SetFloat(intensityProperty, hiddenIntensity);
        yield return new WaitForSeconds(intensityDuration);
        manualMaterial.SetFloat(intensityProperty, originalIntensity);
        intensityCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        if (other.CompareTag("Player"))
        {
            camera.LockOn(lookat.transform);
            StartCoroutine(DelayedSceneLoad(1, 2.5f));
        }
    }

    private IEnumerator DelayedSceneLoad(int sceneIndex, float delay)
    {
        if (sceneLoading) yield break;
        sceneLoading = true;

        yield return new WaitForSeconds(delay);
        SceneLoader.LoadSceneByIndex(sceneIndex);
    }

    private void TriggerChromaticEffect()
    {
        if (chroma == null) return;

        if (chromaticCoroutine != null)
            StopCoroutine(chromaticCoroutine);

        chromaticCoroutine = StartCoroutine(ChromaticEffectCoroutine());
    }

    private IEnumerator ChromaticEffectCoroutine()
    {
        chroma.intensity.value = chromaticPeak; 
        float timer = 0f;

        while (timer < chromaticDuration)
        {
            timer += Time.deltaTime;
            chroma.intensity.value = Mathf.Lerp(chromaticPeak, 0f, timer / chromaticDuration);
            yield return null;
        }

        chroma.intensity.value = 0f;
        chromaticCoroutine = null;
    }

    private void SetAllLights(float intensity, Color color)
    {
        for (int i = 0; i < allLights.Length; i++)
        {
            allLights[i].intensity = intensity;
            allLights[i].color = color;
        }
    }
}
