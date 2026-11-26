using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanPhysicalButton : MonoBehaviour
{
    [Header("操作対象の扇風機")]
    public FanZone targetFan;

    [Header("ボタンの押し込み演出")]
    [Tooltip("沈み込むボタンの可動パーツ（未指定ならこのオブジェクト自身）")]
    public Transform buttonVisual;
    public float pressDepth = 0.08f;
    public float pressSpeed = 12f;

    [Header("押し方とクールタイム")]
    [Tooltip("ヒップドロップ着地で押すか、通常接触・着地でも押すか")]
    public bool requireHipDrop = false;
    public float cooldown = 0.4f;

    [Header("自動復帰（任意）")]
    [Tooltip("0より大きい場合、止めたあとに指定秒数で自動で扇風機を再開します")]
    public float autoRestartSeconds = 0f;

    [Header("演出（任意）")]
    public Renderer indicatorRenderer;
    public Color activeColor = Color.cyan;
    public Color stoppedColor = Color.red;
    public AudioSource audioSource;
    public AudioClip pressClip;

    private Vector3 initialLocalPos;
    private float cooldownTimer = 0f;
    private float restartTimer = 0f;
    private bool isPressedVisual = false;

    private void Awake()
    {
        if (buttonVisual == null)
        {
            buttonVisual = transform;
        }
        initialLocalPos = buttonVisual.localPosition;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (targetFan != null && !targetFan.IsFanActive && autoRestartSeconds > 0f)
        {
            restartTimer -= Time.deltaTime;
            if (restartTimer <= 0f)
            {
                targetFan.SetFanActive(true);
                UpdateVisualState();
            }
        }

        // ボタンの押し込み・復帰アニメーション
        Vector3 targetLocalPos = isPressedVisual
            ? initialLocalPos + Vector3.down * pressDepth
            : initialLocalPos;

        buttonVisual.localPosition = Vector3.Lerp(
            buttonVisual.localPosition,
            targetLocalPos,
            pressSpeed * Time.deltaTime
        );
    }

    private void Start()
    {
        UpdateVisualState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (cooldownTimer > 0f || targetFan == null) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null)
        {
            player = collision.gameObject.GetComponentInParent<PlayerController>();
        }
        if (player == null) return;

        // 上から踏んだ／着地したか確認（横からこすっただけの誤爆防止）
        bool hitFromAbove = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                hitFromAbove = true;
                break;
            }
        }
        if (!hitFromAbove) return;

        // ヒップドロップ必須設定の場合のチェック
        if (requireHipDrop && !player.IsHipDropping) return;

        TriggerButton();
    }

    public void TriggerButton()
    {
        if (targetFan == null) return;

        cooldownTimer = cooldown;
        targetFan.ToggleFan();

        restartTimer = !targetFan.IsFanActive ? autoRestartSeconds : 0f;

        if (audioSource != null && pressClip != null)
        {
            audioSource.PlayOneShot(pressClip);
        }

        UpdateVisualState();
        StopCoroutine(nameof(PressAnimationRoutine));
        StartCoroutine(nameof(PressAnimationRoutine));
    }

    private System.Collections.IEnumerator PressAnimationRoutine()
    {
        isPressedVisual = true;
        yield return new WaitForSeconds(0.15f);
        isPressedVisual = false;
    }

    private void UpdateVisualState()
    {
        if (targetFan == null) return;

        if (indicatorRenderer != null && indicatorRenderer.material != null)
        {
            indicatorRenderer.material.color = targetFan.IsFanActive ? activeColor : stoppedColor;
        }
    }
}