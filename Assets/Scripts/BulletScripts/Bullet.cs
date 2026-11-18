using UnityEngine;

// 弾1個を管理するためのスクリプト
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    // ==============================
    // Inspectorから設定する項目
    // ==============================

    // 弾が生きているかどうかを表す変数
    // true  = 生きている
    // false = 死んでいる
    public bool isAlive = false;

    // 弾が1秒間に移動する距離
    public float moveSpeed = 5.0f;

    // 命中時に対象へ与えるダメージ
    public int damage = 10;

    // 弾がPlayerから離れてよい最大距離
    public float maxDistanceFromPlayer = 30.0f;

    // 弾の見た目を表示・非表示にするためのSpriteRenderer
    private SpriteRenderer spriteRenderer;

    // 弾が飛んでいる方向
    private Vector2 direction = Vector2.down;

    // 前のフレームで弾が生きていたかどうか
    // 「死んでいる→生きている」の変化を検出するために使う
    private bool previousIsAlive = false;

    // 発射時に指定された方向があるかどうか
    private bool hasLaunchDirection = false;

    private Rigidbody2D body;
    private Collider2D bulletCollider;


    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();
        UpdateAliveState();
    }


    // ==============================
    // 指定した方向へ弾を発射する処理
    // ==============================
    public void Launch(Vector2 launchDirection, Vector2 launchPosition)
    {
        if (launchDirection != Vector2.zero)
        {
            direction = launchDirection.normalized;
            hasLaunchDirection = true;
        }

        body.position = launchPosition;
        SetAlive(true);
    }


    public void SetAlive(bool alive)
    {
        isAlive = alive;
        UpdateAliveState();
        if (!alive)
        {
            body.linearVelocity = Vector2.zero;
        }
    }


    // ==============================
    // ゲーム開始時に1回だけ実行する処理
    // ==============================
    void Start()
    {
        // この弾に付いているSpriteRendererを取得する
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 現在の生存状態に合わせて表示・非表示を設定する
        UpdateAliveState();

        // 開始時は死んでいる状態なのでfalseにしておく
        previousIsAlive = isAlive;
    }


    // ==============================
    // 毎フレーム実行する処理
    // ==============================
    void Update()
    {
        // 現在の生存状態に応じて処理を分ける
        if (isAlive)
        {
            // 死んでいる状態から生きている状態になった瞬間か確認する
            if (!previousIsAlive)
            {
                // 明示的な発射方向がない場合はPlayerの向きを使う
                if (!hasLaunchDirection)
                {
                    SetDirectionFromPlayer();
                }

                hasLaunchDirection = false;
            }

            // Playerから30以上離れたか確認する
            CheckDistanceFromPlayer();
        }

        // 今回の生存状態を次のフレームのために記録する
        previousIsAlive = isAlive;
    }

    void FixedUpdate()
    {
        body.linearVelocity = isAlive
            ? direction * moveSpeed
            : Vector2.zero;
    }


    // ==============================
    // Playerの向きを取得する処理
    // ==============================
    void SetDirectionFromPlayer()
    {
        // タグ「Player」を持つGameObjectを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つからなかった場合は処理を終了する
        if (player == null)
        {
            return;
        }

        // PlayerについているObjectDirectionを取得する
        ObjectDirection objectDirection = player.GetComponent<ObjectDirection>();

        // ObjectDirectionが見つからなかった場合は処理を終了する
        if (objectDirection == null)
        {
            return;
        }

        // Playerの最後の移動方向を弾の方向として保存する
        direction = objectDirection.direction.normalized;
    }


    // ==============================
    // Playerから離れすぎていないか確認する処理
    // ==============================
    void CheckDistanceFromPlayer()
    {
        // タグ「Player」を持つGameObjectを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つからなかった場合は処理を終了する
        if (player == null)
        {
            return;
        }

        // BulletとPlayerの距離を計算する
        float distance = Vector3.Distance(
            transform.position,
            player.transform.position
        );

        // Playerから30以上離れているか確認する
        if (distance >= maxDistanceFromPlayer)
        {
            SetAlive(false);
        }
    }


    // ==============================
    // 弾の表示・非表示を切り替える処理
    // ==============================
    void UpdateAliveState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isAlive;
        }

        if (bulletCollider != null)
        {
            bulletCollider.enabled = isAlive;
        }
    }

   
}