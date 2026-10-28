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

    [Header("滞空時間調整（落下・重力強化）")]
    public float gravityMultiplier = 2.5f; // 値が大きいほど素早く落ちます（デフォルトは2.0〜3.0あたりがおすすめ）

    private bool isGrounded;
    private bool isHipDropping = false;

    // 外部（PhysicalKey）からヒップドロップ中か確認できるようにするプロパティ
    public bool IsHipDropping => isHipDropping;

    [Header("エフェクト設定")]
    public ParticleSystem dustEffect; // 砂煙のパーティクル

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;

        // もしインスペクターで割り当てていなければ自動取得
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
                    isHipDropping = false;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                    if (dustEffect != null)
                    {
                        dustEffect.Play();
                    }
                    Debug.Log("着地！ヒップドロップ終了＆砂煙再生！");
                }
            }
        }

        // --- Aボタン（Jump / HipDrop）の入力判定 ---
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
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
            // 標準の重力に加えて、さらに下向きの力を加えることで落下スピードを速くする
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        if (isHipDropping) return;

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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
        Debug.Log("ヒップドロップ！");
    }

    public void EndHipDrop()
    {
        if (isHipDropping)
        {
            isHipDropping = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (dustEffect != null)
            {
                dustEffect.Play();
            }
            Debug.Log("キーを踏んでヒップドロップ終了＆砂煙再生！");
        }
    }
}