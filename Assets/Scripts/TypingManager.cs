using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // UIを使う場合

public class TypingManager : MonoBehaviour
{
    [Header("問題設定")]
    public string targetWord = "UNITY"; // お題の単語
    public Text displayParentText;     // お題を表示するUIテキスト（任意）
    public Text displayInputText;      // 入力状況を表示するUIテキスト（任意）

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

        // 大文字小文字を区別しないようにする（任意）
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
            // ミス時のペナルティ処理（ライフ減少、音など）をここに書く
        }

        UpdateDisplay();
    }

    // UIの表示を更新する
    void UpdateDisplay()
    {
        if (displayParentText != null)
        {
            displayParentText.text = targetWord;
        }

        if (displayInputText != null)
        {
            // 入力済みの部分を色を変えるなどして表示
            string inputText = "<color=green>" + targetWord.Substring(0, currentIndex) + "</color>";
            if (currentIndex < targetWord.Length)
            {
                inputText += targetWord.Substring(currentIndex);
            }
            displayInputText.text = inputText;
        }
    }

    // クリア時の処理
    void OnClear()
    {
        isFinished = true;
        Debug.Log("<color=blue>★★★ クリア！ ★★★</color>");
        // クリア演出、次のステージへ、などの処理
    }
}