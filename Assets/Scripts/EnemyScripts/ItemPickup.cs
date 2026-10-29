using UnityEngine;

// Playerが接触したアイテムを取得するためのスクリプト
public class ItemPickup : MonoBehaviour
{
    private bool isCollected;

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
        bulletManager.OpenUpgradeSelection();
        Destroy(gameObject);
    }
}