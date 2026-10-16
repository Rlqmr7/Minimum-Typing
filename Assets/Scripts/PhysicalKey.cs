using UnityEngine;
using System.Collections;

public class PhysicalKey : MonoBehaviour
{
    [Header("キーの設定")]
    public char keyValue = 'A';
    public Transform visualMesh;       // 沈ませる見た目のオブジェクト（子オブジェクト）
    public float pressDepth = 0.1f;    // 沈む深さ
    public float moveSpeed = 20f;      // 沈む・戻るスピード
    public float bounceVelocity = 8f;  // 踏んだ後にプレイヤーを跳ね返す上方向の速度

    private Vector3 defaultLocalPos;
    private Vector3 pressedLocalPos;
    private bool isPressed = false;

    void Start()
    {
        if (visualMesh == null)
        {
            visualMesh = transform;
        }

        defaultLocalPos = visualMesh.localPosition;
        pressedLocalPos = defaultLocalPos - new Vector3(0, pressDepth, 0);
    }

    // 「通常の物理衝突（OnCollisionEnter）」を使うことで、キーの上に乗ることができます
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // 完全に「ヒップドロップ中」かつ「上から踏んだ場合」のみ反応
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

    public void PressKey()
    {
        if (!isPressed)
        {
            StartCoroutine(PressRoutine());
            Debug.Log($"キー [{keyValue}] がヒップドロップで押されました！");

            if (TypingManager.Instance != null)
            {
                TypingManager.Instance.AppendChar(keyValue);
            }
        }
    }

    IEnumerator PressRoutine()
    {
        isPressed = true;

        // 1. 沈み込み（見た目のメッシュだけが動くので、親のコライダーはビクともせずスタックしない）
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