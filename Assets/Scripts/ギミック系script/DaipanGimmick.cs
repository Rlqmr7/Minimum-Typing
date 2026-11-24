using System.Collections;
using UnityEngine;

public class DaipanGimmick : MonoBehaviour
{
    [Header("自動台パンループ設定")]
    [Tooltip("一定間隔で台パンを自動で繰り返すか")]
    public bool autoRepeatDaipan = true;
    [Tooltip("次の台パンまでの最小間隔（秒）")]
    public float minDaipanInterval = 10.0f;
    [Tooltip("次の台パンまでの最大間隔（秒）")]
    public float maxDaipanInterval = 15.0f;
    [Tooltip("開始してから最初の台パンまでの待機時間（秒）")]
    public float startDelay = 2.0f;
    [Tooltip("台パンアニメーションを再生するAnimator")]
    public Animator handAnimator;
    [Tooltip("Animatorに設定したTrigger名（空なら直接OnDaipanImpactを呼ぶ）")]
    public string daipanTriggerName = "Daipan";

    [Header("BoxCollider 遅延移動設定")]
    [Tooltip("動かす左手のBoxCollider（空ならこのオブジェクトから自動取得）")]
    public BoxCollider handBoxCollider;
    [Tooltip("アニメーション開始からBoxColliderを下げるまでの秒数（X秒）")]
    public float colliderDropDelay = 0.2f;
    [Tooltip("BoxCollider.center をずらす量（下に下げるならYをマイナスに）")]
    public Vector3 colliderDropOffset = new Vector3(0f, -0.15f, 0f);
    [Tooltip("下げたBoxColliderを維持する時間（秒。0以下なら戻さない）")]
    public float colliderDropDuration = 0.25f;

    [Header("台パン直後の位置ずらし設定")]
    [Tooltip("台パン直後にずらすローカル座標のオフセット（下にずらすならYをマイナスに）")]
    public Vector3 slamOffset = new Vector3(0f, -0.08f, 0f);
    [Tooltip("ずらした位置を維持する時間（秒）")]
    public float slamHoldDuration = 0.15f;

    [Header("カメラ揺れ設定")]
    public Transform cameraTransform;
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.15f;
    [Tooltip("揺れの減衰を有効にするか（後半にかけて自然に収まる）")]
    public bool damping = true;

    [Header("遅延設定")]
    [Tooltip("接触・呼び出しから衝撃（カメラ揺れ・スタン）が発生するまでの秒数")]
    public float impactDelay = 0.0f;

    [Header("接触検知設定（机にColliderを使う場合）")]
    [Tooltip("コライダー接触でも衝撃を発動させるか")]
    public bool triggerOnDeskContact = true;
    [Tooltip("机オブジェクトに設定するTag（空ならどのColliderでも反応）")]
    public string deskTag = "Desk";
    [Tooltip("連続ヒット防止のクールタイム（秒）")]
    public float impactCooldown = 0.2f;

    [Header("スタン設定")]
    public float stunDuration = 1.2f;

    private Coroutine shakeCoroutine;
    private Coroutine delayCoroutine;
    private Coroutine repeatCoroutine;
    private Coroutine slamPosCoroutine;
    private Coroutine colliderMoveCoroutine;

    private Vector3 baseCameraLocalPos;
    private Vector3 baseHandLocalPos;
    private Vector3 baseColliderCenter;
    private bool isColliderCenterInitialized = false;
    private bool isHandSlamOffsetActive = false;
    private float lastImpactTime = -999f;

    private void Awake()
    {
        if (cameraTransform != null)
        {
            baseCameraLocalPos = cameraTransform.localPosition;
        }

        baseHandLocalPos = transform.localPosition;

        if (handAnimator == null)
        {
            handAnimator = GetComponent<Animator>();
        }

        if (handBoxCollider == null)
        {
            handBoxCollider = GetComponent<BoxCollider>();
        }

        if (handBoxCollider != null)
        {
            baseColliderCenter = handBoxCollider.center;
            isColliderCenterInitialized = true;
        }
    }

    private void OnEnable()
    {
        if (autoRepeatDaipan)
        {
            StartAutoDaipanLoop();
        }
    }

    private void OnDisable()
    {
        StopAutoDaipanLoop();
        RestoreHandPositionImmediately();
        RestoreColliderCenterImmediately();
    }

    public void StartAutoDaipanLoop()
    {
        StopAutoDaipanLoop();
        repeatCoroutine = StartCoroutine(AutoDaipanCoroutine());
    }

    public void StopAutoDaipanLoop()
    {
        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }

