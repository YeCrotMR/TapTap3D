using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffButton : MonoBehaviour
{
    [Header("菜单引用")]
    public GameObject StaffPenal;

    // Start is called before the first frame update
    void Start()
    {
        if (StaffPenal != null)
            StaffPenal.SetActive(false);
    }

    public void Settings()
    {
        if (StaffPenal != null)
            StaffPenal.SetActive(true);
    }
}
