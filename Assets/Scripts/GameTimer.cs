using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class GameTimer : MonoBehaviour
{
    [Header("UI設定")]
    public TextMeshProUGUI timerText; // 左上の残り時間を表示するTextMeshPro

    [Header("タイマー設定")]
    public float timeLimit = 180f;    // 3分 = 180秒
    private float currentTime;
    private bool isPlaying = true;

    void Start()
    {
        currentTime = timeLimit;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isPlaying) return;

        // 時間を減らす
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isPlaying = false;
                OnTimeUp();
            }
            UpdateTimerUI();
        }
    }

    // UIの表示を更新するメソッド
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 分と秒を計算
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            if (minutes > 0)
            {
                timerText.text = $"残り {minutes}m {seconds:00}s";
            }
            else
            {
                timerText.text = $"残り {seconds}s";
            }
        }
    }

    // 時間切れになったときの処理
    void OnTimeUp()
    {
        Debug.Log("タイムアップ！リザルト画面に移動します");

        // --- ScoreManagerから現在のスコアを取得して保存する ---
        int finalScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.GetScore();
        }

        PlayerPrefs.SetInt("PlayerScore", finalScore);
        PlayerPrefs.Save(); // 保存を確定
        // --------------------------------------------------

        // リザルト画面（ResultScene）へシーン遷移
        SceneManager.LoadScene("ResultScene");
    }

    // 外からタイマーをリスタートさせたい時用
    public void ResetTimer()
    {
        currentTime = timeLimit;
        isPlaying = true;
    }
}