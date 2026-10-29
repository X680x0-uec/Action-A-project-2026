using UnityEngine;

public class MoveWASD : MonoBehaviour
{
    // 移動速度
    public float moveSpeed = 5f;

    void Update()
    {
        // 入力を取得
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            y += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            y -= 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x += 1f;
        }

        // 移動
        Vector3 moveDirection = new Vector3(x, y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
