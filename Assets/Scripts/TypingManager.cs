using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI; // モニターの文字を表示しているTextMeshPro

    [Header("ゲーム設定（出題する単語プール）")]
    public List<string> targetWordsPool = new List<string> { "ASD", "UNITY", "C#", "GAME", "PLAYER" }; // 好きなだけ追加可能

    private List<string> remainingWords = new List<string>(); // まだ出題されていない単語のリスト
    private string currentTargetWord;   // 現在の正解ワード
    private int nextCharIndex = 0;        // 次に入力すべき文字の番号

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetAndShufflePool();
        LoadNextWord();
    }

    // 問題プールをリセットしてシャッフルする
    void ResetAndShufflePool()
    {
        remainingWords = new List<string>(targetWordsPool);

        // リストをランダムに並び替える（Fisher-Yatesアルゴリズム）
        for (int i = 0; i < remainingWords.Count; i++)
        {
            string temp = remainingWords[i];
            int randomIndex = Random.Range(i, remainingWords.Count);
            remainingWords[i] = remainingWords[randomIndex];
            remainingWords[randomIndex] = temp;
        }
    }

    // 次の単語をランダム（シャッフル済みリストから順に）取得する
    void LoadNextWord()
    {
        // もし全部使い切ったら、再度プールを補充してシャッフルする
        if (remainingWords.Count == 0)
        {
            Debug.Log("すべての問題をクリアしました！新しくシャッフルしてループします。");
            ResetAndShufflePool();
        }

        // リストの先頭から1つ取り出して削除する
        currentTargetWord = remainingWords[0];
        remainingWords.RemoveAt(0);

        nextCharIndex = 0;
        UpdateUI();
        Debug.Log($"次の問題: {currentTargetWord}");
    }

    // キーが踏まれて文字が渡されたとき
    public void AppendChar(char c)
    {
        if (string.IsNullOrEmpty(currentTargetWord)) return;

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
                LoadNextWord(); // 次の問題へ
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
        if (typedTextUI != null && !string.IsNullOrEmpty(currentTargetWord))
        {
            if (nextCharIndex > 0)
            {
                string typedPart = "<color=yellow>" + currentTargetWord.Substring(0, nextCharIndex) + "</color>";
                string untypedPart = "<color=white>" + currentTargetWord.Substring(nextCharIndex) + "</color>";

                typedTextUI.text = typedPart + untypedPart;
            }
            else
            {
                typedTextUI.text = "<color=white>" + currentTargetWord + "</color>";
            }
        }
    }

    // リセット用
    public void ResetTyping()
    {
        ResetAndShufflePool();
        LoadNextWord();
    }
}