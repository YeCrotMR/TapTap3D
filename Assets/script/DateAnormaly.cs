using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateAnormaly : MonoBehaviour
{
    [Header("日历模型")]
    public GameObject[] calendar;
    public GameObject anormalyCalendar;

    private GameObject currentCalendar;
    private int loopIndex;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject model in calendar)
        {
            model.SetActive(false);
        }
        calendar[loopIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
