using UnityEngine;
using TMPro;

public class TypingManager : MonoBehaviour
{
    [Header("問題設定")]
    public string targetWord = "warewareha";     // お題の英単語

    [Header("UI表示用 (TextMeshPro)")]
    public TextMeshProUGUI displayInputText;      // 入力状況を表示するテキスト

    private int currentIndex = 0;       // 現在狙っている文字のインデックス
    private bool isFinished = false;    // クリアしたか

    void Start()
    {
        UpdateDisplay();
        Debug.Log($"<color=cyan>【タイピング開始】お題: {targetWord}</color>");
    }

    // キーが押された時に呼ばれる（PhysicalKeyから）
    public void OnKeyTargeted(char keyPressed)
    {
        if (isFinished) return;

        // 大文字小文字を合わせる（必要に応じて）
        keyPressed = char.ToLower(keyPressed);
        char requiredKey = char.ToLower(targetWord[currentIndex]);

        if (keyPressed == requiredKey)
        {
            // 正解！
            currentIndex++;

            // ★ コンソールに残りの状況や進捗を表示
            string typed = targetWord.Substring(0, currentIndex);
            string remaining = targetWord.Substring(currentIndex);
            Debug.Log($"<color=green>【正解！】入力: {keyPressed} | 進捗: [{typed}]<color=gray>{remaining}</color> ({currentIndex}/{targetWord.Length})</color>");

            // 全文字入力したらクリア
            if (currentIndex >= targetWord.Length)
            {
                OnClear();
            }
        }
        else
        {
            // 不正解（ミス）
            Debug.Log($"<color=red>【ミス！】入力: {keyPressed} (正解は '{targetWord[currentIndex]}')</color>");
        }

        UpdateDisplay();
    }

    // UIの表示を更新する
    void UpdateDisplay()
    {
        if (displayInputText != null)
        {
            string typedPart = targetWord.Substring(0, currentIndex);
            string remainingPart = "";
            if (currentIndex < targetWord.Length)
            {
                remainingPart = targetWord.Substring(currentIndex);
            }

            displayInputText.text = $"<color=#FFFF00>{typedPart}</color>{remainingPart}";
        }
    }

    // クリア時の処理
    void OnClear()
    {
        isFinished = true;
        Debug.Log("<color=blue>★★★ クリア！すべて入力完了しました！ ★★★</color>");
    }
}