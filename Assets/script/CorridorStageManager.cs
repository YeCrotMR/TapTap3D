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
    public float specialforwardOffsetX = -10f;
    public float specialBackwardOffsetX = -10f;


    private int currentStage = 1;
    private GameObject currentCorridor;
    private GameObject previousCorridor;
    private GameObject lastpreviousCorridor;

    private bool[] usedAbnormalCorridors;
    private bool isTransitioning = false;

    private int forwardCount = 0;
    private int backwardCount = 0;
    private int lastDirection = 0;
    private int nowDirection = 0;

    void Start()
    {
        usedAbnormalCorridors = new bool[abnormalCorridors.Length];
        previousCorridor = null;
        lastpreviousCorridor = null;

        currentCorridor = initialCorridor != null ? initialCorridor : normalCorridors[0];
        currentCorridor.SetActive(true);
        ResetDoorState(currentCorridor);

        Debug.Log($"初始阶段：{currentStage}，当前走廊：{currentCorridor.name}");
    }

    void Update()
    {
        //if (currentCorridor == null || isTransitioning) return;
        
        if(currentCorridor != initialCorridor){
            
            DoorInteraction door = currentCorridor.GetComponentInChildren<DoorInteraction>();
            

            if (door == null) return;

            if (door.openDirection != 0 )
            {
                int dir = door.openDirection;
                door.openDirection = 0;
                isTransitioning = true;
                if (dir > 0)
                    MoveForward();
                else
                    MoveBackward();
                    //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
            }

            if(previousCorridor != null && previousCorridor != initialCorridor){
                DoorInteraction predoor = previousCorridor.GetComponentInChildren<DoorInteraction>();

                if (predoor.openDirection != 0 )
                {
                    
                    int dir = predoor.openDirection;
                    predoor.openDirection = 0;
                    isTransitioning = true;

                    if (dir > 0)
                        MoveForward();
                    else
                        MoveBackward();
                        //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
                }
            }else if(previousCorridor == initialCorridor){
                if (initialDoorA.openDirection != 0)
                {
                    int dir = initialDoorA.openDirection;
                    initialDoorA.openDirection = 0;
                    isTransitioning = true;

                    if (dir > 0)
                        MoveForward();
                    else
                        MoveBackward();
                        //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
                }
                if (initialDoorB.openDirection != 0)
                {
                    int dir = initialDoorB.openDirection;
                    initialDoorB.openDirection = 0;
                    isTransitioning = true;

                    if (dir > 0)
                        MoveForward();
                    else
                        MoveBackward();
                        //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
                }
            }
        }else{
            if (initialDoorA.openDirection != 0)
            {
                int dir = initialDoorA.openDirection;
                initialDoorA.openDirection = 0;
                isTransitioning = true;

                if (dir > 0)
                    MoveForward();
                else
                    MoveBackward();
                    //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
            }

            if (initialDoorB.openDirection != 0)
            {
                int dir = initialDoorB.openDirection;
                initialDoorB.openDirection = 0;
                isTransitioning = true;

                if (dir > 0)
                    MoveForward();
                else
                    MoveBackward();
                    //Debug.Log($"nowDirection：{nowDirection}，lastDirection：{lastDirection}");
            }
        }
    }

    void MoveForward()
    {
        lastDirection = nowDirection;
        nowDirection = 1;

        
        // 异常走廊前进回到第1阶段
        if (IsCurrentCorridorAbnormal())
        {
            ResetToFirstStage(1);
            isTransitioning = false;
            return;
        }
        if(previousCorridor != null){
            lastpreviousCorridor = previousCorridor;
            
        }
        previousCorridor = currentCorridor;
        currentStage++;

        if(lastpreviousCorridor != null){
        DoorInteraction[] lastpredoor = lastpreviousCorridor.GetComponentsInChildren<DoorInteraction>();
         foreach (var d in lastpredoor)
        {
            d.isLocked = true;
        }
        //lastpredoor.isLocked = true;
        }
        initialDoorB.isLocked = true;
        if(lastpreviousCorridor == initialCorridor){
            initialDoorA.isLocked = true;
        }

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
        ActivateRelevantCorridors();
        DoorInteraction newdoor = previousCorridor.transform.GetChild(3).gameObject.GetComponentInChildren<DoorInteraction>();
        newdoor.isLocked = true;
        isTransitioning = false;
        forwardCount++;
        
        Debug.Log($"进入第{currentStage}阶段（{(useNormal ? "正常" : "异常")}走廊）：{currentCorridor.name}");
    }

    void MoveBackward()
    {
        lastDirection = nowDirection;
        nowDirection = -1;
        // 正常走廊后退回到第1阶段
        if (IsCurrentCorridorNormal())
        {
            ResetToFirstStage(-1);
            isTransitioning = false;
             return;
        }
        if(previousCorridor != null){
            lastpreviousCorridor = previousCorridor;
            //Debug.Log(lastpreviousCorridor.name);
        }
        previousCorridor = currentCorridor;
        currentStage++;

        if(lastpreviousCorridor != null){
        DoorInteraction[] lastpredoor = lastpreviousCorridor.GetComponentsInChildren<DoorInteraction>();
        foreach (var d in lastpredoor)
        {
            d.isLocked = true;
        }
        //lastpredoor.isLocked = true;
        
        }
        initialDoorA.isLocked = true;
        if(lastpreviousCorridor == initialCorridor){
            initialDoorB.isLocked = true;
        }
        
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
        ActivateRelevantCorridors();
        DoorInteraction newdoor = previousCorridor.transform.GetChild(3).gameObject.GetComponentInChildren<DoorInteraction>();
        newdoor.isLocked = true;
        isTransitioning = false;
        backwardCount++;
        
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
        int index = 0;
        for (int i = 0; i < normalCorridors.Length; i++){
            if(currentCorridor != normalCorridors[i] && previousCorridor != normalCorridors[i] && lastpreviousCorridor != normalCorridors[i] ){
                index = i;
                break;
            }
        }
        //int index = (currentStage - 1) % normalCorridors.Length;
        return normalCorridors[index];
    }

    GameObject GetRandomAbnormalCorridor()
    {
        int index = Random.Range(0, abnormalCorridors.Length);
        while (usedAbnormalCorridors[index])
        {
            index = Random.Range(0, abnormalCorridors.Length);
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

        if((nowDirection * lastDirection) < 0){
            offset = new Vector3(specialforwardOffsetX, 0, backwardOffsetZ);
        }

        corridor.transform.position = reference.transform.position + offset;
        corridor.transform.localScale = new Vector3(1, 1, 1);
        
        }else{
            Vector3 offset = new Vector3(backwardOffsetX, 0, backwardOffsetZ);
                if(nowDirection != lastDirection){
                    offset = new Vector3(specialBackwardOffsetX, 0, backwardOffsetZ);
                }
            corridor.transform.position = reference.transform.position + offset;
            corridor.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void ActivateRelevantCorridors()
    {
        if (currentCorridor != null) currentCorridor.SetActive(true);
        if ((nowDirection * lastDirection) < 0){
            currentCorridor.transform.GetChild(3).gameObject.SetActive(true);
            DoorInteraction newdoor = currentCorridor.transform.GetChild(3).gameObject.GetComponentInChildren<DoorInteraction>();
                newdoor.Start();
                newdoor.OpenDoor();
                newdoor.openDirection = 0;
            }else{
                currentCorridor.transform.GetChild(3).gameObject.SetActive(false);
            }
        if (previousCorridor != null) previousCorridor.SetActive(true);
        if (lastpreviousCorridor != null && (nowDirection * lastDirection) >= 0){
            lastpreviousCorridor.SetActive(true);
            }else if((nowDirection * lastDirection) < 0){
                lastpreviousCorridor.SetActive(false);
            }
        

        foreach (var n in normalCorridors)
        {
            if (n != currentCorridor && n != previousCorridor && n != lastpreviousCorridor){
                n.SetActive(false);
               }
        }

        foreach (var a in abnormalCorridors)
        {
            if (a != currentCorridor && a != previousCorridor && a != lastpreviousCorridor){
                a.SetActive(false);
               }
        }

        if (initialCorridor != currentCorridor && initialCorridor != previousCorridor && initialCorridor != lastpreviousCorridor){
                initialCorridor.SetActive(false);
               }
        Debug.Log("C:"+currentCorridor.name + " P:"+previousCorridor.name+" L:" + lastpreviousCorridor.name);
    }

    void ResetToFirstStage(int dir)
    {
        currentStage = 1;
        if(previousCorridor != null){
            lastpreviousCorridor = previousCorridor;
        }
        previousCorridor = currentCorridor;
        if(lastpreviousCorridor != null){
            DoorInteraction[] lastpredoor = lastpreviousCorridor.GetComponentsInChildren<DoorInteraction>();
            foreach (var d in lastpredoor)
            {
                d.isLocked = true;
            }
            //lastpredoor.isLocked = true;
        }
        if(dir>0){
            if(lastpreviousCorridor == initialCorridor){
            initialDoorA.isLocked = true;
        }
        }else{
            initialDoorB.isLocked = true;
        }
        
        
        //currentCorridor = normalCorridors[0];
        for (int i = 0; i < normalCorridors.Length; i++){
            if(currentCorridor != normalCorridors[i] && previousCorridor != normalCorridors[i] && lastpreviousCorridor != normalCorridors[i]){
                currentCorridor = normalCorridors[i];
                break;
            }
        }
        PositionCorridor(currentCorridor, previousCorridor, true,dir);
        ResetDoorState(currentCorridor);
        ActivateRelevantCorridors();
        DoorInteraction newdoor = previousCorridor.transform.GetChild(3).gameObject.GetComponentInChildren<DoorInteraction>();
        newdoor.isLocked = true;

        for (int i = 0; i < usedAbnormalCorridors.Length; i++)
            usedAbnormalCorridors[i] = false;

        Debug.Log("回到第1阶段，重置完成"+currentCorridor.name);
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
