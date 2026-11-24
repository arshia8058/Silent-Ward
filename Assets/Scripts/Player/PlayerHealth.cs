using System;
using UnityEngine;

namespace SilentWard.Player
{
    /// <summary>Health + Focus (mental stability). No permanent auto-regeneration:
    /// health returns only via meds/first-aid; focus recovers slowly only in safe, lit areas.</summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxFocus = 100f;
        public float Health { get; private set; }
        public float Focus { get; private set; }
        public bool IsDead => Health <= 0f;
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float, float> OnFocusChanged;
        public event Action OnDied;

        private void Awake() { Health = maxHealth; Focus = maxFocus; }

        public void Restore(float health, float focus)
        {
            Health = Mathf.Clamp(health, 0, maxHealth); Focus = Mathf.Clamp(focus, 0, maxFocus);
            OnHealthChanged?.Invoke(Health, maxHealth); OnFocusChanged?.Invoke(Focus, maxFocus);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            Health = Mathf.Max(0, Health - amount);
            OnHealthChanged?.Invoke(Health, maxHealth);
            if (IsDead) OnDied?.Invoke();
        }

        /// <summary>Psychological threats drain focus instead of health.</summary>
        public void DrainFocus(float amount)
        {
            Focus = Mathf.Max(0, Focus - amount);
            OnFocusChanged?.Invoke(Focus, maxFocus);
        }

        public bool Heal(float amount)
        {
            if (IsDead || Health >= maxHealth) return false;
            Health = Mathf.Min(maxHealth, Health + amount);
            OnHealthChanged?.Invoke(Health, maxHealth);
            return true;
        }

        public void AddMaxHealth(float amount) { maxHealth += amount; OnHealthChanged?.Invoke(Health, maxHealth); }
    }
}
