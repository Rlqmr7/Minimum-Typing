using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EscapeManager : MonoBehaviour
{
    [Header("遷移先のシーン名")]
    public string titleSceneName = "TitleScene";

    void Update()
    {
        // 新しい Input System の Keyboard クラスを使用して Esc キーの入力を安全に検知
        // wasPressedThisFrame は「そのフレームで押された瞬間」を正確に捉えます
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LoadTitleScene();
        }
    }

    public void LoadTitleScene()
    {
        // 指定したシーンが Build Settings に登録されているか事前に安全チェック
        if (Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.Log($"<color=cyan>【Input System】Escキー検知: '{titleSceneName}' に遷移します。</color>");
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            Debug.LogError($"<color=red>【エラー】'{titleSceneName}' が Build Settings に登録されていないか、シーン名が間違っています。</color>");
        }
    }
}