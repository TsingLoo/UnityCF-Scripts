using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ProjectileStandard : ProjectileBase
    {
        [Header("General")] 
        // todo get in code
        [Tooltip("used for accurate collision detection")]
        public Transform Root;

        // front
        [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
        public Transform Tip;

        // Hit Detect
        [Header("Hit Detect")]
        //todo check if too fast, need to cast a ray from current position to next
        [Tooltip("Radius of this projectile's collision detection")]
        public float Radius = 0.01f;

        public bool DestroyedOnHit = true;

        public GameObject ImpactVfx;
        public float ImpactVfxLifetime = 5f;
        public float ImpactVfxSpawnOffset = 0.1f;

        [Tooltip("Clip to play on impact")] 
        public AudioClip ImpactSfxClip;

        [Tooltip("Layers this projectile can collide with")]
        public LayerMask HittableLayers = -1;

        // damage
        [Header("Damage")]
        [Tooltip("Damage of the projectile")]
        public float Damage = 55f;

        [Tooltip("Area of damage")]
        public DamageArea AreaOfDamage;

        [Header("Movement")] [Tooltip("Speed of the projectile")]
        public float Speed = 20f;

        [Tooltip("Downward acceleration from gravity")]
        public float GravityDownAcceleration = 0f;

        // correct to center
        [Tooltip(
            "Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
        public float TrajectoryCorrectionDistance = -1;

        [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
        public bool InheritWeaponVelocity = false;

        [Header("Debug")] 
        [Tooltip("Color of the projectile radius debug view")]
        public Color RadiusColor = Color.cyan * 0.2f;

        Vector3 m_LastRootPosition;
        Vector3 m_Velocity;
        bool m_HasTrajectoryOverride;
        float m_ShootTime;
        Vector3 m_TrajectoryCorrectionVector;
        Vector3 m_ConsumedTrajectoryCorrectionVector;

        const QueryTriggerInteraction k_TriggerInteraction = 
            QueryTriggerInteraction.Collide;

        ProjectileBase m_ProjectileBase;

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent
                <ProjectileBase, ProjectileStandard>
                (m_ProjectileBase, this, gameObject);

            m_ProjectileBase.OnShoot += OnShoot;

            Destroy(gameObject, MaxLifeTime);
        }

        void Update()
        {
            // Move
            transform.position += m_Velocity * Time.deltaTime;
            if (InheritWeaponVelocity)
            {
                transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
            }

            // Drift towards trajectory override (this is so that projectiles can be centered 
            // with the camera center even though the actual weapon is offset)
            if (m_HasTrajectoryOverride 
                && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude 
                    < m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
                float distanceThisFrame = (Root.position - m_LastRootPosition).magnitude;
                Vector3 correctionThisFrame =
                    (distanceThisFrame / TrajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
                correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
                m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

                // Detect end of correction
                if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
                {
                    m_HasTrajectoryOverride = false;
                }

                transform.position += correctionThisFrame;
            }

            // Orient towards velocity
            transform.forward = m_Velocity.normalized;

            // Gravity
            if (GravityDownAcceleration > 0)
            {
                // add gravity to the projectile velocity for ballistic effect
                m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
            }

            // Hit detection
            if(DestroyedOnHit)
            {
                #region Hit Detect
                RaycastHit closestHit = new RaycastHit();
                closestHit.distance = Mathf.Infinity;
                bool foundHit = false;

                // Sphere cast
                Vector3 displacementSinceLastFrame
                    = Tip.position - m_LastRootPosition;
                RaycastHit[] hits = Physics.SphereCastAll
                    (m_LastRootPosition,
                    Radius,
                    displacementSinceLastFrame.normalized,
                    displacementSinceLastFrame.magnitude, // todo check if correct to use this
                    HittableLayers,
                    k_TriggerInteraction);

                foreach (var hit in hits)
                {
                    if (IsHitValid(hit)
                        && hit.distance < closestHit.distance)
                    {
                        foundHit = true;
                        closestHit = hit;
                    }
                }

                if (foundHit)
                {
                    // Handle case of casting while already inside a collider
                    if (closestHit.distance <= 0f)
                    {
                        closestHit.point = Root.position;
                        closestHit.normal = -transform.forward;
                    }

                    OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                }
                #endregion
            }

            m_LastRootPosition = Root.position;
        }

        new void OnShoot()
        {
            m_ShootTime = Time.time;
            m_LastRootPosition = Root.position;
            m_Velocity = transform.forward * Speed;
            // muzzle has offset
            //transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            // Ignore colliders of owner
            m_IgnoredColliders = new List<Collider>();
            Collider[] ownerColliders = m_ProjectileBase.Owner
                .GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(ownerColliders);


            // todo check use
            return;

            // Handle case of player shooting
            // (make projectiles not go through walls, and remember center-of-screen trajectory)
            PlayerWeaponsManager playerWeaponsManager = m_ProjectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if (playerWeaponsManager)
            {
                m_HasTrajectoryOverride = true;

                Vector3 cameraToMuzzle = (m_ProjectileBase.InitialPosition -
                                          playerWeaponsManager.Camera1P_Weapon.transform.position);

                m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                    playerWeaponsManager.Camera1P_Weapon.transform.forward);
                if (TrajectoryCorrectionDistance == 0)
                {
                    transform.position += m_TrajectoryCorrectionVector;
                    m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
                }
                else if (TrajectoryCorrectionDistance < 0)
                {
                    m_HasTrajectoryOverride = false;
                }

                // todo delete?, not through wall
                //if (Physics.Raycast(playerWeaponsManager.WeaponCamera.transform.position, 
                //    cameraToMuzzle.normalized,
                //    out RaycastHit hit, 
                //    cameraToMuzzle.magnitude,
                //    HittableLayers, 
                //    k_TriggerInteraction))
                //{
                //    if (IsHitValid(hit))
                //    {
                //        OnHit(hit.point, hit.normal, hit.collider);
                //    }
                //}
            }
        }

        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            // damage area
            if (AreaOfDamage)
            {
                AreaOfDamage.HandleDamageInArea(Damage, 
                    point, 
                    HittableLayers, 
                    k_TriggerInteraction,
                    m_ProjectileBase.Owner);
            }
            else // point damage
            {
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable)
                {
                    damageable.HandleDamage(Damage, false, m_ProjectileBase.Owner);
                }
            }

            // impact vfx
            if (ImpactVfx)
            {
                GameObject impactVfxInstance = Instantiate(ImpactVfx, point + (normal * ImpactVfxSpawnOffset),
                    Quaternion.LookRotation(normal));
                if (ImpactVfxLifetime > 0)
                {
                    Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
                }
            }

            // sound
            if (ImpactSfxClip)
            {
                AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
            }

            // Self Destruct
            Destroy(this.gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = RadiusColor;
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}