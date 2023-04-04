using UnityEngine;

namespace Unity.FPS.Game
{
    public class Damageable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Range(0, 1)] [Tooltip("Multiplier to apply to self damage")]
        public float SensibilityToSelfdamage = 0.5f;

        public Health Health { get; private set; }
        public NanoHealth NanoHealth { get; private set; }

        void Awake()
        {
            // find the health component either at the same level, or higher in the hierarchy
            Health = GetComponent<Health>();
            if (!Health)
            {
                Health = GetComponentInParent<Health>();
            }
            
            NanoHealth = GetComponent<NanoHealth>();
            if (NanoHealth == null)
            {
                NanoHealth= GetComponentInParent<NanoHealth>();
            }
        }

        public void HandleDamage(float damage, EDamageType damageType, GameObject damageSource)
        {
            if(damageType == EDamageType.Nano
                && NanoHealth && NanoHealth.IsAlive())
            {
                NanoHealth.TakeDamage(damage, damageType, damageSource);
            }
            else if (Health && Health.IsAlive())
            {
                var totalDamage = damage;

                // skip the crit multiplier if it's from an explosion
                if (damageType != EDamageType.Bomb)
                {
                    totalDamage *= DamageMultiplier;
                }

                // self damage
                if (Health.gameObject == damageSource)
                {
                    totalDamage *= SensibilityToSelfdamage;
                }

                // apply damage
                Health.TakeDamage(totalDamage, damageType, damageSource);
            }
        }

        //
    }
}