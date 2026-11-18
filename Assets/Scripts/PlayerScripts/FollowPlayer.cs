using UnityEngine;

/// 適用した物体とプレイヤーの初期の位置関係を維持するプログラム

public class FollowPlayer : MonoBehaviour
{
    // Playerから見た自分の相対座標
    private Vector3 offset;

    // ゲーム開始時に1回だけ実行される
    void Start()
    {
        // Tagが「Player」のオブジェクトを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerと自分の現在位置から相対座標を計算する
        offset = transform.position - player.transform.position;
    }

    // 毎フレーム実行される
    void Update()
    {
        // Tagが「Player」のオブジェクトを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerの位置 + 最初に設定した相対座標に自分を移動する
        transform.position = player.transform.position + offset;
    }
}