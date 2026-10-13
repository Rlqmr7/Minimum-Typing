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
    public float moveForce = 35f; // 移動に加える力
    public float maxSpeed = 7f;   // 最大移動速度
    public float dashSpeedMultiplier = 1.8f;

    [Header("ジャンプ・ヒップドロップ設定")]
    public float jumpForce = 8f;
    public float hipDropForce = 20f; // 少し強めに調整
    private bool isGrounded;
    private bool isHipDropping = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        // 回転が勝手に起きないように固定
        rb.freezeRotation = true;
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

        // --- ヒップドロップの着地判定 ---
        if (isHipDropping && isGrounded)
        {
            isHipDropping = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Debug.Log("着地！ヒップドロップ終了。");
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

        // 速度を直接上書きせず、力を加えて動かすことで、下のキー（PhysicalKey）を押し潰せるようにする
        Vector3 targetVelocity = new Vector3(moveInput.x * currentMaxSpeed, rb.linearVelocity.y, moveInput.y * currentMaxSpeed);
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0; // Y軸方向の速度変化は除外（重力やジャンプを邪魔しないため）

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void StartHipDrop()
    {
        isHipDropping = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * hipDropForce, ForceMode.Impulse);
        Debug.Log("ヒップドロップ！");
    }
}