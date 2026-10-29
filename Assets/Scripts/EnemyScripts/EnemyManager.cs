using System.Collections.Generic;
using UnityEngine;

// 敵全体を管理するためのスクリプト
public class EnemyManager : MonoBehaviour
{
    // ========================================
    // Inspectorから設定する項目
    // ========================================

    // 生成する敵の元となるEnemy
    // UnityのInspectorからEnemyを設定する
    public GameObject enemyPrefab;

    // 生成した敵をまとめて入れる親オブジェクト
    public Transform enemyParent;

    // あらかじめ生成しておく敵の数
    public int enemyPoolSize = 10;

    // 同時に生存できる敵の最大数
    public int maxAliveEnemies = 5;

    // 敵を出現させる間隔（秒）
    public float spawnInterval = 1.0f;

    // Playerから敵を出現させる距離
    public float spawnDistance = 20f;


    // ========================================
    // 内部で使用する変数
    // ========================================

    // 生成したEnemyを全て保存しておくリスト
    private List<Enemy> enemyList = new List<Enemy>();

    // 現在、生きている敵の数
    private int aliveEnemyCount = 0;

    // 次に敵を出現させる時刻
    private float nextSpawnTime = 0.0f;


    // ========================================
    // ゲーム開始時に1回だけ実行される処理
    // ========================================
    void Start()
    {
        // 敵をenemyPoolSizeの数だけ生成する
        for (int i = 0; i < enemyPoolSize; i++)
        {
            // enemyPrefabを複製して、新しい敵を1体作る
            // enemyParentを指定することで、その中に生成されるのでhierarchyがすっきりする
            GameObject enemyObject = Instantiate(enemyPrefab, enemyParent);

            // 生成した敵をEnemy型として取得する
            Enemy enemy = enemyObject.GetComponent<Enemy>();

            // 生成した敵をリストに追加する
            enemyList.Add(enemy);

            // 最初は死亡状態にする
            enemy.isAlive = false;
        }

        // 最初の敵を1秒後に出現させる
        nextSpawnTime = Time.time + spawnInterval;
    }


    // ========================================
    // 毎フレーム実行される処理
    // ========================================
    void Update()
    {
        // ここで(ゲーム時間から現在の敵の強さを計算する)

        // ここで(ゲーム時間から敵の出現間隔を計算する)
        // 現在はspawnIntervalをそのまま使用する

        // 生きている敵の数を数える
        UpdateAliveEnemyCount();

        // 現在の敵数が最大数に達していないか確認する
        if (aliveEnemyCount < maxAliveEnemies)
        {
            // 現在時刻が、次の出現時刻になったか確認する
            if (Time.time >= nextSpawnTime)
            {
                // 敵を出現させる
                SpawnEnemy();

                // 次に敵を出現させる時刻を設定する
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
    }


    // ========================================
    // 現在生きている敵の数を数える処理
    // ========================================
    void UpdateAliveEnemyCount()
    {
        // 生きている敵の数を0に戻す
        aliveEnemyCount = 0;

        // リストに入っている全ての敵を順番に確認する
        foreach (Enemy enemy in enemyList)
        {
            // その敵が生きているか確認する
            if (enemy.isAlive)
            {
                // 生きていれば、生存数を1増やす
                aliveEnemyCount++;
            }
        }
    }


    // ========================================
    // 敵を1体出現させる処理
    // ========================================
    void SpawnEnemy()
    {
        // ここで(出現する敵の種類を決める)
        // 現在は敵の種類は1種類だけなので何もしない

        // リストの中から死亡状態の敵を1体探す
        foreach (Enemy enemy in enemyList)
        {
            // まだ死亡状態なら、この敵を出現させる
            if (!enemy.isAlive)
            {
                // Tagが「Player」のオブジェクトを探す
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                // Playerの位置を取得する
                Vector3 playerPosition = player.transform.position;

                // 0～360度の中から、敵を出現させる方向をランダムに決める
                float randomAngle = Random.Range(0f, 360f);

                // ランダムに決めた角度を、X方向とY方向の移動量に変換する
                float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
                float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad);

                // ランダムな方向にspawnDistanceだけ離れた位置を計算する
                Vector3 spawnPosition =
                    playerPosition + new Vector3(x, y, 0f) * spawnDistance;

                // 計算した位置に敵を移動させる
                enemy.transform.position = spawnPosition;

                // ここで(敵を出現状態に初期化する)
                // 現在は初期化が必要な処理はない

                // 敵を生存状態にする
                enemy.isAlive = true;

                // 1体出現させたら、この処理を終了する
                break;
            }
        }
    }
}