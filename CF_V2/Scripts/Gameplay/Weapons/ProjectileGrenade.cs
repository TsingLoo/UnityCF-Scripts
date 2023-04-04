using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ProjectileGrenade : ProjectileBase
    {
        [Header("Grenade")]
        public bool DestroyedOnHit = false;
        public float ExplodeTimer = 3.5f;
        public LayerMask HittableLayers = -1;

        [Header("Launcher")]
        public bool UpdateDirection = false;

        [Header("Damage")]
        public float Damage = 135f;
        public float ReachRadius = 2.0f;
        public EDamageType damageType = EDamageType.Grenade;
        public AudioClip DestroyedSound;
        public GameObject EffectPrefab;

        static Collider[] _sphereCastPool = new Collider[32];

        Rigidbody m_Rigidbody;
        Collider m_Collider;

        ProjectileBase m_ProjectileBase;

        void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
        }

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent
                <ProjectileBase, ProjectileGrenade>
                (m_ProjectileBase, this, gameObject);

            m_ProjectileBase.OnShoot += OnShoot;

            Destroy(gameObject, MaxLifeTime);
        }

        private void Start()
        {
            StartCoroutine(ExplosionTimer());
        }

        private IEnumerator ExplosionTimer()
        {
            yield return new WaitForSeconds(ExplodeTimer);

            Explode();
        }

        private void FixedUpdate()
        {
            if (UpdateDirection)
            {
                this.transform.forward =
                    m_Rigidbody.velocity.normalized;
            }
        }

        // Launch
        new void OnShoot()
        {
            gameObject.SetActive(true);

            m_Rigidbody.AddForce(transform.forward * InitialForce);
        }

        void OnCollisionEnter(Collision other)
        {
            if (!HittableLayers.Contains(other.gameObject.layer))
            {
                Physics.IgnoreCollision(m_Collider, other.collider);
            }
            else if (DestroyedOnHit)
            {
                Explode();
            }
        }

        void Explode()
        {
            Vector3 position = transform.position;

            // effect
            var effect = Instantiate(EffectPrefab,
                transform.position,
                Quaternion.identity);
            effect.SetActive(true);

            // hit
            int count = Physics.OverlapSphereNonAlloc
                (position, ReachRadius, _sphereCastPool,
                HittableLayers);

            for (int i = 0; i < count; ++i)
            {
                Damageable damageable = _sphereCastPool[i]
                    .GetComponent<Damageable>();
                if (damageable)
                {
                    damageable.HandleDamage(Damage,
                        EDamageType.Grenade,
                        m_ProjectileBase.Owner);
                }
            }

            // audio
            AudioUtility.CreateSFX(
                DestroyedSound, 
                position, 
                AudioUtility.AudioGroups.Impact,
                1f, 
                3f);

            this.SelfDestroy();
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, ReachRadius);
        }
    }
}