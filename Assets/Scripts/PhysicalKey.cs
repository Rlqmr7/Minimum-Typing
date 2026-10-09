using UnityEngine;

public class PhysicalKey : MonoBehaviour
{
    [Header("キーの設定")]
    public char keyValue; // このキーの文字（'A', 'B'など）
    public float pressThreshold = 0.15f; // 押し込まれたと判定するY座標の閾値（初期位置からの差）

    private ConfigurableJoint joint;
    private Vector3 startLocalPosition;
    private bool isPressed = false;

    // タイピング管理クラスへの参照（後述）
    private TypingManager typingManager;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        startLocalPosition = transform.localPosition;

        // シーン内のTypingManagerを探す
        typingManager = Object.FindFirstObjectByType<TypingManager>();

        if (typingManager == null)
        {
            Debug.LogError("TypingManagerがシーンに見つかりません。");
        }
    }

    void Update()
    {
        // 初期位置からの沈み込み量を計算
        float currentDepression = startLocalPosition.y - transform.localPosition.y;

        // 閾値を超えて押し込まれたか、かつまだ「押された」状態でない場合
        if (currentDepression >= pressThreshold && !isPressed)
        {
            isPressed = true;
            OnKeyPress();
        }
        // 閾値を下回って戻った場合
        else if (currentDepression < pressThreshold * 0.8f && isPressed) // 80%まで戻ったらリセット
        {
            isPressed = false;
        }
    }

    // キーが押された時の処理
    void OnKeyPress()
    {
        Debug.Log($"キー '{keyValue}' が踏まれました！");

        // タイピング管理クラスに文字を送る
        if (typingManager != null)
        {
            typingManager.OnKeyTargeted(keyValue);
        }

        // ここで音を鳴らす、エフェクトを出すなどのフィードバックを行う
        // GetComponent<AudioSource>()?.Play();
    }
}
