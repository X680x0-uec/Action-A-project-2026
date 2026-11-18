using System.Collections.Generic;
using UnityEngine;

// InspectorでバイオームIDと、その場所に出現可能な敵Prefabをひとまとまりで設定する。
[System.Serializable]
public class BiomeEnemyPrefabs
{
    // BiomeMaskの赤チャンネルに記録されている値（森林50、雪原100など）。
    public int biomeId;

    // このバイオームでランダムに選ばれる敵の候補。
    public List<GameObject> enemyPrefabs = new List<GameObject>();
}

// 敵全体を管理するためのスクリプト
public class EnemyManager : MonoBehaviour
{
    // ========================================
    // Inspectorから設定する項目
    // ========================================

    // バイオーム外や該当バイオームの設定がない場合に使う既定の敵Prefab。
    public GameObject enemyPrefab;

    // ワールド座標からBiomeMaskのIDを取得するコンポーネント。
    public BiomeMap biomeMap;

    // バイオームIDごとの敵Prefab候補。候補が複数ならその中からランダムに選ぶ。
    public List<BiomeEnemyPrefabs> biomeEnemyPrefabs = new List<BiomeEnemyPrefabs>();

    // プールとして生成した敵をHierarchy上でまとめておく親オブジェクト。
    public Transform enemyParent;

    // 各Prefabについて起動時に生成する敵の数。スポーン時にInstantiateしないために使う。
    public int enemyPoolSize = 10;

    // 全Prefabを合わせて同時に生存できる敵の最大数。
    public int maxAliveEnemies = 5;

    // 敵を出現させる間隔（秒）。敵数の上限に達している間はスポーンしない。
    public float spawnInterval = 1.0f;

    // Playerから敵を出現させる距離。方向は毎回ランダムに決める。
    public float spawnDistance = 20f;


    // ========================================
    // 内部で使用する変数
    // ========================================

    // すべてのプールに含まれるEnemy。全体の生存数を数えるために使う。
    private List<Enemy> enemyList = new List<Enemy>();

    // 1つのPrefabから作った敵とPrefabの参照をひとまとめに管理する。
    private class EnemyPool
    {
        public GameObject prefab;
        public List<Enemy> enemies = new List<Enemy>();
    }

    // バイオーム別の候補Prefabを、対応するプールへ引けるように保存する。
    private List<EnemyPool> enemyPools = new List<EnemyPool>();

    // 未設定バイオームの警告を同じIDについて何度も表示しないための記録。
    private HashSet<int> warnedBiomeIds = new HashSet<int>();

    // バイオーム別プールが満員の警告を同じIDについて一度だけ表示するための記録。
    private HashSet<int> warnedFullBiomePoolIds = new HashSet<int>();

    // Update内で数えた、現在生きている敵の合計。
    private int aliveEnemyCount = 0;

    // Time.timeがこの時刻に達したら次のスポーンを行う。
    private float nextSpawnTime = 0.0f;


    // ========================================
    // ゲーム開始時に1回だけ実行される処理
    // ========================================
    void Start()
    {
        // 使用するPrefabを集め、Prefabごとの敵をあらかじめ生成する。
        InitializeEnemyPool();

        // Inspectorで設定された間隔が経過してから最初の敵を出現させる。
        nextSpawnTime = Time.time + spawnInterval;
    }

