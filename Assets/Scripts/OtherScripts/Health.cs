using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [SerializeField, Min(1), InspectorName("Max HP")]
    private int maxHP = 100;

    [SerializeField, Min(0), InspectorName("Current HP")]
    private int currentHP = 100;

    private int initialHP;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDepleted => currentHP == 0;

    public event Action<Health> HealthChanged;
    public event Action<Health> Depleted;

    void Awake()
    {
        ClampValues();
        initialHP = currentHP;
    }

    void OnValidate()
    {
        ClampValues();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetCurrentHP(currentHP - amount);
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

    public void SetCurrentHP(int value)
    {
        int clampedValue = Mathf.Clamp(value, 0, maxHP);
        if (currentHP == clampedValue)
        {
            return;
        }

        bool wasAlive = currentHP > 0;
        currentHP = clampedValue;
        HealthChanged?.Invoke(this);

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
