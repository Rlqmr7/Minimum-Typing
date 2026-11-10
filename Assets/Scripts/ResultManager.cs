using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    // 指定したシーンに移動する汎用メソッド
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // リトライ用（必要に応じてゲーム本編のシーン名を指定）
    public void RetryGame(string gameSceneName)
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // 難易度選択画面に戻る
    public void BackToDifficulty()
    {
        SceneManager.LoadScene("DifficultyScene");
    }
}