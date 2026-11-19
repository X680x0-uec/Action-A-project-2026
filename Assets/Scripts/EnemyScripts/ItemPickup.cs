using UnityEngine;

// Playerが接触したアイテムを取得するためのスクリプト
public class ItemPickup : MonoBehaviour
{
    public WeaponUpgradeType upgradeType = WeaponUpgradeType.Bullet;

    private bool isCollected;

    public void SetUpgradeType(WeaponUpgradeType type)
    {
        upgradeType = type;
        SpriteRenderer itemRenderer = GetComponent<SpriteRenderer>();
        if (itemRenderer != null)
        {
            itemRenderer.color = type == WeaponUpgradeType.Sword
                ? new Color(1f, 0.55f, 0.25f)
                : new Color(0.35f, 0.75f, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        TryCollect(other.gameObject);
    }

    void TryCollect(GameObject other)
    {
        if (isCollected || !other.CompareTag("Player"))
        {
            return;
        }

        BulletManager bulletManager = BulletManager.Instance;
        if (bulletManager == null)
        {
            Debug.LogWarning("BulletManagerが見つからないため、アイテム強化を開始できません。", this);
            return;
        }

        isCollected = true;
        Debug.Log("アイテムゲット！");
        bulletManager.OpenUpgradeSelection(upgradeType);
        Destroy(gameObject);
    }
}