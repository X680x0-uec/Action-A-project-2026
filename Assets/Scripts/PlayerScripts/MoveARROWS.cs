using UnityEngine;
using UnityEngine.InputSystem;

public class MoveARROWS : MonoBehaviour
{
    // 移動速度
    public float moveSpeed = 5f;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        float x = 0f;
        float y = 0f;

        // 上矢印
        if (keyboard.upArrowKey.isPressed)
        {
            y += 1f;
        }

        // 下矢印
        if (keyboard.downArrowKey.isPressed)
        {
            y -= 1f;
        }

        // 左矢印
        if (keyboard.leftArrowKey.isPressed)
        {
            x -= 1f;
        }

        // 右矢印
        if (keyboard.rightArrowKey.isPressed)
        {
            x += 1f;
        }

        // 移動
        Vector3 moveDirection = new Vector3(x, y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}