using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class MeleeCollider: MonoBehaviour
    {
        public float Damage = 35;

        GameObject _ownerPawn;

        [HideInInspector] public Collider hitCollider;

        void Start()
        {
            // todo set _ownerPawn

            SetCollider();

            // todo set damage
        }

        private void SetCollider()
        {
            hitCollider = GetComponentInChildren<Collider>();
            hitCollider.isTrigger = true;
            hitCollider.enabled = false;

            // todo both way is not effecting enemy layer
            // https://docs.unity3d.com/Manual/LayerBasedCollision.html
            //Physics.IgnoreLayerCollision(AllLayers.Weapon1P.GetValue(), 
            //    AllLayers.Player3P.GetValue());
        }

        private void OnTriggerEnter(Collider collider)
        {
            Damageable damageable = collider.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.HandleDamage(Damage,  EDamageType.Melee, _ownerPawn);
            }
        }

        internal void EnableCollider()
        {
            hitCollider.enabled = true;
        }

        internal void DisableCollider()
        {
            hitCollider.enabled = false;
        }
    }
}
