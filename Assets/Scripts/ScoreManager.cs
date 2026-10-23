using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI scoreText; // スコアを表示するTextMeshPro

    private int currentScore = 0;

    void Awake()
    {
        // シングルトン化してどこからでもアクセスできるようにする
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
    }

    // スコアを加算するメソッド
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    // スコアをリセットするメソッド
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    // 現在のスコアを取得するメソッド（リザルト画面等で利用）
    public int GetScore()
    {
        return currentScore;
    }

    // スコアUIの表示更新
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア : {currentScore}";
        }
    }
}