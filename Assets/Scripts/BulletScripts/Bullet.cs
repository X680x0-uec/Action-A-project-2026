using UnityEngine;

// 弾1個を管理するためのスクリプト
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


    // ==============================
    // 指定した方向へ弾を発射する処理
    // ==============================
    public void Launch(Vector2 launchDirection)
    {
        if (launchDirection != Vector2.zero)
        {
            direction = launchDirection.normalized;
            hasLaunchDirection = true;
        }

        isAlive = true;
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
            // 弾が生きている場合は表示する
            UpdateAliveState();

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

            // 弾を移動させる
            MoveBullet();

            // Playerから30以上離れたか確認する
            CheckDistanceFromPlayer();
        }
        else
        {
            // 弾が死んでいる場合は非表示にする
            UpdateAliveState();
        }

        // 今回の生存状態を次のフレームのために記録する
        previousIsAlive = isAlive;
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
    // 弾を一定方向へ移動させる処理
    // ==============================
    void MoveBullet()
    {
        // 保存しておいた方向へ弾を移動させる
        // Time.deltaTimeを掛けることで、
        // 「1秒あたりmoveSpeedだけ進む」動きになる
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
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
            // Bulletを死んでいる状態にする
            isAlive = false;

            // Bulletを非表示にする
            UpdateAliveState();
        }
    }


    // ==============================
    // 弾の表示・非表示を切り替える処理
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

   
}