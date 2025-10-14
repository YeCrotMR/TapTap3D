using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 根据场景序号切换场景
    /// </summary>
    /// <param name="sceneIndex">场景在Build Settings中的序号</param>
    public static void LoadSceneByIndex(int sceneIndex)
    {
        // 检查是否序号有效
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"场景序号 {sceneIndex} 无效！请确认已在 Build Settings 中添加该场景。");
            return;
        }

        // 切换场景
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"正在切换到场景：{sceneIndex}");
    }
}
