using System;
using UnityEngine;

namespace alpha.ingame.enemy
{
    /// <summary>적의 체력, 피격, 사망을 단독으로 관리한다.</summary>
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float _maxHealth = 100f;

        public float MaxHealth => Mathf.Max(1f, _maxHealth);
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<float> Damaged;
        public event Action Died;

        private void Awake() => CurrentHealth = MaxHealth;

        public void ApplyDamage(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(amount);
            if (IsDead) Died?.Invoke();
        }
    }
}
