using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [Header("菜单引用")]
    public GameObject SettingsPenal;

    // Start is called before the first frame update
    void Start()
    {
        if (SettingsPenal != null)
            SettingsPenal.SetActive(false);
    }

    public void Settings()
    {
        if (SettingsPenal != null)
            SettingsPenal.SetActive(true);
    }
}
