using UnityEngine;

// 敵1体を管理するためのスクリプト
public class Enemy : MonoBehaviour
{
    // ==============================
    // Inspectorから設定する項目
    // ==============================

    // 敵が生きているかどうかを表す変数
    // true  = 生きている
    // false = 死んでいる
    public bool isAlive = false;

    // 敵が1秒間に移動する距離
    public float moveSpeed = 2.0f;

    // プレイヤーに接触したときに減らすHP
    public int contactDamage = 1;

    // 敵の見た目を表示・非表示にするためのSpriteRenderer
    private SpriteRenderer spriteRenderer;


    // ==============================
    // ゲーム開始時に1回だけ実行する処理
    // ==============================
    void Start()
    {
        // 敵の初期位置を(10, 10)に設定する
        transform.position = new Vector3(10f, 10f, 0f);

        // この敵に付いているSpriteRendererを取得する
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 現在の生存状態に合わせて表示・非表示を設定する
        UpdateAliveState();
    }


    // ==============================
    // 毎フレーム実行する処理
    // ==============================
    void Update()
    {
        // 現在の生存状態に応じて処理を分ける
        if (isAlive)
        {
            // 敵が生きている場合は表示する
            UpdateAliveState();

            // Playerに向かって移動する
            MoveToPlayer();

            // ここに(プレイヤーへの攻撃処理など)を入れる
        }
        else
        {
            // 敵が死んでいる場合は非表示にする
            UpdateAliveState();
        }
    }


    // ==============================
    // Playerに向かって移動する処理
    // ==============================
    void MoveToPlayer()
    {
        // タグ「Player」を持つGameObjectを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つからなかった場合は処理を終了する
        if (player == null)
        {
            return;
        }

        // 敵からPlayerへ向かう方向を計算する
        Vector3 direction = player.transform.position - transform.position;

        // 方向だけを利用したいので、長さを1にする
        direction = direction.normalized;

        // Playerの方向へ移動する
        transform.position += direction * moveSpeed * Time.deltaTime;
    }


    // ==============================
    // 敵を死亡状態にする処理
    // ==============================
    void Die(bool dropItem)
    {
        // すでに死んでいる場合は何もしない
        if (!isAlive)
        {
            return;
        }

        // 敵を死亡状態にする
        isAlive = false;

        // アイテムをドロップするか確認する
        if (dropItem)
        {
            // アイテムドロップ用スクリプトがあれば、敵の位置にアイテムを生成する
            EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();
            if (itemDrop != null)
            {
                itemDrop.DropItem(transform.position);
            }
        }

        // ここに(死亡時の処理)を入れる

        // 敵を非表示にする
        UpdateAliveState();
    }


    // ==============================
    // 敵の表示・非表示を切り替える処理
    // ==============================
    void UpdateAliveState()
    {
        // SpriteRendererがまだ取得できていない場合は何もしない
        if (spriteRenderer == null)
        {
            return;
        }

        // 生きているなら表示し、
        // 死んでいるなら非表示にする
        spriteRenderer.enabled = isAlive;
    }


    // ==============================
    // 敵が他の2D物体に接触したときに呼ばれる処理
    // ==============================
    void OnTriggerEnter2D(Collider2D other)
    {
        // 敵が死んでいる場合は何もしない
        if (!isAlive)
        {
            return;
        }


        // ==============================
        // Playerと接触した場合
        // ==============================
        if (other.CompareTag("Player"))
        {
            // シーン内のHPBarを探す
            PlayerHPBar playerHPBar = FindAnyObjectByType<PlayerHPBar>();

            // HPBarが見つかった場合だけ処理する
            if (playerHPBar != null)
            {
                // プレイヤーのHPをcontactDamage分減らす
                playerHPBar.TakeDamage(contactDamage);
            }

            // Playerとの衝突による死亡なので、
            // アイテムをドロップする
            Die(true);

            // Playerとの処理はここで終了する
            return;
        }


        // ==============================
        // Bulletと接触した場合
        // ==============================
        if (other.CompareTag("Bullet"))
        {
            // 接触したBulletについているBulletスクリプトを取得する
            Bullet bullet = other.GetComponent<Bullet>();

            // Bulletスクリプトが見つかった場合だけ処理する
            if (bullet != null)
            {
                // Bulletが生きている場合だけ命中として扱う
                if (bullet.isAlive)
                {
                    // Bulletとの衝突による死亡なので、
                    // アイテムをドロップする
                    Die(true);

                    // Bulletも死亡状態にする
                    bullet.isAlive = false;

                    // Bulletを非表示にする
                    SpriteRenderer bulletSpriteRenderer =
                        other.GetComponent<SpriteRenderer>();

                    if (bulletSpriteRenderer != null)
                    {
                        bulletSpriteRenderer.enabled = false;
                    }
                }
            }
        }
    }
}