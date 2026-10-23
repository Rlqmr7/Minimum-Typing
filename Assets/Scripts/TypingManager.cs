using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI; // モニターの文字を表示しているTextMeshPro

    [Header("ゲーム設定（出題する単語プール）")]
    public List<string> targetWordsPool = new List<string> { "ASD", "UNITY", "C#", "GAME", "PLAYER" };

    private List<string> remainingWords = new List<string>();
    private string currentTargetWord;
    private int nextCharIndex = 0;

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

    void ResetAndShufflePool()
    {
        remainingWords = new List<string>(targetWordsPool);

        for (int i = 0; i < remainingWords.Count; i++)
        {
            string temp = remainingWords[i];
            int randomIndex = Random.Range(i, remainingWords.Count);
            remainingWords[i] = remainingWords[randomIndex];
            remainingWords[randomIndex] = temp;
        }
    }

    void LoadNextWord()
    {
        if (remainingWords.Count == 0)
        {
            Debug.Log("すべての問題をクリアしました！新しくシャッフルしてループします。");
            ResetAndShufflePool();
        }

        currentTargetWord = remainingWords[0];
        remainingWords.RemoveAt(0);

        nextCharIndex = 0;
        UpdateUI();
        Debug.Log($"次の問題: {currentTargetWord}");
    }

    public void AppendChar(char c)
    {
        if (string.IsNullOrEmpty(currentTargetWord)) return;

        if (char.ToUpper(c) == char.ToUpper(currentTargetWord[nextCharIndex]))
        {
            nextCharIndex++;
            UpdateUI();

            // 単語をクリアしたかチェック
            if (nextCharIndex >= currentTargetWord.Length)
            {
                Debug.Log($"単語「{currentTargetWord}」をクリア！");

                // --- 独立したScoreManagerにスコア加算を頼む ---
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(10);
                }

                LoadNextWord();
            }
        }
        else
        {
            Debug.Log($"不正解のキーです ('{c}')");
        }
    }

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

    public void ResetTyping()
    {
        ResetAndShufflePool();
        LoadNextWord();
    }
}