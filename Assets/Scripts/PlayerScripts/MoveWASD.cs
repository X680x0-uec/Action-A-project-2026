using UnityEngine;
using UnityEngine.InputSystem;

public class MoveWASD : MonoBehaviour
{
    // 移動速度
    public float moveSpeed = 5f;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        float x = 0f;
        float y = 0f;

        if (keyboard != null)
        {
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
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            x += stick.x;
            y += stick.y;
        }

        // 移動
        Vector3 moveDirection = new Vector3(x, y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
