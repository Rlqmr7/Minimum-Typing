using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [Header("遷移先のシーン名")]
    public string nextSceneName = "DifficultyScene";

    void Update()
    {
        // 1. キーボードのいずれかのキーが押されたか
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        // 2. ゲームパッドのいずれかのボタン（南ボタン、東ボタン、十字キーなど）が押されたか
        bool gamepadPressed = false;
        if (Gamepad.current != null)
        {
            // 代表的なボタン（A/B/X/Y または 十字キー）のいずれかが押されたかチェック
            var gp = Gamepad.current;
            if (gp.buttonSouth.wasPressedThisFrame ||
                gp.buttonNorth.wasPressedThisFrame ||
                gp.buttonEast.wasPressedThisFrame ||
                gp.buttonWest.wasPressedThisFrame ||
                gp.startButton.wasPressedThisFrame ||
                gp.selectButton.wasPressedThisFrame ||
                gp.dpad.up.wasPressedThisFrame ||
                gp.dpad.down.wasPressedThisFrame ||
                gp.dpad.left.wasPressedThisFrame ||
                gp.dpad.right.wasPressedThisFrame)
            {
                gamepadPressed = true;
            }
        }

        // どちらかが押されていたらシーンを切り替える
        if (keyboardPressed || gamepadPressed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}