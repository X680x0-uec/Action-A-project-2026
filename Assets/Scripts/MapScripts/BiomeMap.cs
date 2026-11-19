using UnityEngine;

// バイオームマスクのピクセル色を使って、ワールド座標上のバイオームを判定する
public class BiomeMap : MonoBehaviour
{
    [Header("バイオーム判定に使用する画像")]
    // 赤チャンネルの値をバイオームIDとして使うマスク画像。
    // ピクセルを読み取るため、InspectorでRead/Writeを有効にする。
    [SerializeField] private Texture2D biomeMask;

    // ワールド座標とマスク画像の座標を対応させるマップ画像のSpriteRenderer。
    [SerializeField] private SpriteRenderer mapSpriteRenderer;

    // 起動時に一度だけ読み込む全ピクセル。参照時にTexture2Dから再取得しない。
    private Color32[] pixels;

    // マスク画像の幅と高さ。ワールド座標をピクセル座標に変換する際に使う。
    private int maskWidth;
    private int maskHeight;

    // 画像情報の読み込みが成功したかを示す。失敗時は判定を行わない。
    private bool isInitialized;

    private void Awake()
    {
        // GetPixels32は画像全体を取得するため、ゲーム開始時に一度だけ実行する。
        Initialize();
    }

    private void Initialize()
    {
        // 必須参照の不足をログに出し、無効な状態での画像アクセスを防ぐ。
        if (biomeMask == null)
        {
            Debug.LogError("BiomeMap: Biome Maskが設定されていません。", this);
            return;
        }

        if (mapSpriteRenderer == null)
        {
            Debug.LogError("BiomeMap: Map Sprite Rendererが設定されていません。", this);
            return;
        }

        // Read/Writeが無効な画像はピクセルデータをCPU側から読めない。
        if (!biomeMask.isReadable)
        {
            Debug.LogError("BiomeMap: Biome MaskのRead/Writeを有効にしてください。", this);
            return;
        }

        // 画像サイズとピクセル配列を保存し、以降の判定は配列参照だけで行う。
        maskWidth = biomeMask.width;
        maskHeight = biomeMask.height;
        pixels = biomeMask.GetPixels32();
        isInitialized = pixels.Length == maskWidth * maskHeight;

        // 想定外のデータ長なら、後続処理で配列範囲外アクセスを起こさないようにする。
        if (!isInitialized)
        {
            Debug.LogError("BiomeMap: Biome Maskのピクセル情報を読み込めませんでした。", this);
        }
    }

    // マップの左下と右上をマスク画像の左下と右上に対応させ、指定位置のIDを返す。
    // 画像の下端・左端は0、上端・右端は最後のピクセルとして扱う。
    // 範囲外または初期化失敗時は無効なIDとして-1を返す。
    public int GetBiomeId(Vector3 worldPosition)
    {
        if (!isInitialized || mapSpriteRenderer == null)
        {
            return -1;
        }

        // SpriteRenderer.boundsはSpriteのワールド空間上の軸平行範囲を返す。
        // マップの回転や変形ではなく、位置・大きさの変更に対応する。
        Bounds mapBounds = mapSpriteRenderer.bounds;
        Vector3 min = mapBounds.min;
        Vector3 size = mapBounds.size;

        // 0サイズの範囲では座標変換できず、範囲外の位置もピクセルに対応しない。
        if (size.x <= 0f || size.y <= 0f ||
            worldPosition.x < min.x || worldPosition.x > mapBounds.max.x ||
            worldPosition.y < min.y || worldPosition.y > mapBounds.max.y)
        {
            return -1;
        }

        // マップ内の位置を0～1の割合に直し、その割合を画像の幅・高さに掛ける。
        float normalizedX = (worldPosition.x - min.x) / size.x;
        float normalizedY = (worldPosition.y - min.y) / size.y;
        int pixelX = Mathf.Min(Mathf.FloorToInt(normalizedX * maskWidth), maskWidth - 1);
        int pixelY = Mathf.Min(Mathf.FloorToInt(normalizedY * maskHeight), maskHeight - 1);

        // GetPixels32の配列は左下から横方向に並ぶため、行番号×幅＋列番号で位置を求める。
        int pixelIndex = pixelY * maskWidth + pixelX;

        // 指定仕様に合わせてColor32の赤チャンネルをバイオームIDとして返す。
        return pixels[pixelIndex].r;
    }

    // マップ情報を利用できる場合に、位置がマップ範囲外かを返す。
    public bool IsPositionOutsideMap(Vector3 worldPosition)
    {
        if (!isInitialized || mapSpriteRenderer == null)
        {
            return false;
        }

        Bounds mapBounds = mapSpriteRenderer.bounds;
        Vector3 min = mapBounds.min;
        Vector3 size = mapBounds.size;
        if (size.x <= 0f || size.y <= 0f)
        {
            return false;
        }

        return worldPosition.x < min.x || worldPosition.x > mapBounds.max.x ||
            worldPosition.y < min.y || worldPosition.y > mapBounds.max.y;
    }
}
