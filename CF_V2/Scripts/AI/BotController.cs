using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// BotPawn
    /// </summary>
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class BotController : PawnController
    {
        #region Sub Class

        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }
        #endregion

        [Header("Parameters")]
        public float SelfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float PathReachingRadius = 2f;

        // body turn
        public float RotateSpeed = 10f;

        public float DeathDuration = 3f;

        // todo low health
        [Header("Flash on hit")]
        [Tooltip("The material used for the body of the hoverbot")]
        public Material BodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")]
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float FlashOnHitDuration = 0.5f;

        [Header("VFX")]
        [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Header("Loot")]
        [Tooltip("The object this enemy can drop when dying")]
        public GameObject LootPrefab;

        [Tooltip("The chance the object has to drop")]
        [Range(0, 1)]
        public float DropRate = 1f;

        [Header("Debug Display")]
        [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color PathReachingRangeColor = Color.yellow;

        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;

        List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
        MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
        float m_LastTimeDamaged = float.NegativeInfinity;

        public PatrolPath PatrolPath { get; set; }
        public GameObject KnownTarget
        {
            get
            {
                return BotView.KnownTarget;
            }
        }

        public bool IsTargetInAttackRange => BotView.IsTargetInAttackRange;
        public bool IsSeeingTarget => BotView.IsSeeingTarget;
        public bool HadKnownTarget => BotView.HadKnownTarget;
        public NavMeshAgent NavMeshAgent { get; private set; }
        public BotView BotView { get; private set; }

        int m_PathDestinationNodeIndex;
        BotManager _botManager;
        ActorsManager m_ActorsManager;

        Collider[] m_SelfColliders;
        bool m_WasDamagedThisFrame;

        GameFlowManager m_GameFlowManager;
        NavigationModule m_NavigationModule;

        protected override void Init()
        {
            base.Init();

            // todo 
            PawnName = GUID.Generate().ToString();

            NavMeshAgent = GetComponent<NavMeshAgent>();
            m_SelfColliders = GetComponentsInChildren<Collider>();

            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, BotController>(m_ActorsManager, this);

            _botManager = FindObjectOfType<BotManager>();
            DebugUtility.HandleErrorIfNullFindObject<BotManager, BotController>(_botManager, this);
        }

        protected override void Start()
        {
            base.Start();

            // Subscribe to damage & death actions
            m_Health.OnDie += OnDie;
            m_Health.OnDamaged += OnDamaged;

            _botManager.RegisterBot(this);

            m_GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, BotController>(m_GameFlowManager, this);

            // detect module
            var detectionModules = GetComponentsInChildren<BotView>();
            DebugUtility.HandleErrorIfNoComponentFound<BotView, BotController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<BotView, BotController>(detectionModules.Length,
                this, gameObject);

            // Initialize detection module
            BotView = detectionModules[0];
            BotView.onDetectedTarget += OnDetectedTarget;
            BotView.onLostTarget += OnLostTarget;
            onAttack += BotView.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<BotView, BotController>(detectionModules.Length,
                this, gameObject);

            // Override navmesh agent data
            if (navigationModules.Length > 0)
            {
                m_NavigationModule = navigationModules[0];
                NavMeshAgent.speed = m_NavigationModule.MoveSpeed; // todo pawn speed
                NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
                NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == BodyMaterial)
                    {
                        m_BodyRenderers.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            // Eye Color
        }

        protected override void Update()
        {
            base.Update();

            EnsureIsWithinLevelBounds();

            BotView.HandleTargetDetection(m_Actor, m_SelfColliders);

            //UpdateOnHitColor();

            m_WasDamagedThisFrame = false;
        }

        private void UpdateOnHitColor()
        {
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
            m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in m_BodyRenderers)
            {
                data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
            }
        }

        // todo base
        void EnsureIsWithinLevelBounds()
        {
            // at every frame, this tests for conditions to kill the enemy
            if (transform.position.y < SelfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnLostTarget()
        {
            onLostTarget.Invoke();

            // stop vfx
            // OnDetectVfx[i].Stop()
        }

        void OnDetectedTarget()
        {
            onDetectedTarget.Invoke();

            // sound vfx
            // AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            // OnDetectVfx[i].Play()
        }

        public void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotateSpeed);
            }
        }

        bool IsPathValid()
        {
            return PatrolPath && PatrolPath.PathNodes.Count > 0;
        }

        public void ResetPathDestination()
        {
            m_PathDestinationNodeIndex = 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                int closestPathNodeIndex = 0;
                for (int i = 0; i < PatrolPath.PathNodes.Count; i++)
                {
                    float distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                    {
                        closestPathNodeIndex = i;
                    }
                }

                m_PathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                m_PathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
            {
                return PatrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex);
            }
            else
            {
                return transform.position;
            }
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent)
            {
                NavMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                // Check if reached the path destination
                if ((transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
                {
                    // increment path destination index
                    m_PathDestinationNodeIndex =
                        inverseOrder ? (m_PathDestinationNodeIndex - 1) : (m_PathDestinationNodeIndex + 1);
                    if (m_PathDestinationNodeIndex < 0)
                    {
                        m_PathDestinationNodeIndex += PatrolPath.PathNodes.Count;
                    }

                    if (m_PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
                    {
                        m_PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
                    }
                }
            }
        }

        protected override void OnDamaged(float damage, GameObject damageSource)
        {
            base.OnDamaged(damage, damageSource);

            if (damageSource && !damageSource.GetComponent<BotController>())
            {
                // move to the player
                BotView.OnDamaged(damageSource);

                onDamaged?.Invoke();

                m_LastTimeDamaged = Time.time;
                // play the damage tick sound
                if (!m_WasDamagedThisFrame)
                {
                    // todo damage sound in weapon con
                    //var clip = GetClip(AllClips.PLAYER_GUNHIT_SCREAM);

                    //AnimController.PlayOneShot(clip);
                    // todo fix
                    //AudioUtility.CreateSFX(clip, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
                }

                m_WasDamagedThisFrame = true;
            }
        }

        protected override void OnDie()
        {
            if (!IsDead)
            {
                base.OnDie();

                if (DeathVfx)
                {
                    var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
                    Destroy(vfx, 5f);
                }

                // loot
                if (CanDropItem())
                {
                    Instantiate(LootPrefab, transform.position, Quaternion.identity);
                }

                // unregister, send event
                _botManager.UnregisterBot(this);

                // destroy
                Debug.Assert(DeathDuration > 0);
                DelayAction(DeathDuration,
                    () =>
                    {
                        Destroy(gameObject);
                    });
            }
        }

        void OnDrawGizmosSelected()
        {
            // Path reaching range
            Gizmos.color = PathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, PathReachingRadius);
        }

        public bool TryAtack(Vector3 enemyPosition)
        {
            if (m_GameFlowManager.GameIsEnding)
                return false;

            // Shoot
            bool didFire = _weaponsManager.GetCurrentWeapon()
                .HandleShootInputs(false, true, false);

            if (didFire && onAttack != null)
            {
                onAttack.Invoke();

                // change to next wp?
            }

            return didFire;
        }

        public bool CanDropItem()
        {
            if (DropRate != 0 && LootPrefab != null)
            {
                if (DropRate == 1)
                    return true;
                else
                    return DropRate >= Random.Range(0f, 1f);
            }
            else
                return false;
        }

        internal WeaponController GetCurrentWeapon()
        {
            return _weaponsManager.GetCurrentWeapon();
        }

        public override Ray GetShotRay()
        {
            var currentWeapon = _weaponsManager.GetCurrentWeapon();

            var muzzlePosition = currentWeapon.WeaponMuzzle.position;
            // aim point on pawn
            var aimPosition = KnownTarget.transform.position;
            var aimPoint = KnownTarget.GetComponent<AimPoint>();
            if (aimPoint != null)
            {
                aimPosition = aimPoint.transform.position;
            }

            var aimDir = aimPosition - muzzlePosition;

            float spreadAngleRatio = currentWeapon.bulletSpreadAngle / 180f;// / fov
            Vector3 aimDirWithSpread = Vector3.Slerp
                (aimDir,
                UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            var ray = new Ray(muzzlePosition, aimDirWithSpread);
            return ray;
        }

        protected override void UpdateMovement()
        {
            // movement controlled by BotMobile
        }
        // End
    }
}