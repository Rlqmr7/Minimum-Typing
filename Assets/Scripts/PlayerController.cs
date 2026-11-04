using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))] // ★ PlayerInputを必須にする
public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    private Rigidbody rb;
    private CapsuleCollider col;
    private PlayerInput playerInput; // ★ どのプレイヤー（プレイヤー1、プレイヤー2）か識別するコンポーネント

    [Header("移動設定")]
    public float moveForce = 35f;
    public float maxSpeed = 7f;
    public float dashSpeedMultiplier = 1.8f;

    [Header("ジャンプ・ヒップドロップ設定")]
    public float jumpForce = 8f;
    public float hipDropForce = 50f;
    public float hipDropRecoveryTime = 1.0f;
    private float recoveryTimer = 0f;

    [Header("滞空時間調整（落下・重力強化）")]
    public float gravityMultiplier = 2.5f;

    private bool isGrounded;
    private bool isHipDropping = false;

    public bool IsHipDropping => isHipDropping;
    public bool IsRecovering => recoveryTimer > 0f;

    [Header("エフェクト設定")]
    public ParticleSystem dustEffect;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        rb.freezeRotation = true;

        if (dustEffect == null)
        {
            dustEffect = GetComponentInChildren<ParticleSystem>();
        }
    }

    // PlayerInput コンポーネントから自動で呼ばれる
    public void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();

        // ★ ここでコンソールに入力が届いているか確認する
        Debug.Log("入力値: " + rawInput);

        if (rawInput.magnitude < 0.05f)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = rawInput;
        }
    }

    // PlayerInput からボタンが押されたときに呼ばれるアクション（後述の設定用）
    public void OnJumpOrHipDrop(InputValue value)
    {
        if (value.isPressed && recoveryTimer <= 0f)
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

    public void OnDash(InputValue value)
    {
        // ダッシュボタンが押されているかどうかの判定用などとして拡張可能
    }

    void Update()
    {
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
        }

        float rayDistance = col.height / 2f + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance);

        Debug.DrawRay(transform.position, Vector3.down * rayDistance, isGrounded ? Color.red : Color.blue);

        if (isHipDropping && isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
            {
                if (hit.collider.GetComponent<PhysicalKey>() == null)
                {
                    EndHipDropProcess();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded && !isHipDropping)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        if (isHipDropping || recoveryTimer > 0f) return;

        // Gamepad.current ではなく、このプレイヤーに割り当てられたデバイスからダッシュ入力を取ることもできますが、
        // 簡易的に各プレイヤーの右肩ボタン（または特定の判定）にしたい場合は Input System の Actions を使います。
        // ここでは便宜上、既存のロジックを維持しつつ調整します。
        bool isDashing = false;
        if (playerInput.devices.Count > 0 && playerInput.devices[0] is Gamepad pad)
        {
            isDashing = pad.rightShoulder.isPressed;
        }
        else if (Keyboard.current != null)
        {
            isDashing = Keyboard.current.shiftKey.isPressed;
        }

        float currentMaxSpeed = isDashing ? maxSpeed * dashSpeedMultiplier : maxSpeed;

        Vector3 targetVelocity = new Vector3(moveInput.x * currentMaxSpeed, rb.linearVelocity.y, moveInput.y * currentMaxSpeed);
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void StartHipDrop()
    {
        isHipDropping = true;
        recoveryTimer = 0f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
    }

    public void EndHipDrop()
    {
        if (isHipDropping)
        {
            EndHipDropProcess();
        }
    }

    private void EndHipDropProcess()
    {
        isHipDropping = false;
        rb.linearVelocity = Vector3.zero;
        recoveryTimer = hipDropRecoveryTime;

        if (dustEffect != null)
        {
            dustEffect.Play();
        }
    }
}