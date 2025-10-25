using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // Start is called before the first frame update
    public void exitGame()
    {
        Debug.Log("退出程序");
        Application.Quit();
    }
}
