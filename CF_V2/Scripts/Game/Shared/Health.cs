using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float CriticalHealthRatio = 0.3f;
        
        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public UnityAction<GameObject> OnKilledBy;

        public float CurrentHealth { get; set; }
        public bool IsDead { get; private set; }
        public bool Invincible { get; set; }

        public bool IsAlive()
        {
            return !IsDead;
        }

        public bool CanPickup()
        {
            return CurrentHealth < MaxHealth;
        }

        public float GetRatio()
        {
            return CurrentHealth / MaxHealth;
        }

        public bool IsCritical()
        {
            return GetRatio() <= CriticalHealthRatio;
        }


        void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (!Invincible && CurrentHealth > 0) 
            {
                float healthBefore = CurrentHealth;
                CurrentHealth -= damage;
                CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

                // damage
                float trueDamageAmount = healthBefore - CurrentHealth;
                if (trueDamageAmount > 0f)
                {
                    OnDamaged?.Invoke(trueDamageAmount, damageSource);
                }

                // kill
                var getKilled = HandleDeath();
                if (getKilled)
                {
                    OnKilledBy?.Invoke(damageSource);
                }
            }
        }

        /// <summary>
        /// Y Kill ...
        /// </summary>
        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        bool HandleDeath()
        {
            var gotKilled = false;

            if (!IsDead
                && CurrentHealth <= 0f)
            {
                IsDead = true;
                OnDie?.Invoke();

                gotKilled = true;
            }

            return gotKilled;
        }

        public void ResetHealth()
        {
            IsDead = false;
            CurrentHealth = MaxHealth;
        }

        // End
    }
}