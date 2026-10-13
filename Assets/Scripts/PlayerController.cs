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
    public float moveSpeed = 7f; // Rigidbody用に少し速めの値を設定
    public float dashSpeedMultiplier = 1.8f;

    [Header("ジャンプ・ヒップドロップ設定")]
    public float jumpForce = 8f; // ジャンプの力
    public float hipDropForce = 15f; // ヒップドロップの急降下スピード
    private bool isGrounded; // 地面にいるか
    private bool isHipDropping = false; // ヒップドロップ中か

    void Awake()
    {
        // 必要なコンポーネントを取得
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    // Player Input の "OnMove" イベントから呼び出されます
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // --- 接地判定（簡易版） ---
        // カプセルの少し下に向かってレイ（光線）を飛ばし、地面に触れているか確認
        float rayDistance = col.height / 2f + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance);

        // デバッグ用に地面を検知したら赤、空中なら青でレイを表示
        Debug.DrawRay(transform.position, Vector3.down * rayDistance, isGrounded ? Color.red : Color.blue);

        // --- ヒップドロップの着地判定 ---
        if (isHipDropping && isGrounded)
        {
            isHipDropping = false;
            // 着地した瞬間、少し移動を止めるなどの処理を入れても良い（今回はそのまま）
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Y方向の速度をリセット
            Debug.Log("着地！ヒップドロップ終了。");
        }

        // --- Aボタン（Jump / HipDrop）の入力判定 ---
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                // 地面：ジャンプ
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isHipDropping = false; // ジャンプ中はヒップドロップ状態をリセット
            }
            else if (!isHipDropping)
            {
                // 空中：ヒップドロップ
                StartHipDrop();
            }
        }
    }

    void FixedUpdate()
    {
        // --- 物理演算を用いた移動処理 ---
        // ヒップドロップ中は移動操作を受け付けないようにする場合
        if (isHipDropping) return;

        // ダッシュ判定（XboxコントローラーのRB長押し）
        bool isDashing = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
        float currentSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;

        // 入力方向から移動速度（velocity）を作成
        Vector3 movement = new Vector3(moveInput.x * currentSpeed, rb.linearVelocity.y, moveInput.y * currentSpeed);
        rb.linearVelocity = movement;
    }

    void StartHipDrop()
    {
        isHipDropping = true;
        // 上方向への速度を完全にリセットしてから
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        // 真下に向かって一気に力を加える
        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
        Debug.Log("ヒップドロップ！");
    }
}