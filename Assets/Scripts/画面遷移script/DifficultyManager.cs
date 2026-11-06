using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour
{
    // ボタンのインスペクターから指定したシーン名に遷移する
    public void LoadStageScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("遷移先のシーン名が設定されていません！");
        }
    }
}