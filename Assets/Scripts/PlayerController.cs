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
    private bool isGrounded;
    private bool isHipDropping = false;

<<<<<<< HEAD
    // 外部（PhysicalKey）からヒップドロップ中か確認できるようにするプロパティ
    public bool IsHipDropping => isHipDropping;
=======
    [Header("エフェクト設定")]
    public ParticleSystem dustEffect; // 砂煙のパーティクル
>>>>>>> 9336a3ad5ba1c5e0a0a89515afe8eb9280b3a6f5

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
<<<<<<< HEAD
            // 真下にあるのが「PhysicalKey」でなければ、通常通りヒップドロップ終了
            // （キーの上に着地したときは、PhysicalKey側の判定に任せるためここでは消さない）
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
            {
                if (hit.collider.GetComponent<PhysicalKey>() == null)
                {
                    isHipDropping = false;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    Debug.Log("着地！ヒップドロップ終了。");
                }
=======
            isHipDropping = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // ★着地した瞬間に砂煙を再生！
            if (dustEffect != null)
            {
                dustEffect.Play();
                Debug.Log("着地！ヒップドロップ終了＆砂煙再生！");
>>>>>>> 9336a3ad5ba1c5e0a0a89515afe8eb9280b3a6f5
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

    // --- キー側（PhysicalKey）から呼ばれてヒップドロップを安全に終了させるメソッド ---
    public void EndHipDrop()
    {
        if (isHipDropping)
        {
            isHipDropping = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Debug.Log("キーを踏んでヒップドロップ終了！");
        }
    }
}