    // 既定Prefabとバイオーム別Prefabを集め、Prefabごとに再利用用プールを作る。
    void InitializeEnemyPool()
    {
        // 同じPrefabが複数のバイオーム候補に含まれても、プールは一つだけ作る。
        List<GameObject> prefabs = new List<GameObject>();
        AddPrefabIfValid(prefabs, enemyPrefab);

        if (biomeEnemyPrefabs != null)
        {
            foreach (BiomeEnemyPrefabs biome in biomeEnemyPrefabs)
            {
                // Inspectorリストの要素自体が未設定なら読み飛ばす。
                if (biome == null)
                {
                    continue;
                }

                // 候補がないバイオームでは、スポーン時に既定Prefabへフォールバックする。
                if (biome.enemyPrefabs == null || biome.enemyPrefabs.Count == 0)
                {
                    Debug.LogWarning(
                        "EnemyManager: バイオームID " + biome.biomeId +
                        " に敵Prefabが設定されていません。既定の敵Prefabを使用します。",
                        this);
                    continue;
                }

                foreach (GameObject prefab in biome.enemyPrefabs)
                {
                    // 候補リスト内の未設定要素も、他の有効な候補の処理を止めずに読み飛ばす。
                    if (prefab == null)
                    {
                        Debug.LogWarning(
                            "EnemyManager: バイオームID " + biome.biomeId +
                            " の敵Prefabが未設定です。既定の敵Prefabを使用します。",
                            this);
                        continue;
                    }

                    AddPrefabIfValid(prefabs, prefab);
                }
            }
        }

        // 有効なPrefabが一つもなければ、生成処理を続けられない。
        if (prefabs.Count == 0)
        {
            Debug.LogError("EnemyManager: 使用できる敵Prefabがありません。", this);
            return;
        }

        // 0体では敵をスポーンできないため、Inspector設定の誤りをログで知らせる。
        if (enemyPoolSize <= 0)
        {
            Debug.LogWarning("EnemyManager: enemyPoolSizeが0以下のため敵プールを作成しません。", this);
            return;
        }

        for (int prefabIndex = 0; prefabIndex < prefabs.Count; prefabIndex++)
        {
            // Prefabごとにプールを作成し、全体リストにも登録する。
            EnemyPool pool = new EnemyPool();
            pool.prefab = prefabs[prefabIndex];
            enemyPools.Add(pool);

            // 全候補の敵を開始時に生成する。各Enemyは最初は非生存状態にしておく。
            for (int i = 0; i < enemyPoolSize; i++)
            {
                GameObject enemyObject = Instantiate(pool.prefab, enemyParent);
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                enemy.isAlive = false;
                pool.enemies.Add(enemy);
                enemyList.Add(enemy);
            }
        }
    }

    // nullやEnemyコンポーネントのないPrefabを除き、有効なPrefabを重複なく追加する。
    void AddPrefabIfValid(List<GameObject> prefabs, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (prefab.GetComponent<Enemy>() == null)
        {
            Debug.LogError("EnemyManager: 敵PrefabにEnemyコンポーネントがありません: " + prefab.name, this);
            return;
        }

        if (!prefabs.Contains(prefab))
        {
            prefabs.Add(prefab);
        }
    }


    // ========================================
    // 毎フレーム実行される処理。スポーン位置や間隔の既存ルールを管理する。
    // ========================================
    void Update()
    {
        // 今後、ゲーム時間に応じた敵の強さを調整する場合はここに追加する。

        // 今後、ゲーム時間に応じて出現間隔を変える場合はここに追加する。
        // 現在はInspectorのspawnIntervalをそのまま使う。

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
        // 前フレームの値を持ち越さず、登録済みの敵を数え直す。
        aliveEnemyCount = 0;

        // すべてのPrefabプールをまとめたリストを確認する。
        foreach (Enemy enemy in enemyList)
        {
            // 外部で破棄された敵がいても例外にせず、生存中の敵だけを数える。
            if (enemy != null && enemy.isAlive)
            {
                // 生きていれば、生存数を1増やす
                aliveEnemyCount++;
            }
        }
    }


    // ========================================
    // 既存のランダムなスポーン位置を決め、その地点のバイオームに合う敵を出す。
    // ========================================
    void SpawnEnemy()
    {
        // どのPrefabプールにも空きがなければ、再利用できる敵が戻るまで待つ。
        if (!HasAvailableEnemy())
        {
            return;
        }

        // 既存の方式と同じく、Playerタグからスポーン位置の基準を取得する。
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("EnemyManager: Playerタグのオブジェクトが見つかりません。", this);
            return;
        }

        // 0～360度のランダムな方向を決め、PlayerからspawnDistanceだけ離れた位置を計算する。
        Vector3 playerPosition = player.transform.position;
        float randomAngle = Random.Range(0f, 360f);
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
        float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
        Vector3 spawnPosition = playerPosition + new Vector3(x, y, 0f) * spawnDistance;

        // このスポーン位置に重なるマスクの赤チャンネルからバイオームIDを取得する。
        // BiomeMap未設定やマップ範囲外の場合、GetBiomeIdは-1を返す。
        int biomeId = biomeMap != null ? biomeMap.GetBiomeId(spawnPosition) : -1;
        EnemyPool selectedPool = FindPoolForBiome(biomeId);

