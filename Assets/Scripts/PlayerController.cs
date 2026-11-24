using System.Collections;
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
    public float hipDropRecoveryTime = 1.0f;
    private float recoveryTimer = 0f;

    [Header("行動不能（スタン）設定")]
    private bool isStunned = false;
    private Coroutine stunCoroutine;

    [Header("滞空時間調整（落下・重力強化）")]
    public float gravityMultiplier = 2.5f;

    private bool isGrounded;
    private bool isHipDropping = false;

    public bool IsGrounded => isGrounded;
    public bool IsHipDropping => isHipDropping;
    public bool IsRecovering => recoveryTimer > 0f;
    public bool IsStunned => isStunned;

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

    public void OnMove(InputValue value)
    {
        if (isStunned)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 rawInput = value.Get<Vector2>();

        if (rawInput.magnitude < 0.05f)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = rawInput;
        }
    }

    public void OnJumpOrHipDrop(InputValue value)
    {
        if (isStunned) return;

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

        if (isStunned || isHipDropping || recoveryTimer > 0f) return;

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

    // 外部（DaipanGimmickなど）から行動不能を開始する
    public void StartStun(float duration)
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        isHipDropping = false;
        moveInput = Vector2.zero;

        // 横方向の慣性を止めてその場で行動不能にする（落下Y速度は維持）
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        Debug.Log($"{gameObject.name}：台パンヒット！{duration}秒間行動不能");

        yield return new WaitForSeconds(duration);

        isStunned = false;
        stunCoroutine = null;
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