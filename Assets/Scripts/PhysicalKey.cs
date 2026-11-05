using UnityEngine;
using System.Collections;

public class PhysicalKey : MonoBehaviour
{
    [Header("キーの設定")]
    public char keyValue = 'a';        // このキーが担当する文字（インスペクターでは小文字推奨）
    public Transform visualMesh;       // 沈ませる見た目のオブジェクト（子オブジェクト）
    public float pressDepth = 0.1f;    // 沈む深さ
    public float moveSpeed = 20f;      // 沈む・戻るスピード
    public float bounceVelocity = 8f;  // 踏んだ後にプレイヤーを跳ね返す上方向の速度

    [Header("発光（マテリアル）設定")]
    public Renderer keyRenderer;       // 色を変えるRenderer（未指定なら自動取得）
    public Material normalMaterial;    // 通常時のマテリアル
    public Material emissionMaterial;  // 光っている時のマテリアル

    private Vector3 defaultLocalPos;
    private Vector3 pressedLocalPos;
    private bool isPressed = false;

    void Start()
    {
        if (visualMesh == null)
        {
            visualMesh = transform;
        }

        // Rendererが未設定なら自動で取得
        if (keyRenderer == null)
        {
            keyRenderer = visualMesh.GetComponent<Renderer>();
        }

        // 初期マテリアルを通常状態に設定
        if (keyRenderer != null && normalMaterial != null)
        {
            keyRenderer.material = normalMaterial;
        }

        defaultLocalPos = visualMesh.localPosition;
        pressedLocalPos = defaultLocalPos - new Vector3(0, pressDepth, 0);
    }

    // プレイヤーがキーに乗ったとき
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // 乗っている間は常に光らせる
            SetEmission(true);

            // 完全に「ヒップドロップ中」かつ「上から踏んだ場合」の判定
            if (player.IsHipDropping && collision.transform.position.y > transform.position.y + 0.05f)
            {
                PressKey();
                player.EndHipDrop();

                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, bounceVelocity, playerRb.linearVelocity.z);
                }
            }
        }
    }

    // プレイヤーがキーから離れたとき
    private void OnCollisionExit(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // 離れたら光を消す
            SetEmission(false);
        }
    }

    private void SetEmission(bool isOn)
    {
        if (keyRenderer != null)
        {
            // 光らせるマテリアルが設定されている場合のみ切り替え
            if (isOn && emissionMaterial != null)
            {
                keyRenderer.material = emissionMaterial;
            }
            else if (!isOn && normalMaterial != null)
            {
                keyRenderer.material = normalMaterial;
            }
        }
    }

    public void PressKey()
    {
        if (!isPressed)
        {
            StartCoroutine(PressRoutine());

            // ★ ShiftキーがONかどうかで大文字・小文字を動的に切り替える
            char charToSend = keyValue;
            if (ShiftKey.IsShiftPressed)
            {
                charToSend = char.ToUpper(keyValue);
                Debug.Log($"<color=magenta>【文字入力 (大文字)】 キー [{charToSend}] がヒップドロップで押されました！ (Shift: ON)</color>");
            }
            else
            {
                charToSend = char.ToLower(keyValue);
                Debug.Log($"<color=green>【文字入力 (小文字)】 キー [{charToSend}] がヒップドロップで押されました！ (Shift: OFF)</color>");
            }

            // TypingManager に送信してUIの判定を進める
            if (TypingManager.Instance != null)
            {
                TypingManager.Instance.AppendChar(charToSend);
            }
        }
    }

    IEnumerator PressRoutine()
    {
        isPressed = true;

        // 1. 沈み込み
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;
            visualMesh.localPosition = Vector3.Lerp(defaultLocalPos, pressedLocalPos, time);
            yield return null;
        }
        visualMesh.localPosition = pressedLocalPos;

        yield return new WaitForSeconds(0.02f);

        // 2. 元の位置に戻る
        time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;
            visualMesh.localPosition = Vector3.Lerp(pressedLocalPos, defaultLocalPos, time);
            yield return null;
        }
        visualMesh.localPosition = defaultLocalPos;

        isPressed = false;
    }
}