        // 候補Prefab自体は登録済みなのにプールが満員の場合、別バイオームの敵で代用しない。
        if (selectedPool == null && HasConfiguredPoolForBiome(biomeId))
        {
            if (warnedFullBiomePoolIds.Add(biomeId))
            {
                Debug.LogWarning(
                    "EnemyManager: バイオームID " + biomeId +
                    " の敵プールに空きがありません。Enemy Pool Sizeを増やすか、敵の生存数を減らしてください。",
                    this);
            }

            return;
        }

        // 判定不能・未登録ID・有効な候補なしの場合は既定Prefabのプールを使う。
        if (selectedPool == null)
        {
            if (warnedBiomeIds.Add(biomeId))
            {
                Debug.LogWarning(
                    "EnemyManager: バイオームID " + biomeId +
                    " に使用可能な敵Prefabがないため、既定の敵Prefabを使用します。",
                    this);
            }

            selectedPool = FindPool(enemyPrefab);
        }

        // 選ばれた種類のプールから死亡状態の敵を探す。空きがなければスポーンしない。
        Enemy enemyToSpawn = FindAvailableEnemy(selectedPool);
        if (enemyToSpawn == null)
        {
            return;
        }

        // プール内の敵を決定済みの位置へ移動し、生存状態にして表示・行動させる。
        enemyToSpawn.transform.position = spawnPosition;
        Health health = enemyToSpawn.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("EnemyManager: spawned enemy has no Health component.", enemyToSpawn);
            return;
        }

        health.ResetToInitial();
        enemyToSpawn.isAlive = true;
    }

    // IDに対応する有効なPrefabプールが登録されているか調べる。
    // FindPoolForBiomeがnullを返した理由が「設定なし」か「プール満員」かを区別する。
    bool HasConfiguredPoolForBiome(int biomeId)
    {
        if (biomeId < 0 || biomeEnemyPrefabs == null)
        {
            return false;
        }

        foreach (BiomeEnemyPrefabs biome in biomeEnemyPrefabs)
        {
            if (biome == null || biome.biomeId != biomeId || biome.enemyPrefabs == null)
            {
                continue;
            }

            foreach (GameObject prefab in biome.enemyPrefabs)
            {
                if (FindPool(prefab) != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // いずれかのPrefabプールに、再利用できる死亡状態の敵がいるか確認する。
    bool HasAvailableEnemy()
    {
        foreach (EnemyPool pool in enemyPools)
        {
            if (FindAvailableEnemy(pool) != null)
            {
                return true;
            }
        }

        return false;
    }

    // 指定IDの候補Prefabのうち、空きのあるプールから一つをランダムに選ぶ。
    EnemyPool FindPoolForBiome(int biomeId)
    {
        // -1は範囲外などの無効IDなので、バイオーム候補の検索を行わない。
        if (biomeId < 0 || biomeEnemyPrefabs == null)
        {
            return null;
        }

        foreach (BiomeEnemyPrefabs biome in biomeEnemyPrefabs)
        {
            if (biome == null || biome.biomeId != biomeId || biome.enemyPrefabs == null)
            {
                continue;
            }

            // 候補が複数ある場合も、敵プールが満杯の候補は選択対象に含めない。
            int availablePoolCount = 0;
            foreach (GameObject prefab in biome.enemyPrefabs)
            {
                EnemyPool pool = FindPool(prefab);
                if (FindAvailableEnemy(pool) != null)
                {
                    availablePoolCount++;
                }
            }

            if (availablePoolCount == 0)
            {
                continue;
            }

            // 0以上availablePoolCount未満の整数をランダムに選び、候補を一様に選択する。
            int selectedIndex = Random.Range(0, availablePoolCount);
            foreach (GameObject prefab in biome.enemyPrefabs)
            {
                EnemyPool pool = FindPool(prefab);
                if (FindAvailableEnemy(pool) == null)
                {
                    continue;
                }

                if (selectedIndex == 0)
                {
                    return pool;
                }

                selectedIndex--;
            }
        }

        return null;
    }

    // Prefab参照に一致するプールを返す。一致しない場合はnull。
    EnemyPool FindPool(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        foreach (EnemyPool pool in enemyPools)
        {
            if (pool.prefab == prefab)
            {
                return pool;
            }
        }

        return null;
    }

    // プールから再利用可能な死亡状態の敵を一体返す。見つからなければnull。
    Enemy FindAvailableEnemy(EnemyPool pool)
    {
        if (pool == null)
        {
            return null;
        }

        foreach (Enemy enemy in pool.enemies)
        {
            if (enemy != null && !enemy.isAlive)
            {
                return enemy;
            }
        }

        return null;
    }
}