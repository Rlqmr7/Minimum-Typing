using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI scoreText;   // スコアを表示するTextMeshPro
    public TextMeshProUGUI comboText;   // コンボ数を表示するTextMeshPro（例: "3 コンボ"）

    [Header("スコア・コンボ設定")]
    private int currentScore = 0;
    private int comboCount = 0;         // 現在のコンボ数
    public int baseScorePerWord = 10;   // 1単語あたりの基本スコア

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }
    // 単語クリア時に呼ばれる（コンボを加算してスコアを増やす）
    public void AddScoreOnClear()
    {
        comboCount++; // コンボ増加

        // コンボ数に応じたスコア計算（例: 1コンボ目=10点, 2コンボ目=20点...）
        int earnedScore = baseScorePerWord * comboCount;
        currentScore += earnedScore;

        Debug.Log($"単語クリア！ {comboCount}コンボ！ 獲得スコア: {earnedScore} (合計: {currentScore})");
        UpdateUI();
    }

    // 不正解のときにコンボをリセットする
    public void ResetCombo()
    {
        if (comboCount > 0)
        {
            Debug.Log("コンボが途切れました…！");
        }
        comboCount = 0;
        UpdateUI();
    }

    // 全リセット（ゲーム開始時ややり直し用）
    public void ResetAll()
    {
        currentScore = 0;
        comboCount = 0;
        UpdateUI();
    }

    public int GetScore()
    {
        return currentScore;
    }

    // UIの表示更新
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア : {currentScore}";
        }

        if (comboText != null)
        {
            if (comboCount > 1)
            {
                comboText.text = $"{comboCount} コンボ";
            }
            else
            {
                comboText.text = ""; // 1コンボ以下なら非表示、または "0 コンボ" などお好みで
            }
        }
    }
}