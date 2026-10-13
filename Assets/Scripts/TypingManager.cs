using UnityEngine;
using TMPro; // TextMeshProを使うため

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
    }

    // キーが押された時に呼ばれる（PhysicalKeyから）
    public void OnKeyTargeted(char keyPressed)
    {
        if (isFinished) return;

        // 大文字小文字を区別しないようにする
        keyPressed = char.ToUpper(keyPressed);
        char requiredKey = char.ToUpper(targetWord[currentIndex]);

        if (keyPressed == requiredKey)
        {
            // 正解！
            Debug.Log($"<color=green>正解: {keyPressed}</color>");
            currentIndex++;

            // 全文字入力したらクリア
            if (currentIndex >= targetWord.Length)
            {
                OnClear();
            }
        }
        else
        {
            // 不正解（ミス）
            Debug.Log($"<color=red>ミス: {keyPressed} (正解は {requiredKey})</color>");
        }

        UpdateDisplay();
    }

    // UIの表示を更新する
    void UpdateDisplay()
    {
        if (displayInputText != null)
        {
            // 入力済みの部分（例：黄色やネオン風）
            string typedPart = targetWord.Substring(0, currentIndex);
            // まだ入力していない残りの部分
            string remainingPart = "";
            if (currentIndex < targetWord.Length)
            {
                remainingPart = targetWord.Substring(currentIndex);
            }

            // TextMeshProのタグを使って、入力済みを黄色、未入力を白（またはシアン）にする
            displayInputText.text = $"<color=#FFFF00>{typedPart}</color>{remainingPart}";
        }
    }

    // クリア時の処理
    void OnClear()
    {
        isFinished = true;
        Debug.Log("<color=blue>★★★ クリア！ ★★★</color>");
    }
}