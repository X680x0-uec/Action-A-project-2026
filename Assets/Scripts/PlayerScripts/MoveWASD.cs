using UnityEngine;
using UnityEngine.InputSystem;

public class MoveWASD : MonoBehaviour
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

        // 入力を取得
        float x = 0f;
        float y = 0f;

        if (keyboard.wKey.isPressed)
        {
            y += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            y -= 1f;
        }

        if (keyboard.aKey.isPressed)
        {
            x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            x += 1f;
        }

        // 移動
        Vector3 moveDirection = new Vector3(x, y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
