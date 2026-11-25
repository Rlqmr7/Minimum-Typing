using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("警告表示設定（Consoleログ + UnityEvent）")]
    [Tooltip("左手が現れる何秒前に警告を出すか（X秒前）")]
    public float warningLeadTime = 1.5f;
    [Tooltip("Consoleに出す警告メッセージ")]
    public string warningMessage = "台パンがくるよ！";
    [Tooltip("台パン発動と同時に警告を終了するか")]
    public bool hideWarningOnImpact = true;
    [Tooltip("台パン発動で消さない場合、警告を自動で終わらせる秒数")]
    public float warningDisplayDuration = 1.5f;
    [Tooltip("警告が出たタイミングで呼ばれるイベント")]
    public UnityEvent onWarningTriggered;
    [Tooltip("警告が消えたタイミングで呼ばれるイベント")]
    public UnityEvent onWarningEnded;

    [Header("左手の出現・消滅設定")]
    [Tooltip("出現・消滅させる左手のオブジェクト（空ならこのGameObject自体を使う）")]
    public GameObject handVisualTarget;
    [Tooltip("台パンの何秒前に左手を出現させるか")]
    public float handAppearLeadTime = 1.0f;
    [Tooltip("台パン発動（ExecuteImpact）から何秒後に左手を消すか")]
    public float handHideDelay = 1.0f;
    [Tooltip("開始時に左手を非表示にしておくか")]
    public bool hideHandOnStart = true;

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
    private Coroutine handHideCoroutine;
    private Coroutine warningHideCoroutine;

    private Vector3 baseCameraLocalPos;
    private Vector3 baseHandLocalPos;
    private Vector3 baseColliderCenter;
    private bool isColliderCenterInitialized = false;
    private bool isHandSlamOffsetActive = false;
    private bool isWarningActive = false;
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

        if (hideHandOnStart)
        {
            SetHandVisible(false);
        }

        EndWarningImmediate();
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
        EndWarningImmediate();

        if (handHideCoroutine != null)
        {
            StopCoroutine(handHideCoroutine);
            handHideCoroutine = null;
        }
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
        float handLead = Mathf.Max(0f, handAppearLeadTime);
        float totalLeadBeforeImpact = handLead + Mathf.Max(0f, warningLeadTime);

        yield return StartCoroutine(WaitWithWarningAndHandAppear(startDelay, handLead, totalLeadBeforeImpact));

        while (true)
        {
            TriggerDaipanAnimation();

            float minWait = Mathf.Max(0.2f, Mathf.Min(minDaipanInterval, maxDaipanInterval));
            float maxWait = Mathf.Max(minWait, Mathf.Max(minDaipanInterval, maxDaipanInterval));
            float waitTime = Random.Range(minWait, maxWait);

            Debug.Log($"次の台パンまで {waitTime:F2} 秒待機");

            yield return StartCoroutine(WaitWithWarningAndHandAppear(waitTime, handLead, totalLeadBeforeImpact));
        }
    }

    private IEnumerator WaitWithWarningAndHandAppear(float interval, float handLead, float totalLeadBeforeImpact)
    {
        float clampedTotalLead = Mathf.Min(totalLeadBeforeImpact, Mathf.Max(0f, interval - 0.05f));
        float clampedHandLead = Mathf.Min(handLead, clampedTotalLead);
        float warningLead = Mathf.Max(0f, clampedTotalLead - clampedHandLead);

        float beforeWarningWait = Mathf.Max(0f, interval - clampedTotalLead);
        if (beforeWarningWait > 0f)
        {
            yield return new WaitForSeconds(beforeWarningWait);
        }

        if (warningLead > 0f)
        {
            TriggerWarning();
            yield return new WaitForSeconds(warningLead);
        }

        SetHandVisible(true);

        if (clampedHandLead > 0f)
        {
            yield return new WaitForSeconds(clampedHandLead);
        }
    }

    private void TriggerWarning()
    {
        if (warningHideCoroutine != null)
        {
            StopCoroutine(warningHideCoroutine);
            warningHideCoroutine = null;
        }

        isWarningActive = true;

        if (!string.IsNullOrEmpty(warningMessage))
        {
            Debug.Log(warningMessage);
        }

        onWarningTriggered?.Invoke();

        if (!hideWarningOnImpact && warningDisplayDuration > 0f)
        {
            warningHideCoroutine = StartCoroutine(EndWarningAfterDelayCoroutine(warningDisplayDuration));
        }
    }

    private IEnumerator EndWarningAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndWarningImmediate();
    }

    public void EndWarningImmediate()
    {
        if (warningHideCoroutine != null)
        {
            StopCoroutine(warningHideCoroutine);
            warningHideCoroutine = null;
        }

        if (isWarningActive)
        {
            isWarningActive = false;
            onWarningEnded?.Invoke();
        }
    }

    private void SetHandVisible(bool visible)
    {
        GameObject target = handVisualTarget != null ? handVisualTarget : gameObject;

        if (target == gameObject)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                r.enabled = visible;
            }
            return;
        }

        target.SetActive(visible);
    }

    private void ScheduleHideHand()
    {
        if (handHideCoroutine != null)
        {
            StopCoroutine(handHideCoroutine);
            handHideCoroutine = null;
        }

        if (handHideDelay <= 0f)
        {
            SetHandVisible(false);
            return;
        }

        handHideCoroutine = StartCoroutine(HideHandAfterDelayCoroutine());
    }

    private IEnumerator HideHandAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(handHideDelay);
        SetHandVisible(false);
        handHideCoroutine = null;
    }

    public void TriggerDaipanAnimation()
    {
        SetHandVisible(true);

        if (handAnimator != null && !string.IsNullOrEmpty(daipanTriggerName))
        {
            handAnimator.SetTrigger(daipanTriggerName);
        }
        else
        {
            OnDaipanImpact();
        }

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

        if (hideWarningOnImpact)
        {
            EndWarningImmediate();
        }

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

            player.StartStun(stunDuration);
        }

        ScheduleHideHand();
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