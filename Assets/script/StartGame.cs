using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    public void GameStart()
    {
        string sceneName = "firstPerson";
        Scene ExistingScene = SceneManager.GetSceneByName(sceneName);
        if (ExistingScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(ExistingScene);
        }
        SceneManager.LoadScene(sceneName);
        Debug.Log("Start.");
    }
}
