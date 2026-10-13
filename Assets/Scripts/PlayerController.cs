using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    public float moveSpeed = 5f;
    public float dashSpeedMultiplier = 2f; // ダッシュ時の速度倍率

    // Player Input の "OnMove" イベントから呼び出されます
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // XboxコントローラーのRB（Right Shoulder）が押されている間だけ true になる
        bool isDashing = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;

        // ダッシュ中なら速度を倍増させる
        float currentSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * currentSpeed * Time.deltaTime, Space.World);
    }
}