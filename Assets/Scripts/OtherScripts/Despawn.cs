using UnityEngine;

public class Despawn : MonoBehaviour
{
    // ========================================
    // Inspectorから設定する項目
    // ========================================

    // Playerからこの距離以上離れたら消滅
    // 0以上：有効
    // 負の値：無効
    public float maxDistance = 30.0f;

    // 生成されてからこの秒数経過したら消滅
    // 0以上：有効
    // 負の値：無効
    public float maxLifetime = 10.0f;


    // ========================================
    // 内部で使用する変数
    // ========================================

    // Playerを記録する
    private Transform player;

    // この物体が生成された時間
    private float spawnTime;


    void Start()
    {
        // 生成された時間を記録
        spawnTime = Time.time;

        // Playerタグの物体を探す
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つかった場合
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }


    void Update()
    {
        // ========================================
        // 経過時間による消滅判定
        // ========================================

        // maxLifetimeが0以上の場合だけ判定する
        if (maxLifetime >= 0)
        {
            // 生成されてからの経過時間
            float elapsedTime = Time.time - spawnTime;

            // 設定時間以上経過していたら消滅
            if (elapsedTime >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }
        }


        // ========================================
        // Playerとの距離による消滅判定
        // ========================================

        // maxDistanceが0以上の場合だけ判定する
        // かつ、Playerが見つかっている場合
        if (maxDistance >= 0 && player != null)
        {
            // Playerとの距離を計算
            float distance = Vector2.Distance(
                transform.position,
                player.position
            );

            // 設定した距離以上離れていたら消滅
            if (distance >= maxDistance)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}