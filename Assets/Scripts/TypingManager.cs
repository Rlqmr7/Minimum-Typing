using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI;

    [Header("ゲーム設定（出題する単語プール）")]
    public List<string> targetWordsPool = new List<string> { "ASD", "UNITY", "C#", "GAME", "PLAYER" };

    [Header("オーディオ設定（効果音）")]
    public AudioClip correctKeySound;
    public AudioClip incorrectKeySound;
    public AudioClip clearSound;
    private AudioSource audioSource;

    private List<string> remainingWords = new List<string>();
    private string currentTargetWord;
    private int nextCharIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
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
            ResetAndShufflePool();
        }

        currentTargetWord = remainingWords[0];
        remainingWords.RemoveAt(0);

        nextCharIndex = 0;
        UpdateUI();
    }

    public void AppendChar(char c)
    {
        if (string.IsNullOrEmpty(currentTargetWord)) return;

        // 正解の文字を入力した場合
        if (char.ToUpper(c) == char.ToUpper(currentTargetWord[nextCharIndex]))
        {
            if (correctKeySound != null) audioSource.PlayOneShot(correctKeySound);

            nextCharIndex++;
            UpdateUI();

            // 単語をすべてクリアした場合
            if (nextCharIndex >= currentTargetWord.Length)
            {
                if (clearSound != null) audioSource.PlayOneShot(clearSound);

                // --- スコアマネージャーにクリアを通知（コンボ加算＆スコアアップ） ---
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScoreOnClear();
                }

                LoadNextWord();
            }
        }
        else
        {
            // --- 不正解の場合：効果音を鳴らしてコンボをリセットする ---
            if (incorrectKeySound != null) audioSource.PlayOneShot(incorrectKeySound);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetCombo();
            }

            Debug.Log($"不正解のキーです ('{c}') - コンボリセット");
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
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetAll();
        }
        ResetAndShufflePool();
        LoadNextWord();
    }
}