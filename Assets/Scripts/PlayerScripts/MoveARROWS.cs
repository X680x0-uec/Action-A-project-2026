using UnityEngine;

public class MoveARROWS : MonoBehaviour
{
    // 移動速度
    public float moveSpeed = 5f;

    void Update()
    {
        float x = 0f;
        float y = 0f;

        // 上矢印
        if (Input.GetKey(KeyCode.UpArrow))
        {
            y += 1f;
        }

        // 下矢印
        if (Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1f;
        }

        // 左矢印
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1f;
        }

        // 右矢印
        if (Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }

        // 移動
        Vector3 moveDirection = new Vector3(x, y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}