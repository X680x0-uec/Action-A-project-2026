using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [SerializeField, Min(1), InspectorName("Max HP")]
    private int maxHP = 100;

    [SerializeField, Min(0), InspectorName("Current HP")]
    private int currentHP = 100;

    [Header("Player Respawn Buffs")]
    [SerializeField, Min(0), InspectorName("Max HP Increase Per Respawn")]
    private int maxHPIncreasePerRespawn = 5;

    [SerializeField, Min(0f), InspectorName("Move Speed Increase Per Respawn")]
    private float moveSpeedIncreasePerRespawn = 1f;

    [SerializeField, Min(0f), InspectorName("Respawn Message Duration (seconds)")]
    private float respawnNoticeDuration = 2f;

    private int initialHP;
    private int respawnCount;
    private float killMoveSpeedBonus;
    private Vector3 initialPosition;
    private MoveWASD wasdMovement;
    private MoveARROWS arrowsMovement;
    private float initialWASDMoveSpeed;
    private float initialArrowsMoveSpeed;
    private float respawnNoticeRemaining;
    private GUIStyle respawnNoticeStyle;
    private bool managesPlayerRespawn;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDepleted => currentHP == 0;
    public int RespawnCount => respawnCount;

    public event Action<Health> HealthChanged;
    public event Action<Health> Depleted;

    void Awake()
    {
        ClampValues();
        initialHP = currentHP;

        managesPlayerRespawn = CompareTag("Player");
        if (managesPlayerRespawn)
        {
            initialPosition = transform.position;
            wasdMovement = GetComponent<MoveWASD>();
            arrowsMovement = GetComponent<MoveARROWS>();
            if (wasdMovement != null)
            {
                initialWASDMoveSpeed = wasdMovement.moveSpeed;
            }
            if (arrowsMovement != null)
            {
                initialArrowsMoveSpeed = arrowsMovement.moveSpeed;
            }
            Depleted += RespawnPlayer;
        }
    }

    void OnDestroy()
    {
        if (managesPlayerRespawn)
        {
            Depleted -= RespawnPlayer;
        }
    }

    void Update()
    {
        if (respawnNoticeRemaining > 0f)
        {
            respawnNoticeRemaining -= Time.unscaledDeltaTime;
        }
    }

    void OnGUI()
    {
        if (!managesPlayerRespawn || respawnNoticeRemaining <= 0f)
        {
            return;
        }

        if (respawnNoticeStyle == null)
        {
            respawnNoticeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold
            };
            respawnNoticeStyle.normal.textColor = Color.white;
        }

        GUI.Label(
            new Rect(0f, Screen.height * 0.35f, Screen.width, 72f),
            "Respawn",
            respawnNoticeStyle);
    }

    void OnValidate()
    {
        ClampValues();
    }

    public int TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int actualDamage = Mathf.Min(amount, currentHP);
        SetCurrentHP(currentHP - amount);
        return actualDamage;
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetCurrentHP(amount >= maxHP - currentHP ? maxHP : currentHP + amount);
    }

    public void ResetToInitial()
    {
        SetCurrentHP(initialHP);
    }

    void RespawnPlayer(Health depletedHealth)
    {
        respawnCount++;

        int maxHPIncrease = Mathf.Max(0, maxHPIncreasePerRespawn);
        if (maxHPIncrease > 0)
        {
            maxHP = (int)Math.Min(int.MaxValue, (long)maxHP + maxHPIncrease);
            HealthChanged?.Invoke(this);
        }

        ApplyPlayerMoveSpeedBonuses();

        transform.position = initialPosition;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }

        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.AddRespawnAttackDamage();
        }
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.DespawnAllAliveEnemies();
        }

        respawnNoticeRemaining = respawnNoticeDuration;
        SetCurrentHP(maxHP);
    }

    public void AddKillMoveSpeedBonus(float amount)
    {
        if (!managesPlayerRespawn || amount <= 0f)
        {
            return;
        }

        killMoveSpeedBonus += amount;
        ApplyPlayerMoveSpeedBonuses();
    }

    void ApplyPlayerMoveSpeedBonuses()
    {
        float moveSpeedIncrease =
            moveSpeedIncreasePerRespawn * respawnCount + killMoveSpeedBonus;
        if (wasdMovement != null)
        {
            wasdMovement.moveSpeed = initialWASDMoveSpeed + moveSpeedIncrease;
        }
        if (arrowsMovement != null)
        {
            arrowsMovement.moveSpeed = initialArrowsMoveSpeed + moveSpeedIncrease;
        }
    }

    public void SetCurrentHP(int value)
    {
        int clampedValue = Mathf.Clamp(value, 0, maxHP);
        if (currentHP == clampedValue)
        {
            return;
        }

        bool wasAlive = currentHP > 0;
        int damageAmount = Mathf.Max(0, currentHP - clampedValue);
        currentHP = clampedValue;
        HealthChanged?.Invoke(this);

        if (damageAmount > 0)
        {
            EffectManager.Instance.ShowDamageNumber(this, damageAmount);
        }

        if (wasAlive && currentHP == 0)
        {
            Depleted?.Invoke(this);
        }
    }

    void ClampValues()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }
}
