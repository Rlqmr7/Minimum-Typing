using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI設定")]
    public TextMeshProUGUI timerText; // 左上の「残り30秒」などを表示するTextMeshPro

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
            // 分と秒を計算（例: 2分15秒）
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            // 「残り 2分15秒」または「残り 135秒」の形式で表示
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
        Debug.Log("タイムアップ！ゲーム終了");
        // TODO: ゲームオーバー画面の表示や、タイピング入力をストップする処理をここに書きます
    }

    // 外からタイマーをリスタートさせたい時用
    public void ResetTimer()
    {
        currentTime = timeLimit;
        isPlaying = true;
    }
}