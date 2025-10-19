using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("提示文字对象（可选）")]
    public GameObject promptText; // “按E开门”提示

    [Header("门动画设置")]
    private Animator doorAnimator; // 当前门的 Animator（自动获取）
    public string openParameter = "isOpen"; // Animator 参数名
    public string openDirectionParameter = "OpenDirection"; // Animator 参数名（float或int）

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E; // 交互按键
    public bool canInteract = true;          // 防止重复交互

    [Header("可选关联门")]
    public GameObject linkedDoor; // 关联门对象（只需拖GameObject）

    [Header("开门事件")]
    public UnityEvent onDoorOpened;               // 普通开门事件
    public UnityEvent onDoorOpenedForward;        // 正向开门事件
    public UnityEvent onDoorOpenedBackward;       // 反向开门事件

    private bool isPlayerNear = false;
    public bool isOpen = false;

    private Animator linkedDoorAnimator; // 自动识别的关联门Animator
    private Transform player;            // 自动找到玩家对象

    [Header("开门方向状态（+1=正向，-1=反向）")]
    [Tooltip("外部程序可读取此值判断开门方向")]
    public int openDirection = 0; // 供外部脚本读取使用

    void Start()
    {
        // 自动获取 Animator
        doorAnimator = GetComponent<Animator>();

        // 自动查找 Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[DoorInteraction] 未找到Tag为 'Player' 的对象，无法判断开门方向。");

        // 查找关联门 Animator
        if (linkedDoor != null)
        {
            linkedDoorAnimator = linkedDoor.GetComponent<Animator>();
            if (linkedDoorAnimator == null)
                Debug.LogWarning($"[DoorInteraction] 关联门 {linkedDoor.name} 上没有找到 Animator！");
        }

        if (promptText != null)
            promptText.SetActive(false);
    }

    void Update()
    {
        if (!canInteract || isOpen) return;

        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        canInteract = false;

        // --- 计算开门方向 ---
        if (player != null)
        {
            float relativeX = player.position.x - transform.position.x;
            openDirection = (relativeX < 0) ? +1 : -1;
            Debug.Log(openDirection);
        }
        else
        {
            openDirection = +1; // 默认正向
        }

        // --- 播放动画 ---
        if (doorAnimator != null)
        {
            if (!string.IsNullOrEmpty(openParameter))
                doorAnimator.SetBool(openParameter, true);

            if (!string.IsNullOrEmpty(openDirectionParameter))
                doorAnimator.SetFloat(openDirectionParameter, openDirection);
        }

        // --- 播放关联门动画（可选）---
        if (linkedDoorAnimator != null)
        {
            if (!string.IsNullOrEmpty(openParameter))
                linkedDoorAnimator.SetBool(openParameter, true);
            if (!string.IsNullOrEmpty(openDirectionParameter))
                linkedDoorAnimator.SetFloat(openDirectionParameter, openDirection);
        }

        // 隐藏提示
        if (promptText != null)
            promptText.SetActive(false);

        // --- 调用事件 ---
        onDoorOpened?.Invoke();

        if (openDirection > 0)
            onDoorOpenedForward?.Invoke();
        else
            onDoorOpenedBackward?.Invoke();

        Debug.Log($"[DoorInteraction] 门 {name} 已打开。方向：{(openDirection > 0 ? "正向" : "反向")}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canInteract || isOpen) return;

        if (other.CompareTag("Player"))
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
}
