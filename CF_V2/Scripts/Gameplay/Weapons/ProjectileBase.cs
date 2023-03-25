using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        public GameObject Owner { get; private set; }

        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        
        public float InitialForce { get; private set; }
        public float InitialCharge { get; private set; }

        protected float MaxLifeTime = 10f;

        protected List<Collider> m_IgnoredColliders;

        /// <summary>
        /// virtual
        /// </summary>
        public UnityAction OnShoot;

        public void Shoot(WeaponController weapon)
        {
            Owner = weapon.Owner;

            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = weapon.MuzzleWorldVelocity;
            InitialCharge = weapon.CurrentCharge;
            InitialForce = weapon.ProjectileForce;

            OnShoot?.Invoke();
        }

        protected bool IsHitValid(RaycastHit hit)
        {
            // ignore hits with an ignore component
            if (hit.collider.GetComponent<IgnoreHitDetection>())
            {
                return false;
            }

            // ignore hits with triggers that don't have a Damageable component
            if (hit.collider.isTrigger
                && hit.collider.GetComponent<Damageable>() == null)
            {
                return false;
            }

            // self collider
            if (m_IgnoredColliders != null
                && m_IgnoredColliders.Contains(hit.collider))
            {
                return false;
            }

            return true;
        }

    }
}