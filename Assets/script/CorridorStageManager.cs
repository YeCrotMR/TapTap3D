using UnityEngine;
using System.Collections.Generic;

public class CorridorStageManager : MonoBehaviour
{
    public Transform player;

    [Header("走廊模型")]
    public GameObject[] normalCorridors;
    public GameObject[] abnormalCorridors;

    [Header("初始走廊（第1阶段）")]
    public GameObject initialCorridor;

    [Header("初始走廊的两个门")]
    public DoorInteraction initialDoorA; // 第1阶段走廊的门A
    public DoorInteraction initialDoorB; // 第1阶段走廊的门B

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
    private bool isTransitioning = false;

    void Start()
    {
        usedAbnormalCorridors = new bool[abnormalCorridors.Length];
        previousCorridor = null;
        nextCorridor = null;

        currentCorridor = initialCorridor != null ? initialCorridor : normalCorridors[0];
        currentCorridor.SetActive(true);
        ResetDoorState(currentCorridor);

        Debug.Log($"初始阶段：{currentStage}，当前走廊：{currentCorridor.name}");
    }

    void Update()
    {
        if (currentCorridor == null || isTransitioning) return;

        DoorInteraction door = currentCorridor.GetComponentInChildren<DoorInteraction>();

            if (door == null) return;

            if (door.openDirection != 0)
            {
                int dir = door.openDirection;
                door.openDirection = 0;
                isTransitioning = true;

                if (dir > 0)
                    MoveForward(dir);
                else
                    MoveBackward(dir);
            }
        
        if (initialDoorA.openDirection != 0)
        {
            int dir = initialDoorA.openDirection;
            initialDoorA.openDirection = 0;
            isTransitioning = true;

            if (dir > 0)
                MoveForward(dir);
            else
                MoveBackward(dir);
        }

        if (initialDoorB.openDirection != 0)
        {
            int dir = initialDoorB.openDirection;
            initialDoorB.openDirection = 0;
            isTransitioning = true;

            if (dir > 0)
                MoveForward(dir);
            else
                MoveBackward(dir);
        }
    }

    void MoveForward(int dir)
    {
        // 异常走廊前进回到第1阶段
        if (IsCurrentCorridorAbnormal())
        {
            ResetToFirstStage(dir);
            isTransitioning = false;
            return;
        }

        previousCorridor = currentCorridor;
        currentStage++;

        bool useNormal = DecideIfNextCorridorIsNormal();
        if (useNormal)
        {
            currentCorridor = GetNextNormalCorridor();
            PositionCorridor(currentCorridor, previousCorridor, true,1);
        }
        else
        {
            currentCorridor = GetRandomAbnormalCorridor();
            PositionCorridor(currentCorridor, previousCorridor, !currentCorridor.activeSelf,1);
        }

        ResetDoorState(currentCorridor);
        nextCorridor = null;
        ActivateRelevantCorridors();
        isTransitioning = false;

        Debug.Log($"进入第{currentStage}阶段（{(useNormal ? "正常" : "异常")}走廊）：{currentCorridor.name}");
    }

    void MoveBackward(int dir)
    {
        // 正常走廊后退回到第1阶段
        if (IsCurrentCorridorNormal())
        {
            ResetToFirstStage(dir);
            isTransitioning = false;
             return;
        }

        previousCorridor = currentCorridor;
        currentStage++;

        bool useNormal = DecideIfNextCorridorIsNormal();
        if (useNormal)
        {
            currentCorridor = GetNextNormalCorridor();
            PositionCorridor(currentCorridor, previousCorridor, true,-1);
        }
        else
        {
            currentCorridor = GetRandomAbnormalCorridor();
            PositionCorridor(currentCorridor, previousCorridor, !currentCorridor.activeSelf,-1);
        }

        ResetDoorState(currentCorridor);
        nextCorridor = null;
        ActivateRelevantCorridors();
        isTransitioning = false;

        Debug.Log($"进入第{currentStage}阶段（{(useNormal ? "正常" : "异常")}走廊）：{currentCorridor.name}");
    }

    bool IsCurrentCorridorNormal()
    {
        foreach (var n in normalCorridors)
            if (n == currentCorridor) return true;
        return false;
    }

    bool IsCurrentCorridorAbnormal()
    {
        foreach (var a in abnormalCorridors)
            if (a == currentCorridor) return true;
        return false;
    }

    bool DecideIfNextCorridorIsNormal()
    {
        if (currentStage == 1) return true;
        if (currentStage >= 2 && currentStage <= 5)
            return Random.Range(0, 4) != 0; // 3/4 正常
        return false; // 第6阶段可特殊处理
    }

    GameObject GetNextNormalCorridor()
    {
        int index = (currentStage - 1) % normalCorridors.Length;
        return normalCorridors[index];
    }

    GameObject GetRandomAbnormalCorridor()
    {
        int index = Random.Range(0, abnormalCorridors.Length);
        int tries = 0;
        while (usedAbnormalCorridors[index] && tries < 20)
        {
            index = Random.Range(0, abnormalCorridors.Length);
            tries++;
        }
        usedAbnormalCorridors[index] = true;
        return abnormalCorridors[index];
    }

    void PositionCorridor(GameObject corridor, GameObject reference, bool doOffset,int dir)
    {
        if (corridor == null || reference == null) return;
        if (!doOffset) return;

        if(dir>0){
        Vector3 offset = new Vector3(forwardOffsetX, 0, forwardOffsetZ);
        corridor.transform.position = reference.transform.position + offset;
        }else{
        Vector3 offset = new Vector3(backwardOffsetX, 0, backwardOffsetZ);
        corridor.transform.position = reference.transform.position + offset;
        }
        
    }

    void ActivateRelevantCorridors()
    {
        if (currentCorridor != null) currentCorridor.SetActive(true);
        if (previousCorridor != null) previousCorridor.SetActive(true);
        if (nextCorridor != null) nextCorridor.SetActive(true);

        foreach (var n in normalCorridors)
        {
            if (n != currentCorridor && n != previousCorridor){
                //n.SetActive(false);
               }
        }

        foreach (var a in abnormalCorridors)
        {
            if (a != currentCorridor && a != previousCorridor){
                a.SetActive(false);
               }
        }

        if (initialCorridor != currentCorridor && initialCorridor != previousCorridor){
                initialCorridor.SetActive(false);
               }
    }

    void ResetToFirstStage(int dir)
    {
        currentStage = 1;
        nextCorridor = null;
        previousCorridor = currentCorridor;
        currentCorridor = normalCorridors[0];
        PositionCorridor(currentCorridor, previousCorridor, true,dir);
        ResetDoorState(currentCorridor);
        ActivateRelevantCorridors();

        for (int i = 0; i < usedAbnormalCorridors.Length; i++)
            usedAbnormalCorridors[i] = false;

        Debug.Log("回到第1阶段，重置完成");
    }

    void ResetDoorState(GameObject corridor)
    {
        if (corridor == null) return;
        DoorInteraction[] doors = corridor.GetComponentsInChildren<DoorInteraction>();
        foreach (var d in doors)
        {
            d.CloseDoorInstantly();
            d.isOpen = false;
            d.canInteract = true;
            d.openDirection = 0;
        }
    }
}
