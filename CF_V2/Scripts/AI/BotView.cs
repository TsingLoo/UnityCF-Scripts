using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Bot View, find enemy
    /// </summary>
    public class BotView : MonoBehaviour
    {
        [Tooltip("The point representing the source of target-detection raycasts for the enemy AI")]
        public Transform DetectionSourcePoint;

        public float ViewAngle = 90f;
        public float ViewRange = 20f;
        public bool HasBackView = false;
        public float ViewRangeBack = 5f;

        float _defaultAttackRange = 10f;

        public float KnownTargetTimeout = 4f;

        public Animator Animator;

        [Header("Debug")]
        public Color ViewRangeColor = Color.blue;
        public Color AttackRangeColor = Color.red;

        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public GameObject KnownTarget { get; private set; }
        public bool IsTargetInAttackRange { get; private set; }
        public bool IsSeeingTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }

        protected float TimeLastSeenTarget = Mathf.NegativeInfinity;

        PawnWeaponsManager _weaponsManager;
        ActorsManager m_ActorsManager;

        const string k_AnimAttackParameter = "Attack";
        const string k_AnimOnDamagedParameter = "OnDamaged";

        protected virtual void Start()
        {
            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, BotView>(m_ActorsManager, this);

            _weaponsManager = GetComponent<PawnWeaponsManager>();
        }

        public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
        {
            // detection timeout
            if (KnownTarget
                && !IsSeeingTarget
                && (Time.time - TimeLastSeenTarget) > KnownTargetTimeout)
            {
                KnownTarget = null;
            }

            // Find the closest visible hostile actor
            float sqrViewRange = ViewRange * ViewRange;
            float sqrViewRangeBack = ViewRangeBack * ViewRangeBack;
            IsSeeingTarget = false;
            float closestSqrDist = Mathf.Infinity;
            foreach (Actor otherActor in m_ActorsManager.Actors)
            {
                if (IsEnemy(actor, otherActor))
                {
                    // view angle
                    Vector3 dirToOther = otherActor.transform.position
                        - transform.position;
                    var inViewAngle = Vector3.Angle(transform.forward, dirToOther)
                        <= ViewAngle / 2;

                    // view range
                    float sqrDist = (otherActor.transform.position - DetectionSourcePoint.position).sqrMagnitude;
                    if (sqrDist < closestSqrDist)
                    {
                        if (inViewAngle && sqrDist < sqrViewRange // front
                            || !inViewAngle && HasBackView && sqrDist < sqrViewRangeBack) // back
                        {
                            #region In Range
                            // Check for obstructions
                            RaycastHit[] hits = Physics.RaycastAll(DetectionSourcePoint.position,
                                (otherActor.AimPoint.position - DetectionSourcePoint.position).normalized,
                                ViewRange,
                                -1,
                                QueryTriggerInteraction.Ignore);

                            RaycastHit closestValidHit = new RaycastHit();
                            closestValidHit.distance = Mathf.Infinity;

                            bool foundValidHit = false;
                            foreach (var hit in hits)
                            {
                                if (!selfColliders.Contains(hit.collider)
                                    && hit.distance < closestValidHit.distance)
                                {
                                    closestValidHit = hit;
                                    foundValidHit = true;
                                }
                            }

                            if (foundValidHit)
                            {
                                Actor hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                                if (hitActor == otherActor)
                                {
                                    IsSeeingTarget = true;
                                    closestSqrDist = sqrDist;

                                    TimeLastSeenTarget = Time.time;
                                    KnownTarget = otherActor.gameObject;
                                }
                            }
                            #endregion
                        }

                    }
                }
            }

            IsTargetInAttackRange = KnownTarget != null
                && Vector3.Distance(transform.position, KnownTarget.transform.position)
                    <= GetAttackRange();

            // detect events
            if (!HadKnownTarget &&
                KnownTarget != null)
            {
                OnDetect();
            }

            if (HadKnownTarget &&
                KnownTarget == null)
            {
                OnLostTarget();
            }

            HadKnownTarget = KnownTarget != null;
        }

        private bool IsEnemy(Actor actor, Actor otherActor)
        {
            var gameMode = GameFlowManager.Ins.currentGameMode;
            switch (gameMode)
            {
                case EGameMode.TD:
                case EGameMode.Bomb:
                case EGameMode.Nano:
                    {
                        return actor.Team != otherActor.Team;
                    }

                case EGameMode.FreeForAll:
                    return true;
            }

            return false;
        }


        public virtual void OnLostTarget()
        {
            onLostTarget?.Invoke();
        }

        public virtual void OnDetect()
        {
            onDetectedTarget?.Invoke();
        }

        public virtual void OnDamaged(GameObject damageSource)
        {
            TimeLastSeenTarget = Time.time;
            KnownTarget = damageSource;

            if (Animator)
            {
                Animator.SetTrigger(k_AnimOnDamagedParameter);
            }
        }

        public virtual void OnAttack()
        {
            if (Animator)
            {
                Animator.SetTrigger(k_AnimAttackParameter);
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawViewAngle();

            // Attack range
            Gizmos.color = AttackRangeColor;
            Gizmos.DrawWireSphere(transform.position, _defaultAttackRange);

        }

        public float GetDefaultAttackRange()
        {
            return _defaultAttackRange;
        }

        public float GetAttackRange()
        {
            var weaponRange = _defaultAttackRange;
            if(_weaponsManager && _weaponsManager.GetCurrentWeapon() != null)
            {
                weaponRange = _weaponsManager.GetCurrentWeapon().WeaponData.WeaponRange;
            }

            return Mathf.Min(weaponRange, _defaultAttackRange);
        }

        #region Draw view
        private void DrawViewAngle()
        {
            Handles.color = ViewRangeColor;

            var halfAngle = ViewAngle / 2;

            // line
            Vector3 viewAngleLeft = DirFromAngle(-halfAngle, false);
            Vector3 viewAngleRight = DirFromAngle(halfAngle, false);

            Handles.DrawLine(transform.position,
                transform.position + viewAngleLeft * ViewRange);
            Handles.DrawLine(transform.position,
                transform.position + viewAngleRight * ViewRange);


            // arc front
            Handles.DrawWireArc(transform.position,
                Vector3.up,
                viewAngleLeft,
                ViewAngle,
                ViewRange);

            // arc back
            if (HasBackView)
            {
                Handles.DrawWireArc(transform.position,
                    Vector3.up,
                    viewAngleRight,
                    360 - ViewAngle,
                    ViewRangeBack);
            }

            // todo sphere
            // Detection range
            //Gizmos.color = ViewRangeColor;
            //Gizmos.DrawWireSphere(transform.position, ViewRange);
        }

        public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
        #endregion

        // End
    }
}