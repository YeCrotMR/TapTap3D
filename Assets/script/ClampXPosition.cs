using UnityEngine;

public class ClampXPosition : MonoBehaviour
{
    [Header("检测参数")]
    public float maxX = 10f;        // 超过这个值就触发
    public float newXValue = 5f;    // 被设置的新 X 坐标

    void Update()
    {
        Vector3 pos = transform.position;

        // 当 X 超过指定阈值时执行
        if (pos.x > maxX)
        {
            pos.x = newXValue;
            transform.position = pos;

            Debug.Log($"物体超出阈值，X 被设置为 {newXValue}");
        }
    }
}
