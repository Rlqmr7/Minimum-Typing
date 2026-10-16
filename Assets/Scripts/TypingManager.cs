using UnityEngine;
using TMPro;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI; // モニターの文字を表示しているTextMeshPro

    [Header("ゲーム設定")]
    public string targetWord = "ASD";       // 画像に合わせた正解ワード[cite: 1]

    private int nextCharIndex = 0;        // 次に入力すべき文字の番号（0番目、1番目…）

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    // キーが踏まれて文字が渡されたとき
    public void AppendChar(char c)
    {
        // すでに全部クリアしていたら何もしない
        if (nextCharIndex >= targetWord.Length) return;

        // 次に入力すべき文字と一致しているかチェック（大文字小文字を無視）
        if (char.ToUpper(c) == char.ToUpper(targetWord[nextCharIndex]))
        {
            // 正解！次の文字へ進む
            nextCharIndex++;
            Debug.Log($"正解！進捗: {nextCharIndex}/{targetWord.Length}");

            UpdateUI();

            // 全部クリアしたかチェック
            if (nextCharIndex >= targetWord.Length)
            {
                Debug.Log("すべての文字の入力に成功しました！");
            }
        }
        else
        {
            Debug.Log($"不正解のキーです ('{c}')");
        }
    }

    // 画面の表示を更新（一文字ずつ色を変える）
    void UpdateUI()
    {
        if (typedTextUI != null)
        {
            if (nextCharIndex > 0)
            {
                // 入力済みの部分を黄色（<color=yellow>）、残りを白（<color=white>）にする
                string typedPart = "<color=yellow>" + targetWord.Substring(0, nextCharIndex) + "</color>";
                string untypedPart = "<color=white>" + targetWord.Substring(nextCharIndex) + "</color>";

                typedTextUI.text = typedPart + untypedPart;
            }
            else
            {
                // 最初は全部白文字
                typedTextUI.text = "<color=white>" + targetWord + "</color>";
            }
        }
    }

    // リセット用
    public void ResetTyping()
    {
        nextCharIndex = 0;
        UpdateUI();
    }
}