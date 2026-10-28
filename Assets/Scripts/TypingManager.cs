using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI typedTextUI;

    [System.Serializable]
    public struct WordData
    {
        [TextArea(2, 4)]
        public string text; // 1行目にローマ字、2行目に日本語を入力
    }

    [Header("ゲーム設定（出題する単語プール）")]
    public List<WordData> targetWordsPool = new List<WordData> {
        new WordData { text = "wareware\nわれわれ" },
        new WordData { text = "tare\nたれ" },
        new WordData { text = "sarada\nさらだ" },
        new WordData { text = "wareta\nわれた" },
        new WordData { text = "dareda\nだれだ" }
    };

    [Header("オーディオ設定（効果音）")]
    public AudioClip correctKeySound;
    public AudioClip incorrectKeySound;
    public AudioClip clearSound;
    private AudioSource audioSource;

    private List<WordData> remainingWords = new List<WordData>();
    private WordData currentWordData;

    private string typingTargetWord; // タイピング判定するローマ字部分
    private string displaySubText;   // 下に表示する日本語部分
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
        remainingWords = new List<WordData>(targetWordsPool);

        for (int i = 0; i < remainingWords.Count; i++)
        {
            WordData temp = remainingWords[i];
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

        if (remainingWords.Count > 0)
        {
            currentWordData = remainingWords[0];
            remainingWords.RemoveAt(0);

            // 改行区切りで「1行目（ローマ字）」と「2行目（日本語）」に分解する
            string[] lines = currentWordData.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 0)
            {
                typingTargetWord = lines[0].Trim(); // タイピング対象は1行目
            }
            else
            {
                typingTargetWord = "ERROR";
            }

            if (lines.Length > 1)
            {
                displaySubText = lines[1].Trim(); // 下の日本語部分
            }
            else
            {
                displaySubText = "";
            }
        }

        nextCharIndex = 0;
        UpdateUI();
    }

    public void AppendChar(char c)
    {
        if (string.IsNullOrEmpty(typingTargetWord)) return;

        // 正解の文字を入力した場合
        if (char.ToUpper(c) == char.ToUpper(typingTargetWord[nextCharIndex]))
        {
            if (correctKeySound != null) audioSource.PlayOneShot(correctKeySound);

            nextCharIndex++;
            UpdateUI();

            // ローマ字をすべて入力し終えた場合（クリア）
            if (nextCharIndex >= typingTargetWord.Length)
            {
                if (clearSound != null) audioSource.PlayOneShot(clearSound);

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScoreOnClear();
                }

                LoadNextWord();
            }
        }
        else
        {
            // --- 不正解の場合 ---
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
        if (typedTextUI != null && !string.IsNullOrEmpty(typingTargetWord))
        {
            string typedPart = "";
            string untypedPart = "";

            if (nextCharIndex > 0)
            {
                typedPart = "<color=yellow>" + typingTargetWord.Substring(0, nextCharIndex) + "</color>";
                untypedPart = "<color=white>" + typingTargetWord.Substring(nextCharIndex) + "</color>";
            }
            else
            {
                untypedPart = "<color=white>" + typingTargetWord + "</color>";
            }

            string line1 = typedPart + untypedPart;

            // 2行目（日本語）がある場合は改行して表示する
            if (!string.IsNullOrEmpty(displaySubText))
            {
                typedTextUI.text = line1 + "\n<color=white>" + displaySubText + "</color>";
            }
            else
            {
                typedTextUI.text = line1;
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