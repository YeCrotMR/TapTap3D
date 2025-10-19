using UnityEngine;

public class CorridorStageManager : MonoBehaviour
{
    public Transform player;
    public GameObject[] normalCorridors;
    public GameObject[] abnormalCorridors;

    [Header("平移设置")]
    public float forwardOffsetX = 10f;
    public float forwardOffsetZ = 0f;
    public float backwardOffsetX = -10f;
    public float backwardOffsetZ = 0f;

    private int currentStage = 1;
    private GameObject currentCorridor;
    private GameObject previousCorridor;
    private GameObject nextCorridor;

    private bool[] usedAbnormalCorridors;

    void Start()
    {
        usedAbnormalCorridors = new bool[abnormalCorridors.Length];

        if (normalCorridors.Length > 0)
        {
            currentCorridor = normalCorridors[0];
            currentCorridor.SetActive(true);
            previousCorridor = null;
            nextCorridor = (normalCorridors.Length > 1) ? normalCorridors[1] : null;
            if (nextCorridor != null) nextCorridor.SetActive(true);
        }
        else
        {
            Debug.LogError("正常走廊数组为空！");
        }
    }

    void Update()
    {
        if (currentCorridor == null) return;

        DoorInteraction door = currentCorridor.GetComponentInChildren<DoorInteraction>();
        if (door == null || door.openDirection == 0) return;

        if (door.openDirection > 0)
        {
            HandleForwardMovement();
        }
        else
        {
            HandleBackwardMovement();
        }

        // 只保持当前走廊和相邻走廊激活
        ActivateRelevantCorridors();
    }

    void HandleForwardMovement()
    {
        if (currentStage == 6)
        {
            Debug.Log("逃脱成功！");
            return;
        }

        bool isNextNormal = IsNextCorridorNormal();
        if (isNextNormal)
        {
            TransitionToNextStage(true);
        }
        else
        {
            Debug.Log("异常走廊，回到第1阶段");
            ResetToFirstStage();
        }
    }

    void HandleBackwardMovement()
    {
        Debug.Log("折返，回到第1阶段");
        ResetToFirstStage();
    }

    bool IsNextCorridorNormal()
    {
        // 第6阶段一定异常
        if (currentStage + 1 == 6) return false;

        if (currentStage < normalCorridors.Length)
        {
            Corridor corridorScript = normalCorridors[currentStage].GetComponent<Corridor>();
            return corridorScript != null && corridorScript.corridorType == CorridorType.Normal;
        }
        return true;
    }

    void TransitionToNextStage(bool isForward)
    {
        // 更新阶段
        currentStage++;

        // 更新前后走廊引用
        previousCorridor = currentCorridor;
        currentCorridor = (currentStage == 6) ? GetRandomAbnormalCorridor() : normalCorridors[currentStage - 1];
        nextCorridor = (currentStage < normalCorridors.Length) ? normalCorridors[currentStage] : null;

        // 设置平移
        Vector3 offset = isForward
            ? new Vector3(forwardOffsetX, 0, forwardOffsetZ)
            : new Vector3(backwardOffsetX, 0, backwardOffsetZ);

        currentCorridor.transform.position = previousCorridor.transform.position + offset;

        // 激活走廊
        ActivateRelevantCorridors();
    }

    void ActivateRelevantCorridors()
    {
        // 保证当前走廊和前后相邻走廊激活，其他禁用
        foreach (var c in normalCorridors)
        {
            c.SetActive(c == currentCorridor || c == previousCorridor || c == nextCorridor);
        }

        foreach (var c in abnormalCorridors)
        {
            c.SetActive(c == currentCorridor);
        }
    }

    GameObject GetRandomAbnormalCorridor()
    {
        int index = Random.Range(0, abnormalCorridors.Length);
        int attempts = 0;
        while (usedAbnormalCorridors[index] && attempts < 10)
        {
            index = Random.Range(0, abnormalCorridors.Length);
            attempts++;
        }

        usedAbnormalCorridors[index] = true;
        return abnormalCorridors[index];
    }

    void ResetToFirstStage()
    {
        currentStage = 1;
        previousCorridor = null;
        currentCorridor = normalCorridors[0];
        nextCorridor = (normalCorridors.Length > 1) ? normalCorridors[1] : null;

        // 禁用异常走廊
        for (int i = 0; i < abnormalCorridors.Length; i++)
            abnormalCorridors[i].SetActive(false);

        // 重置异常走廊记录
        for (int i = 0; i < usedAbnormalCorridors.Length; i++)
            usedAbnormalCorridors[i] = false;

        ActivateRelevantCorridors();
    }
}
