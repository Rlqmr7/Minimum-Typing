using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    private Rigidbody rb;
    private CapsuleCollider col;

    [Header("移動設定")]
    public float moveForce = 35f;
    public float maxSpeed = 7f;
    public float dashSpeedMultiplier = 1.8f;

    [Header("ジャンプ・ヒップドロップ設定")]
    public float jumpForce = 8f;
    public float hipDropForce = 50f;
    public float hipDropRecoveryTime = 1.0f; // ★ ヒップドロップ後の硬直時間（秒）
    private float recoveryTimer = 0f;          // ★ 硬直の残り時間

    [Header("滞空時間調整（落下・重力強化）")]
    public float gravityMultiplier = 2.5f;

    private bool isGrounded;
    private bool isHipDropping = false;

    public bool IsHipDropping => isHipDropping;
    // ★ 現在硬直中かどうかを確認できるプロパティ
    public bool IsRecovering => recoveryTimer > 0f;

    [Header("エフェクト設定")]
    public ParticleSystem dustEffect;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;

        if (dustEffect == null)
        {
            dustEffect = GetComponentInChildren<ParticleSystem>();
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // ★ 硬直中のタイマーを進める
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
        }

        // --- 接地判定 ---
        float rayDistance = col.height / 2f + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance);

        Debug.DrawRay(transform.position, Vector3.down * rayDistance, isGrounded ? Color.red : Color.blue);

        // --- 通常の地面に着地した場合のヒップドロップ終了 ---
        if (isHipDropping && isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
            {
                if (hit.collider.GetComponent<PhysicalKey>() == null)
                {
                    EndHipDropProcess(); // ★ 共通の終了処理にまとめる
                }
            }
        }

        // --- Aボタン（Jump / HipDrop）の入力判定 ---
        // ★ 硬直中（recoveryTimer > 0）はジャンプやヒップドロップを受け付けないようにする
        if (recoveryTimer <= 0f && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isHipDropping = false;
            }
            else if (!isHipDropping)
            {
                StartHipDrop();
            }
        }
    }

    void FixedUpdate()
    {
        // --- 滞空時間を短くするため、空中（非接地時）のときに余分に下方向の力を加える ---
        if (!isGrounded && !isHipDropping)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        // ★ ヒップドロップ中、または硬直中（1秒間の静止）は移動入力を反映させない
        if (isHipDropping || recoveryTimer > 0f) return;

        bool isDashing = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
        float currentMaxSpeed = isDashing ? maxSpeed * dashSpeedMultiplier : maxSpeed;

        Vector3 targetVelocity = new Vector3(moveInput.x * currentMaxSpeed, rb.linearVelocity.y, moveInput.y * currentMaxSpeed);
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void StartHipDrop()
    {
        isHipDropping = true;
        recoveryTimer = 0f; // 開始時は硬直をリセット
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
        Debug.Log("ヒップドロップ！");
    }

    public void EndHipDrop()
    {
        if (isHipDropping)
        {
            EndHipDropProcess();
            Debug.Log("キーを踏んでヒップドロップ終了＆砂煙再生！");
        }
    }

    // ★ ヒップドロップ終了時の処理をまとめたヘルパーメソッド
    private void EndHipDropProcess()
    {
        isHipDropping = false;
        rb.linearVelocity = Vector3.zero; // 着地時の余分な滑りを止めるために完全に速度を0にする場合（必要に応じて調整してください）

        // 硬直タイマーをセット（ここで1秒間静止させる）
        recoveryTimer = hipDropRecoveryTime;

        if (dustEffect != null)
        {
            dustEffect.Play();
        }
    }
}