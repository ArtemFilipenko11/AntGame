using System;
using UnityEngine;

namespace AntGame.Combat
{
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)]
        private float maxHealth = 100f;

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth > 0f)
                return;

            isDead = true;
            Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f || currentHealth >= maxHealth)
                return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

            if (!Mathf.Approximately(previousHealth, currentHealth))
                HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
