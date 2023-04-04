using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        public float MaxArmor = 100f;
        public float MaxHealth = 100f;
        public float CriticalHealthRatio = 0.3f;
        
        public List<GameObject> damageSources = new List<GameObject>();
        public UnityAction<float, GameObject> onDamaged;
        public UnityAction<float> onHealed;
        public UnityAction onDie;

        public UnityAction<GameObject> onKilledBy;

        public float CurrentArmor { get; set; }
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
            InitHealth();
        }

        public void InitHealth()
        {
            CurrentArmor = MaxArmor;
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
                onHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, EDamageType damageType, GameObject damageSource)
        {
            if (!Invincible && CurrentHealth > 0) 
            {
                damageSources.Add(damageSource);

                var damagePenatrate = damage - CurrentArmor;
                if(CurrentArmor > 0)
                {
                    CurrentArmor -= damage;
                    CurrentArmor = Mathf.Clamp(CurrentArmor,  0f, MaxArmor);
                }

                if(damagePenatrate > 0f)
                {
                    ReduceHealth(damagePenatrate, damageSource);
                }
            }
        }

        private void ReduceHealth(float damage, GameObject damageSource)
        {
            if (damage <= 0)
                return;

            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // damage
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                onDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            // kill
            var getKilled = HandleDeath();
            if (getKilled)
            {
                onKilledBy?.Invoke(damageSource);
            }
        }

        /// <summary>
        /// Y Kill ...
        /// </summary>
        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            onDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        bool HandleDeath()
        {
            var gotKilled = false;

            if (!IsDead
                && CurrentHealth <= 0f)
            {
                IsDead = true;
                onDie?.Invoke();

                gotKilled = true;
            }

            return gotKilled;
        }

        // respawn
        public void ResetHealth()
        {
            IsDead = false;
            CurrentArmor = MaxArmor;
            CurrentHealth = MaxHealth;
        }

        // End
    }
}