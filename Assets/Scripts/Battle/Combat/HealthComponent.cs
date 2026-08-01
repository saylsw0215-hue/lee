using System;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Owns clamped health state and guarantees a single death notification.</summary>
    public sealed class HealthComponent : MonoBehaviour
    {
        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> Died;
        public event Action<DamageInfo> Damaged;
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        private bool deathRaised;

        public void Initialize(float maxHealth)
        {
            MaxHealth = Mathf.Max(.01f, maxHealth); CurrentHealth = MaxHealth; deathRaised = false;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(DamageInfo info)
        {
            ApplyDamage(info,info.Amount);
        }

        public float ApplyDamage(DamageInfo info,float amount)
        {
            if (!IsAlive || amount <= 0f) return 0f;
            float before=CurrentHealth;CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(info); HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            if (CurrentHealth <= 0f && !deathRaised) { deathRaised = true; Died?.Invoke(info); }
            return before-CurrentHealth;
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount); HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
