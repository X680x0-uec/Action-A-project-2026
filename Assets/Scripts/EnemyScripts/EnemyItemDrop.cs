using UnityEngine;

// 敵が死亡した位置にアイテムを生成するためのスクリプト
public class EnemyItemDrop : MonoBehaviour
{
    // 生成するアイテムのPrefabをInspectorから設定する
    public GameObject itemPrefab;

    [Range(0f, 1f)]
    public float dropChance = 0.1f;

    // 指定された位置にアイテムを生成する
    public void DropItem(Vector3 dropPosition)
    {
        if (Random.value >= Mathf.Clamp01(dropChance))
        {
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("Item Prefabが設定されていません。", this);
            return;
        }

        GameObject itemsParent = GameObject.Find("PoolObjects/items");
        Transform parentTransform = itemsParent != null
            ? itemsParent.transform
            : null;

        if (parentTransform == null)
        {
            Debug.LogWarning("HierarchyにPoolObjects/itemsが見つかりません。", this);
        }

        GameObject droppedItem = Instantiate(
            itemPrefab,
            dropPosition,
            Quaternion.identity,
            parentTransform
        );

        if (droppedItem.GetComponent<ItemPickup>() == null)
        {
            droppedItem.AddComponent<ItemPickup>();
        }
    }
}