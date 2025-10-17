using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("提示文字对象（可选）")]
    public GameObject promptText; // “按E开门”提示

    [Header("门动画")]
    public Animator doorAnimator; // 门的Animator
    public string openParameter = "isOpen"; // Animator参数名

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E; // 交互按键
    public bool canInteract = true;          // 是否允许交互（防止重复开门）

    [Header("开门事件")]
    public UnityEvent onDoorOpened; // 可在Inspector里绑定激活走廊函数

    private bool isPlayerNear = false;
    public bool isOpen = false;

    void Start()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

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

        // 播放动画
        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
            doorAnimator.SetBool(openParameter, true);

        // 隐藏提示
        if (promptText != null)
            promptText.SetActive(false);

        // 调用事件（例如激活下一个走廊）
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
