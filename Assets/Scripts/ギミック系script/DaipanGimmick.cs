using System.Collections;
using UnityEngine;

public class DaipanGimmick : MonoBehaviour
{
    [Header("カメラ揺れ設定")]
    public Transform cameraTransform;
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.15f;

    [Header("スタン設定")]
    public float stunDuration = 1.2f;

    private Coroutine shakeCoroutine;

    // 台パンの衝撃が来た瞬間に呼ぶメソッド
    public void OnDaipanImpact()
    {
        Debug.Log("OnDaipanImpact が呼ばれた！");
        // 1. カメラを揺らす
        if (cameraTransform != null)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(ShakeCamera());
        }

        // 2. 全プレイヤーの接地状態を見て「回避」か「行動不能」を判定
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player == null) continue;

            // ジャンプして浮いているなら回避成功
            if (!player.IsGrounded)
            {
                Debug.Log($"{player.name}：台パンをジャンプ回避！");
                continue;
            }

            // 地上にいるなら行動不能にする
            player.StartStun(stunDuration);
        }
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}