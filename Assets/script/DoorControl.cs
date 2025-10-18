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

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E; // 交互按键
    public bool canInteract = true;          // 防止重复交互

    [Header("可选关联门")]
    public GameObject linkedDoor; // 关联门对象（只需拖GameObject）

    [Header("开门事件")]
    public UnityEvent onDoorOpened; // 可绑定其他事件（如激活走廊）

    private bool isPlayerNear = false;
    public bool isOpen = false;

    private Animator linkedDoorAnimator; // 自动识别的关联门Animator

    void Start()
    {
        // 自动获取当前门的Animator
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        // 如果有关联门对象，则自动查找其Animator
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

        // 播放当前门动画
        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
            doorAnimator.SetBool(openParameter, true);

        // ✅ 自动播放关联门动画（如果有）
        if (linkedDoorAnimator != null && !string.IsNullOrEmpty(openParameter))
            linkedDoorAnimator.SetBool(openParameter, true);

        // 隐藏提示
        if (promptText != null)
            promptText.SetActive(false);

        // 调用事件
        onDoorOpened?.Invoke();

        Debug.Log($"[DoorInteraction] 门 {name} 已被打开");
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
