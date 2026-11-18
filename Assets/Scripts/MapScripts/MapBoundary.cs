
using UnityEngine;

[ExecuteAlways]
public class MapBoundary : MonoBehaviour
{
    public SpriteRenderer mapSprite;
    public float wallThickness = 1f;

    void Update()
    {
        if (mapSprite == null) return;

        // Spriteのワールド座標上の範囲を取得
        Bounds bounds = mapSprite.bounds;

        // マップの四隅をMapBoundaryのローカル座標に変換
        Vector3 min = transform.InverseTransformPoint(bounds.min);
        Vector3 max = transform.InverseTransformPoint(bounds.max);

        float width = max.x - min.x;
        float height = max.y - min.y;

        float centerX = (min.x + max.x) / 2f;
        float centerY = (min.y + max.y) / 2f;

        SetWall("Top",
            new Vector2(centerX, max.y + wallThickness / 2),
            new Vector2(width + wallThickness * 2, wallThickness));

        SetWall("Bottom",
            new Vector2(centerX, min.y - wallThickness / 2),
            new Vector2(width + wallThickness * 2, wallThickness));

        SetWall("Left",
            new Vector2(min.x - wallThickness / 2, centerY),
            new Vector2(wallThickness, height));

        SetWall("Right",
            new Vector2(max.x + wallThickness / 2, centerY),
            new Vector2(wallThickness, height));
    }

    void SetWall(string wallName, Vector2 position, Vector2 size)
    {
        Transform wall = transform.Find(wallName);

        if (wall == null)
        {
            GameObject obj = new GameObject(wallName);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            wall = obj.transform;
        }

        wall.localPosition = position;

        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();

        if (col == null)
            col = wall.gameObject.AddComponent<BoxCollider2D>();

        col.size = size;
    }
}
