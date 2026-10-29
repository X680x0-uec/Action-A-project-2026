using UnityEngine;

// オブジェクトの最後の移動方向を記録するスクリプト
public class ObjectDirection : MonoBehaviour
{
    // 最後に移動した方向
    public Vector2 direction = Vector2.down;

    // 前のフレームでの位置
    private Vector2 previousPosition;

    // ゲーム開始時に呼ばれる
    void Start()
    {
        // 開始時の位置を記録する
        previousPosition = transform.position;
    }

    // 毎フレーム呼ばれる
    void Update()
    {
        // 現在の位置を取得する
        Vector2 currentPosition = transform.position;

        // 前のフレームから現在のフレームまでの移動量を計算する
        Vector2 movement = currentPosition - previousPosition;

        // 移動している場合
        if (movement != Vector2.zero)
        {
            // 移動方向を正規化して記録する
            direction = movement.normalized;
        }

        // 現在の位置を次のフレームのために保存する
        previousPosition = currentPosition;
    }
}