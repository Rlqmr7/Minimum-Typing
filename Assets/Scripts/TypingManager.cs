using UnityEngine;
using TMPro;
using System.Collections.Generic; // Listを使うために必要

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI; // モニターの文字を表示しているTextMeshPro

    [Header("ゲーム設定（複数の単語リスト）")]
    public List<string> targetWords = new List<string> { "ASD", "UNITY", "C#, SCRIPT" }; // インスペクターから自由に追加・変更可能
    private int currentWordIndex = 0;   // 現在どの単語をタイピングしているか
    private string currentTargetWord;   // 現在の正解ワード

    private int nextCharIndex = 0;        // 次に入力すべき文字の番号

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadNextWord();
    }

    // 次の単語をセットして初期化する
    void LoadNextWord()
    {
        if (targetWords == null || targetWords.Count == 0)
        {
            Debug.LogWarning("タイピング用の単語が登録されていません！");
            return;
        }

        // リストの範囲内で現在の単語を決定
        if (currentWordIndex < targetWords.Count)
        {
            currentTargetWord = targetWords[currentWordIndex];
            nextCharIndex = 0;
            UpdateUI();
        }
        else
        {
            Debug.Log("すべての単語をクリアしました！ゲームクリア！");
            if (typedTextUI != null)
            {
                typedTextUI.text = "<color=green>CLEAR!</color>";
            }
        }
    }

    // キーが踏まれて文字が渡されたとき
    public void AppendChar(char c)
    {
        // すでに全単語クリアしていたら何もしない
        if (currentWordIndex >= targetWords.Count) return;

        // 次に入力すべき文字と一致しているかチェック（大文字小文字を無視）
        if (char.ToUpper(c) == char.ToUpper(currentTargetWord[nextCharIndex]))
        {
            // 正解！次の文字へ進む
            nextCharIndex++;
            Debug.Log($"正解！進捗: {nextCharIndex}/{currentTargetWord.Length}");

            UpdateUI();

            // 現在の単語を全部クリアしたかチェック
            if (nextCharIndex >= currentTargetWord.Length)
            {
                Debug.Log($"単語「{currentTargetWord}」をクリアしました！");
                currentWordIndex++; // 次の単語へインデックスを進める

                // 少し遅らせて次の単語を読み込む（必要に応じてコルーチン等でもOKですが、即時切り替えます）
                LoadNextWord();
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
        if (typedTextUI != null && currentWordIndex < targetWords.Count)
        {
            if (nextCharIndex > 0)
            {
                // 入力済みの部分を黄色（<color=yellow>）、残りを白（<color=white>）にする
                string typedPart = "<color=yellow>" + currentTargetWord.Substring(0, nextCharIndex) + "</color>";
                string untypedPart = "<color=white>" + currentTargetWord.Substring(nextCharIndex) + "</color>";

                typedTextUI.text = typedPart + untypedPart;
            }
            else
            {
                // 最初は全部白文字
                typedTextUI.text = "<color=white>" + currentTargetWord + "</color>";
            }
        }
    }

    // リセット用
    public void ResetTyping()
    {
        currentWordIndex = 0;
        LoadNextWord();
    }
}