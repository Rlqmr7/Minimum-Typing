using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI設定")]
    public TextMeshProUGUI scoreText;        // スコアを表示するTextMeshPro
    public TextMeshProUGUI comboText;        // コンボ数を表示するTextMeshPro（例: "3 コンボ"）
    [Tooltip("スコア加算時に拡大させる対象（空ならscoreTextを使う。バッジ全体も可）")]
    public RectTransform scorePunchTarget;
    [Tooltip("コンボ更新時に拡大させる対象（空ならcomboTextを使う）")]
    public RectTransform comboPunchTarget;

    [Header("スコア・コンボ設定")]
    private int currentScore = 0;
    private int comboCount = 0;              // 現在のコンボ数
    public int baseScorePerWord = 10;        // 1単語あたりの基本スコア

    [Header("ポップ演出設定")]
    [SerializeField] private float scorePunchScale = 1.22f;
    [SerializeField] private float scorePunchDuration = 0.16f;
    [SerializeField] private float comboPunchScale = 1.28f;
    [SerializeField] private float comboPunchDuration = 0.18f;

    private Coroutine scorePunchCoroutine;
    private Coroutine comboPunchCoroutine;
    private Vector3 baseScoreScale = Vector3.one;
    private Vector3 baseComboScale = Vector3.one;
    private bool hasScoreBaseScale = false;
    private bool hasComboBaseScale = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitPunchTargets();
    }

    private void InitPunchTargets()
    {
        if (scorePunchTarget == null && scoreText != null)
        {
            scorePunchTarget = scoreText.rectTransform;
        }

        if (scorePunchTarget != null)
        {
            baseScoreScale = scorePunchTarget.localScale;
            hasScoreBaseScale = true;
        }

        if (comboPunchTarget == null && comboText != null)
        {
            comboPunchTarget = comboText.rectTransform;
        }

        if (comboPunchTarget != null)
        {
            baseComboScale = comboPunchTarget.localScale;
            hasComboBaseScale = true;
        }
    }

    void Start()
    {
        UpdateUI(animateScore: false, animateCombo: false);
    }

    // 単語クリア時に呼ばれる（コンボを加算してスコアを増やす）
    public void AddScoreOnClear()
    {
        comboCount++; // コンボ増加

        // コンボ数に応じたスコア計算（例: 1コンボ目=10点, 2コンボ目=20点...）
        int earnedScore = baseScorePerWord * comboCount;
        currentScore += earnedScore;

        Debug.Log($"単語クリア！ {comboCount}コンボ！ 獲得スコア: {earnedScore} (合計: {currentScore})");
        UpdateUI(animateScore: true, animateCombo: comboCount > 1);
    }

    // 不正解のときにコンボをリセットする
    public void ResetCombo()
    {
        if (comboCount > 0)
        {
            Debug.Log("コンボが途切れました…！");
        }
        comboCount = 0;
        UpdateUI(animateScore: false, animateCombo: false);
    }

    // 全リセット（ゲーム開始時ややり直し用）
    public void ResetAll()
    {
        currentScore = 0;
        comboCount = 0;
        UpdateUI(animateScore: false, animateCombo: false);
    }

    public int GetScore()
    {
        return currentScore;
    }

    // UIの表示更新
    void UpdateUI(bool animateScore = false, bool animateCombo = false)
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
                comboText.text = ""; // 1コンボ以下なら非表示
            }
        }

        if (animateScore)
        {
            PlayPunch(ref scorePunchCoroutine, scorePunchTarget, baseScoreScale, hasScoreBaseScale, scorePunchScale, scorePunchDuration);
        }

        if (animateCombo)
        {
            PlayPunch(ref comboPunchCoroutine, comboPunchTarget, baseComboScale, hasComboBaseScale, comboPunchScale, comboPunchDuration);
        }
    }

    private void PlayPunch(
        ref Coroutine activeCoroutine,
        RectTransform target,
        Vector3 baseScale,
        bool hasBase,
        float punchScale,
        float duration)
    {
        if (target == null || !hasBase) return;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(PunchCoroutine(target, baseScale, punchScale, duration));
    }

    private IEnumerator PunchCoroutine(RectTransform target, Vector3 baseScale, float punchScale, float duration)
    {
        target.localScale = baseScale;
        Vector3 peakScale = baseScale * punchScale;

        float half = Mathf.Max(0.01f, duration * 0.5f);
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float easeOut = 1f - Mathf.Pow(1f - t, 2f);
            target.localScale = Vector3.Lerp(baseScale, peakScale, easeOut);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float easeIn = t * t;
            target.localScale = Vector3.Lerp(peakScale, baseScale, easeIn);
            yield return null;
        }

        target.localScale = baseScale;
    }
}