    private IEnumerator AutoDaipanCoroutine()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (true)
        {
            TriggerDaipanAnimation();

            float minWait = Mathf.Max(0.1f, Mathf.Min(minDaipanInterval, maxDaipanInterval));
            float maxWait = Mathf.Max(minWait, Mathf.Max(minDaipanInterval, maxDaipanInterval));
            float waitTime = Random.Range(minWait, maxWait);

            Debug.Log($"次の台パンまで {waitTime:F2} 秒待機");
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void TriggerDaipanAnimation()
    {
        if (handAnimator != null && !string.IsNullOrEmpty(daipanTriggerName))
        {
            handAnimator.SetTrigger(daipanTriggerName);
        }
        else
        {
            OnDaipanImpact();
        }

        // アニメーション開始から X 秒後に BoxCollider を下げる
        StartDelayedColliderDrop();
    }

    private void StartDelayedColliderDrop()
    {
        if (handBoxCollider == null) return;

        if (!isColliderCenterInitialized)
        {
            baseColliderCenter = handBoxCollider.center;
            isColliderCenterInitialized = true;
        }

        if (colliderMoveCoroutine != null)
        {
            StopCoroutine(colliderMoveCoroutine);
            handBoxCollider.center = baseColliderCenter;
        }

        colliderMoveCoroutine = StartCoroutine(DelayedColliderDropCoroutine());
    }

    private IEnumerator DelayedColliderDropCoroutine()
    {
        if (colliderDropDelay > 0f)
        {
            yield return new WaitForSeconds(colliderDropDelay);
        }

        handBoxCollider.center = baseColliderCenter + colliderDropOffset;
        Debug.Log($"BoxColliderを下げた！現在のcenter: {handBoxCollider.center}");

        if (colliderDropDuration > 0f)
        {
            yield return new WaitForSeconds(colliderDropDuration);
            handBoxCollider.center = baseColliderCenter;
            Debug.Log($"BoxColliderを戻した！現在のcenter: {handBoxCollider.center}");
        }

        colliderMoveCoroutine = null;
    }

    private void RestoreColliderCenterImmediately()
    {
        if (colliderMoveCoroutine != null)
        {
            StopCoroutine(colliderMoveCoroutine);
            colliderMoveCoroutine = null;
        }

        if (handBoxCollider != null && isColliderCenterInitialized)
        {
            handBoxCollider.center = baseColliderCenter;
        }
    }

    public void OnDaipanImpact()
    {
        if (Time.time - lastImpactTime < impactCooldown)
        {
            return;
        }
        lastImpactTime = Time.time;

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        if (impactDelay <= 0f)
        {
            ExecuteImpact();
        }
        else
        {
            delayCoroutine = StartCoroutine(DelayedImpactCoroutine());
        }
    }

    private IEnumerator DelayedImpactCoroutine()
    {
        yield return new WaitForSeconds(impactDelay);
        delayCoroutine = null;
        ExecuteImpact();
    }

    private void ExecuteImpact()
    {
        Debug.Log($"台パン衝撃発動！（遅延: {impactDelay}秒）");

        StartSlamOffset();

        if (cameraTransform != null)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                cameraTransform.localPosition = baseCameraLocalPos;
            }
            shakeCoroutine = StartCoroutine(ShakeCamera());
        }

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player == null) continue;

            if (!player.IsGrounded)
            {
                Debug.Log($"{player.name}：台パンをジャンプ回避！");
                continue;
            }

            // player.StartStun(stunDuration);
        }
    }

    private void StartSlamOffset()
    {
        if (slamPosCoroutine != null)
        {
            StopCoroutine(slamPosCoroutine);
        }

        if (!isHandSlamOffsetActive)
        {
            baseHandLocalPos = transform.localPosition;
        }

        slamPosCoroutine = StartCoroutine(SlamOffsetCoroutine());
    }

    private IEnumerator SlamOffsetCoroutine()
    {
        isHandSlamOffsetActive = true;
        transform.localPosition = baseHandLocalPos + slamOffset;

        if (slamHoldDuration > 0f)
        {
            yield return new WaitForSeconds(slamHoldDuration);
        }

        transform.localPosition = baseHandLocalPos;
        isHandSlamOffsetActive = false;
        slamPosCoroutine = null;
    }

    private void RestoreHandPositionImmediately()
    {
        if (slamPosCoroutine != null)
        {
            StopCoroutine(slamPosCoroutine);
            slamPosCoroutine = null;
        }

        if (isHandSlamOffsetActive)
        {
            transform.localPosition = baseHandLocalPos;
            isHandSlamOffsetActive = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnDeskContact) return;
        if (IsDeskObject(other.gameObject))
        {
            OnDaipanImpact();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!triggerOnDeskContact) return;
        if (IsDeskObject(collision.gameObject))
        {
            OnDaipanImpact();
        }
    }

    private bool IsDeskObject(GameObject target)
    {
        if (string.IsNullOrEmpty(deskTag))
        {
            return true;
        }
        return target.CompareTag(deskTag);
    }

    private IEnumerator ShakeCamera()
    {
        baseCameraLocalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;
            float currentMagnitude = damping ? shakeMagnitude * (1f - progress) : shakeMagnitude;

            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;

            cameraTransform.localPosition = baseCameraLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = baseCameraLocalPos;
        shakeCoroutine = null;
    }
}