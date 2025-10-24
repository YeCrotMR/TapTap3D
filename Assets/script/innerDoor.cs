using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class innerDoor : MonoBehaviour
{
    [Header("提示文字对象（可选）")]
    public GameObject promptText; // “按E开门”提示

    [Header("门动画设置")]
    public Animator doorAnimator; // 当前门的 Animator（自动获取）
    public string openParameter = "isOpen"; // Animator 参数名

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E; // 交互按键
    public bool canInteract = true;          // 防止重复交互

    [Header("锁门选项")]
    [Tooltip("勾选后门无法打开，直到手动解锁。")]
    public bool isLocked = false;            // 是否锁门
    public UnityEvent onDoorLocked;          // 锁门事件
    public UnityEvent onDoorUnlocked;        // 解锁事件

    [Header("可选关联门")]
    public GameObject linkedDoor; // 关联门对象（只需拖GameObject）

    [Header("开关门事件")]
    public UnityEvent onDoorOpened;               // 普通开门事件
    public UnityEvent onDoorOpenedForward;        // 正向开门事件
    public UnityEvent onDoorOpenedBackward;       // 反向开门事件
    public UnityEvent onDoorClosed;               // 关门事件

    [Header("音效设置")]
    public AudioSource audioSource;               // 播放音源（可自动获取）
    public AudioClip openSound;                   // 开门音效
    public AudioClip closeSound;                  // 关门音效
    [Range(0f, 1f)] public float soundVolume = 1f; // 音量调节

    private bool isPlayerNear = false;
    public bool isOpen = false;
    public bool isIntialDoor;

    private Animator linkedDoorAnimator;
    private Transform player;

    [Header("开门方向状态（+1=正向，-1=反向）")]
    [Tooltip("外部程序可读取此值判断开门方向（写入也可触发锁存）")]
    public int openDirection = 0;

    [Header("锁存方向（自动记录第一次开门方向）")]
    [Tooltip("只在门打开时记录方向，关门后重置")]
    public int latchedOpenDirection = 0;

    [Header("自动关门参数")]
    [Tooltip("玩家离开多远时自动关门（仅门打开后检测）")]
    public float autoCloseDistance = 3f;

    public void Start()
    {
        // 自动获取 Animator
        doorAnimator = GetComponent<Animator>();

        // 自动获取 AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // 自动查找 Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        

        // 查找关联门 Animator
        if (linkedDoor != null)
        {
            linkedDoorAnimator = linkedDoor.GetComponent<Animator>();
            
        }

        if (promptText != null)
            promptText.SetActive(false);
    }

    void Update()
    {
        // --- 捕捉外部设置的 openDirection 并锁存 ---
        if (openDirection != 0 && latchedOpenDirection == 0)
        {
            latchedOpenDirection = openDirection;
        }

        // --- 玩家交互逻辑 ---
        if (isPlayerNear && Input.GetKeyDown(interactKey) && canInteract)
        {
            if (isLocked)
            {
                onDoorLocked?.Invoke();
                return;
            }

            if (!isOpen)
            {
                OpenDoor();
            }
            else
            {
                CloseDoor(); // ✅ 如果门已开，再按一次关门
            }
        }

        // --- 自动关门检测 ---
        
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        canInteract = true;

        // --- 计算开门方向 ---
        if (player != null)
        {
            float relativeX = player.position.x - transform.position.x;
            openDirection = (relativeX < 0) ? +1 : -1;
        }
        else
        {
            openDirection = +1;
        }

        if (latchedOpenDirection == 0)
            latchedOpenDirection = openDirection;

        // --- 播放动画 ---
        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
            doorAnimator.SetBool(openParameter, true);

        if (linkedDoorAnimator != null && !string.IsNullOrEmpty(openParameter))
            linkedDoorAnimator.SetBool(openParameter, true);

        if (promptText != null)
            promptText.SetActive(false);

        // --- 播放开门音效 ---
        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound, soundVolume);

        // --- 事件触发 ---
        onDoorOpened?.Invoke();
        if (openDirection > 0)
            onDoorOpenedForward?.Invoke();
        else
            onDoorOpenedBackward?.Invoke();

    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;
        canInteract = true;

        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
            doorAnimator.SetBool(openParameter, false);

        if (linkedDoorAnimator != null && !string.IsNullOrEmpty(openParameter))
            linkedDoorAnimator.SetBool(openParameter, false);

        // --- 播放关门音效 ---
        if (audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound, soundVolume);

        openDirection = 0;
        latchedOpenDirection = 0;

        onDoorClosed?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            isPlayerNear = true;
            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    public void CloseDoorInstantly()
    {
        isOpen = false;
        canInteract = true;
        isLocked = false;

        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
            doorAnimator.SetBool(openParameter, false);

        if (linkedDoorAnimator != null && !string.IsNullOrEmpty(openParameter))
            linkedDoorAnimator.SetBool(openParameter, false);

        openDirection = 0;
        latchedOpenDirection = 0;
    }

    // -------------------------------
    // 🔒 外部控制接口
    // -------------------------------
    public void LockDoor()
    {
        isLocked = true;
        onDoorLocked?.Invoke();
    }

    public void UnlockDoor()
    {
        isLocked = false;
        onDoorUnlocked?.Invoke();
    }
}
