using UnityEngine;

// 敵のデスポーンを管理するスクリプト
public class EnemyDespawn : MonoBehaviour
{
    // ==============================
    // Inspectorから設定する項目
    // ==============================

    // Playerからこの距離以上離れたら敵を死亡させる
    // Inspectorから変更できるようにpublicにする
    public float despawnDistance = 30.0f;


    // ==============================
    // 毎フレーム実行する処理
    // ==============================
    void Update()
    {
        // この敵についているEnemyスクリプトを取得する
        Enemy enemy = GetComponent<Enemy>();

        // Enemyスクリプトが見つからなかった場合は処理を終了する
        if (enemy == null)
        {
            return;
        }

        // 敵がすでに死んでいる場合は処理を終了する
        if (!enemy.isAlive)
        {
            return;
        }

        // タグ「Player」を持つGameObjectを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つからなかった場合は処理を終了する
        if (player == null)
        {
            return;
        }

        // 敵とPlayerの間の距離を計算する
        float distance = Vector3.Distance(
            transform.position,
            player.transform.position
        );

        // 敵とPlayerの距離がデスポーン距離以上になったか確認する
        if (distance >= despawnDistance)
        {
            // デスポーンさせるため、敵を死亡状態にする
            enemy.isAlive = false;
        }
    }
}
