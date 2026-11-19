using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SwordAttack : MonoBehaviour
{
    public int damage = 10;

    private readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
    private Transform owner;
    private Rigidbody2D body;
    private BoxCollider2D hitbox;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float swingDuration;
    private float swingArc;
    private float orbitRadius;
    private float startAngle;
    private float elapsed;
    private bool isSwinging;

    public void Initialize(Transform player, int initialDamage, float initialSize)
    {
        owner = player;
        damage = initialDamage;
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        SetSizeMultiplier(initialSize);
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        hitbox.isTrigger = true;
        hitbox.enabled = false;
        if (spriteRenderer != null)
        {
            if (playerRenderer != null)
            {
                spriteRenderer.sortingLayerID = playerRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
            }
            spriteRenderer.enabled = false;
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(1, newDamage);
    }

    public void SetSizeMultiplier(float multiplier)
    {
        transform.localScale = baseScale * Mathf.Max(0.01f, multiplier);
    }

    public void BeginSwing(Vector2 facing, float duration, float arc, float radius)
    {
        if (owner == null || hitbox == null)
        {
            return;
        }

        facing = facing.sqrMagnitude > 0f ? facing.normalized : Vector2.down;
        startAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f - arc * 0.5f;
        swingDuration = Mathf.Max(0.01f, duration);
        swingArc = arc;
        orbitRadius = Mathf.Max(0f, radius);
        elapsed = 0f;
        hitEnemies.Clear();
        isSwinging = true;

        gameObject.SetActive(true);
        SetSwingPose(0f);
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        hitbox.enabled = true;
    }

    void Update()
    {
        if (!isSwinging)
        {
            return;
        }

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / swingDuration);
        SetSwingPose(progress);

        if (progress >= 1f)
        {
            isSwinging = false;
            hitbox.enabled = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            gameObject.SetActive(false);
        }
    }

    void SetSwingPose(float progress)
    {
        float angle = startAngle + swingArc * progress;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = owner.position + rotation * Vector3.up * orbitRadius;
        transform.rotation = rotation;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || !enemy.isAlive || !hitEnemies.Add(enemy))
        {
            return;
        }

        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth == null)
        {
            Debug.LogError("Enemy requires a Health component.", enemy);
            return;
        }

        int damageDealt = enemyHealth.TakeDamage(damage);
        if (GameHud.Instance != null)
        {
            GameHud.Instance.AddSwordDamage(damageDealt);
        }
    }
}
