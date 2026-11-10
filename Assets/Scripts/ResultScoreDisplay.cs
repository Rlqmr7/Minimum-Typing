using UnityEngine;
using TMPro;

public class ResultScoreDisplay : MonoBehaviour
{
    [Header("スコア表示用テキスト")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // "PlayerScore" というキーで保存されたスコアを読み込む（保存されていなければ 0）
        int score = PlayerPrefs.GetInt("PlayerScore", 0);

        // テキストに反映する
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score} 点";
        }
    }
}