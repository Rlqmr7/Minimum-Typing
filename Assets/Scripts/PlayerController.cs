using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    private Rigidbody rb;
    private CapsuleCollider col;
    private PlayerInput playerInput;

    [Header("移動設定")]
    public float moveForce = 35f;
    public float maxSpeed = 7f;
    public float dashSpeedMultiplier = 1.8f;

    [Header("ジャンプ・ヒップドロップ設定")]
    public float jumpForce = 8f;
    public float hipDropForce = 50f;
    public float hipDropRecoveryTime = 0.5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = ~0;
    private float recoveryTimer = 0f;
    private float jumpLockTimer = 0f;

    [Header("滞空時間調整（落下・重力強化）")]
    public float gravityMultiplier = 2.5f;

    [Header("モデル・アニメーション設定")]
    public Animator animator;
    public Transform modelTransform;
    public float turnSpeed = 12f;

    [Header("エフェクト設定")]
    public ParticleSystem dustEffect;

    private bool isGrounded;
    private bool isHipDropping = false;
    private bool isDashing = false;
    private Vector3 externalWindVelocity = Vector3.zero;

    public bool IsHipDropping => isHipDropping;
    public bool IsRecovering => recoveryTimer > 0f;
    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsHipDroppingHash = Animator.StringToHash("IsHipDropping");
    private static readonly int IsRecoveringHash = Animator.StringToHash("IsRecovering");
    private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");

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

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (modelTransform == null && animator != null)
        {
            modelTransform = animator.transform;
        }
    }

    // PlayerInput コンポーネントから自動で呼ばれる（Move アクション用）
    public void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();
        moveInput = rawInput.magnitude < 0.05f ? Vector2.zero : rawInput;
    }

    // PlayerInput コンポーネントから自動で呼ばれる（Jump アクション用）
    public void OnJump(InputValue value)
    {
        if (!value.isPressed || recoveryTimer > 0f) return;

        // 接地中かつヒップドロップ中でないときだけジャンプ
        if (isGrounded && !isHipDropping && jumpLockTimer <= 0f)
        {
            isGrounded = false;
            jumpLockTimer = 0.15f;

            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // PlayerInput コンポーネントから自動で呼ばれる（HipDrop / ZR アクション用）
    public void OnHipDrop(InputValue value)
    {
        if (!value.isPressed || recoveryTimer > 0f) return;

        // 空中かつヒップドロップ中でないときだけヒップドロップ開始
        if (!isGrounded && !isHipDropping)
        {
            StartHipDrop();
        }
    }

    public void OnDash(InputValue value)
    {
        // ダッシュボタン単体押下時の拡張用
    }

    // FanZone など外部ギミックから風速度を加算する
    public void AddExternalWind(Vector3 windVelocity)
    {
        externalWindVelocity += windVelocity;
    }

    void Update()
    {
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
        }

        if (jumpLockTimer > 0f)
        {
            jumpLockTimer -= Time.deltaTime;
        }

        CheckGrounded();
        UpdateModelRotationAndAnimation();
    }

    private void CheckGrounded()
    {
        if (jumpLockTimer > 0f)
        {
            isGrounded = false;
            Debug.DrawRay(GetGroundRayOrigin(), Vector3.down * (groundCheckDistance + 0.05f), Color.blue);
            return;
        }

        Vector3 origin = GetGroundRayOrigin();
        float rayLength = groundCheckDistance + 0.05f;
        isGrounded = Physics.Raycast(origin, Vector3.down, rayLength, groundLayer, QueryTriggerInteraction.Ignore);

        Debug.DrawRay(origin, Vector3.down * rayLength, isGrounded ? Color.red : Color.blue);
    }

    private Vector3 GetGroundRayOrigin()
    {
        if (col == null) return transform.position + Vector3.up * 0.05f;

        return new Vector3(
            col.bounds.center.x,
            col.bounds.min.y + 0.05f,
            col.bounds.center.z
        );
    }

    void FixedUpdate()
    {
        if (!isGrounded && !isHipDropping)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        if (isHipDropping || recoveryTimer > 0f)
        {
            isDashing = false;
            externalWindVelocity = Vector3.zero;
            return;
        }

        isDashing = false;
        if (playerInput.devices.Count > 0 && playerInput.devices[0] is Gamepad pad)
        {
            isDashing = pad.rightShoulder.isPressed;
        }
        else if (Keyboard.current != null)
        {
            isDashing = Keyboard.current.shiftKey.isPressed;
        }

        float currentMaxSpeed = isDashing ? maxSpeed * dashSpeedMultiplier : maxSpeed;

        // プレイヤーの移動入力と扇風機の風速度を合成
        Vector3 inputVelocity = new Vector3(moveInput.x * currentMaxSpeed, 0f, moveInput.y * currentMaxSpeed);
        Vector3 targetVelocity = inputVelocity + new Vector3(externalWindVelocity.x, 0f, externalWindVelocity.z);

        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0f;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // 上方向の風（上昇気流）がある場合の加算
        if (externalWindVelocity.y > 0.01f)
        {
            rb.AddForce(Vector3.up * externalWindVelocity.y, ForceMode.Acceleration);
        }

        externalWindVelocity = Vector3.zero;
    }

    private void UpdateModelRotationAndAnimation()
    {
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVel.magnitude;

        if (speed > 0.1f && !isHipDropping && recoveryTimer <= 0f && modelTransform != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(horizontalVel.normalized, Vector3.up);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, speed);
            animator.SetBool(IsHipDroppingHash, isHipDropping);
            animator.SetBool(IsRecoveringHash, recoveryTimer > 0f);
            animator.SetBool(IsDashingHash, isDashing && speed > 0.1f && isGrounded);
        }
    }

    void StartHipDrop()
    {
        isHipDropping = true;
        isDashing = false;
        recoveryTimer = 0f;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
    }

    // PhysicalKey.cs から呼ばれる終了処理
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
        recoveryTimer = hipDropRecoveryTime;

        if (dustEffect != null)
        {
            dustEffect.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // PhysicalKey 以外の地面・床にヒップドロップ着地したときの処理
        if (isHipDropping && collision.gameObject.GetComponent<PhysicalKey>() == null)
        {
            EndHipDropProcess();
        }
    }
}