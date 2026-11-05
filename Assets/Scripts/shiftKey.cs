using UnityEngine;

public class ShiftKey : MonoBehaviour
{
    public static bool IsShiftPressed = false;

    [Header("キーの設定")]
    public Transform visualMesh;
    public float pressDepth = 0.1f;
    public float moveSpeed = 20f;

    private Vector3 defaultLocalPos;
    private Vector3 pressedLocalPos;
    private int touchingPlayers = 0;

    void Start()
    {
        if (visualMesh == null)
        {
            visualMesh = transform;
        }

        defaultLocalPos = visualMesh.localPosition;
        pressedLocalPos = defaultLocalPos - new Vector3(0, pressDepth, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            touchingPlayers++;
            UpdateShiftState();
            Debug.Log($"【Shiftキー】プレイヤーが乗りました。（現在の乗員数: {touchingPlayers}）");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            touchingPlayers = Mathf.Max(0, touchingPlayers - 1);
            UpdateShiftState();
            Debug.Log($"【Shiftキー】プレイヤーが離れました。（残りの乗員数: {touchingPlayers}）");
        }
    }

    private void UpdateShiftState()
    {
        bool newState = (touchingPlayers > 0);
        if (IsShiftPressed != newState)
        {
            IsShiftPressed = newState;
            if (IsShiftPressed)
            {
                Debug.Log("<color=cyan>【Shift状態】 ➡ ON (大文字入力が可能になりました！)</color>");
            }
            else
            {
                Debug.Log("<color=yellow>【Shift状態】 ➡ OFF (小文字入力に戻ります)</color>");
            }
        }
    }

    void Update()
    {
        Vector3 targetPos = (touchingPlayers > 0) ? pressedLocalPos : defaultLocalPos;
        visualMesh.localPosition = Vector3.Lerp(visualMesh.localPosition, targetPos, Time.deltaTime * moveSpeed);
    }
}
