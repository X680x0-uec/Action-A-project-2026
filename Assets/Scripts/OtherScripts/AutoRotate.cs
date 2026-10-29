using UnityEngine;

// 物体を自動で回転させるスクリプト
public class AutoRotate : MonoBehaviour
{
    // 1秒間に何度回転するか
    // Inspectorから変更できます
    public float rotationSpeed = 90f;

    // 毎フレーム呼ばれる
    void Update()
    {
        // 時間経過に合わせて物体を回転させる
        // Z軸を中心に回転する
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}