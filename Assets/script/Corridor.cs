using UnityEngine;

public enum CorridorType
{
    Normal,   // 正常走廊
    Abnormal  // 异常走廊
}

public class Corridor : MonoBehaviour
{
    public CorridorType corridorType; // 走廊类型
    public bool isActive = false; // 是否已经激活

    void Start()
    {
        // 默认不激活
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}
