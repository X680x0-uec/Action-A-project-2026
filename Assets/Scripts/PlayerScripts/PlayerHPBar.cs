using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    // HPの最大値。
    // Inspectorから変更できるようにpublicにする。
    public int maxHP = 10;

    // ゲーム開始時のHP。
    // Inspectorから変更できるようにpublicにする。
    public int initialHP = 10;

    // Sliderコンポーネントを保存する変数。
    // HPの残量を画面上に表示するために使用する。
    private Slider slider;

    // 現在のHPを保存する変数。
    private int currentHP;


    // ゲーム開始時に1回だけ呼ばれる。
    void Start()
    {
        // HPBar自身についているSliderコンポーネントを取得する。
        slider = GetComponent<Slider>();

        // 現在HPを初期HPにする。
        currentHP = initialHP;

        // Sliderの最小値を0にする。
        slider.minValue = 0;

        // Sliderの最大値を最大HPにする。
        slider.maxValue = maxHP;

        // Sliderに現在HPを設定する。
        slider.value = currentHP;
    }


    // 外部からHPを減らすための関数。
    // 敵がプレイヤーに接触したときなどに呼び出す。
    public void TakeDamage(int damage)
    {
        // 現在HPから受けたダメージを引く。
        currentHP -= damage;

        // HPが0未満にならないようにする。
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        // HPバーの表示を現在HPに合わせて変更する。
        slider.value = currentHP;

        // 現在HPが0になったら死亡処理を行う。
        if (currentHP <= 0)
        {
            // 現時点では、コンソールに表示するだけ。
            Debug.Log("プレイヤーのHPが0になりました。");
        }
    }
}