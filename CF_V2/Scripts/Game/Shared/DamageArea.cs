using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class DamageArea : MonoBehaviour
    {
        [Tooltip("Area of damage when the projectile hits something")]
        public float AreaOfEffectDistance = 5f;

        [Tooltip("Damage multiplier over distance for area of effect")]
        public AnimationCurve DamageRatioOverDistance;

        [Header("Debug")] 
        [Tooltip("Color of the area of effect radius")]
        public Color AreaOfEffectColor = Color.red * 0.5f;

        public void HandleDamageInArea(float damage, 
            Vector3 center, 
            LayerMask layers,
            QueryTriggerInteraction interaction, 
            GameObject owner)
        {
            // get list to be damaged
            Dictionary<Health, Damageable> uniqueDamagedHealths 
                = new Dictionary<Health, Damageable>();

            Collider[] affectedColliders = Physics
                .OverlapSphere(center, 
                AreaOfEffectDistance, 
                layers, 
                interaction);

            foreach (var coll in affectedColliders)
            {
                Damageable damageable = coll.GetComponent<Damageable>();
                if (damageable)
                {
                    Health health = damageable.GetComponentInParent<Health>();
                    if (health && !uniqueDamagedHealths.ContainsKey(health))
                    {
                        uniqueDamagedHealths.Add(health, damageable);
                    }
                }
            }

            // Apply damages with distance falloff
            foreach (Damageable uniqueDamageable in uniqueDamagedHealths.Values)
            {
                float distance = Vector3
                    .Distance(uniqueDamageable.transform.position, 
                    transform.position);

                // todo no curve
                uniqueDamageable.HandleDamage
                    (damage * DamageRatioOverDistance.Evaluate(distance / AreaOfEffectDistance), 
                     EDamageType.Bomb, // todo get from weapon
                    owner);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = AreaOfEffectColor;
            Gizmos.DrawSphere(transform.position, AreaOfEffectDistance);
        }
    }
}