using UnityEngine;

// 物体の色をゲーミング風に変化させるスクリプト
public class GamingColor : MonoBehaviour
{
    // 色が変化する速さ
    // Inspectorから変更できます
    public float colorChangeSpeed = 1.0f;

    // Rendererを取得
    private Renderer objectRenderer;

    // 色の変化に使う値
    private float hue = 0.0f;

    void Start()
    {
        // この物体のRendererを取得
        objectRenderer = GetComponent<Renderer>();

        // 最初に色を白にリセットする
        // 元々設定されていた色の影響を受けないようにする
        objectRenderer.material.color = Color.white;

        // 色の変化を最初から開始
        hue = 0.0f;
    }

    void Update()
    {
        // 時間経過に合わせて色相を変化させる
        hue += colorChangeSpeed * Time.deltaTime;

        // 0～1の範囲で繰り返す
        if (hue > 1.0f)
        {
            hue -= 1.0f;
        }

        // HSVからRGBに変換
        // 彩度1、明度1なので鮮やかな色になる
        Color newColor = Color.HSVToRGB(hue, 1.0f, 1.0f);

        // 物体の色を変更
        objectRenderer.material.color = newColor;
    }
}