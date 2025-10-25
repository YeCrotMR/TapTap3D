using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateAnormaly : MonoBehaviour
{
    [Header("日历模型")]
    public GameObject[] calendar;
    public GameObject anormalyCalendar;

    [Header("状态设置")]
    public int stageIndex;
    public int anormalStage;

    private GameObject currentCalendar;

    // Start is called before the first frame update
    void Start()
    {
        stageIndex = CorridorStageManager.currentStage;
        foreach (GameObject model in calendar)
        {
            model.SetActive(false);
        }
        calendar[stageIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
