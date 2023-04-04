using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// BotMobile / State
    /// </summary>
    [RequireComponent(typeof(BotController))]
    public class BotAI : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Follow,
            Attack,
            Death
        }

        public Animator BotAnimator;

        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.7f;


        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState CurrentState { get; private set; }
        BotController _botController;
        AudioSource m_AudioSource;

        void Start()
        {
            _botController = GetComponent<BotController>();
            DebugUtility.HandleErrorIfNullGetComponent<BotController, BotAI>(_botController, this,
                gameObject);

            _botController.onAttack += OnAttack;
            _botController.onDetectedTarget += OnDetectedTarget;
            _botController.onLostTarget += OnLostTarget;
            _botController.onDamaged += OnDamaged;
            _botController.SetPathDestinationToClosestNode();

            //BotAnimator = GetComponent<Animator>();

            // Start patrolling
            CurrentState = AIState.Patrol;

            // todo ref
            // adding a audio source to play the movement sound on it
            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, BotAI>(m_AudioSource, this, gameObject);

            m_AudioSource.clip = MovementSound;
            m_AudioSource.Play();

            EventManager.AddListener<BotDeathEvent>(OnBotDeath);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<BotDeathEvent>(OnBotDeath);
        }

        void Update()
        {
            if (CurrentState != AIState.Death)
            {
                UpdateAiStateTransitions();
                UpdateCurrentAiState();

                UpdateMovement();
            }

        }

        // todo ref
        void OnBotDeath(BotDeathEvent evt)
        {
            if(evt.Bot.name != _botController.name // other bot die
                && _botController.KnownTarget != null)
            {
                var botCon = _botController.KnownTarget.GetComponentInParent<BotController>();

                if (botCon == null
                    || botCon.IsDead)
                {
                    OnLostTarget();
                }

            }

        }

        private void UpdateMovement()
        {
            float moveSpeed = _botController.NavMeshAgent.velocity.magnitude;

            _botController.IsRunning = moveSpeed > 0.01;
            if (_botController.IsRunning)
            {
                _botController.animController.XInput = 0;
                _botController.animController.YInput = 1;
            }

            // todo 
            _botController.IsSprinting = false;
            _botController.IsCrouching = false;
            _botController.IsGrounded = true;

            // change pitch via speed
            m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min,
                PitchDistortionMovementSpeed.Max,
                moveSpeed / _botController.NavMeshAgent.speed);
        }

        void UpdateAiStateTransitions()
        {
            if (_botController.IsDead)
            {
                CurrentState = AIState.Death;
                return;
            }

            // Handle transitions 
            switch (CurrentState)
            {
                case AIState.Follow:
                    {
                        // Transition to attack
                        if (_botController.IsSeeingTarget
                            && _botController.IsTargetInAttackRange)
                        {
                            CurrentState = AIState.Attack;
                            _botController.SetNavDestination(transform.position);
                        }

                        break;

                    }
                case AIState.Attack:
                    {
                        // Transition to follow when out of range
                        if (!_botController.IsTargetInAttackRange)
                        {
                            CurrentState = AIState.Follow;
                        }

                        break;

                    }
            }
        }

        void UpdateCurrentAiState()
        {
            Debug.Log(CurrentState);

            // Handle logic 
            switch (CurrentState)
            {
                case AIState.Patrol:
                    {
                        _botController.UpdatePathDestination();
                        _botController.SetNavDestination(_botController.GetDestinationOnPath());
                        break;

                    }
                case AIState.Follow:
                    {
                        _botController.SetNavDestination(_botController.KnownTarget.transform.position);
                        _botController.OrientTowards(_botController.KnownTarget.transform.position);
                        break;

                    }
                case AIState.Attack:
                    {
                        if (_botController.KnownTarget == null)
                        {
                            OnLostTarget();
                            break;
                        }

                        // move to target
                        Vector3 targetPosition = _botController.KnownTarget.transform.position;
                        if (Vector3.Distance(targetPosition,
                                _botController.BotView.DetectionSourcePoint.position)
                            >= GetAttackStopDistance())
                        {
                            _botController.SetNavDestination(targetPosition);
                        }
                        else // stop at stop range
                        {
                            _botController.SetNavDestination(transform.position);
                        }

                        _botController.OrientTowards(targetPosition);
                        _botController.TryAtack(targetPosition);

                        break;

                    }

                case AIState.Death:
                    {
                        _botController.NavMeshAgent.enabled = false;
                    }
                    break;
            }
        }

        private float GetAttackStopDistance()
        {
            var dist = _botController.BotView.GetAttackRange();
            if(dist >= _botController.BotView.GetDefaultAttackRange())
            {
                dist *= AttackStopDistanceRatio;
            }

            return dist;
        }

        void OnAttack()
        {
            //BotAnimator.SetTrigger(k_AnimAttackParameter);
        }

        void OnDetectedTarget()
        {
            if (CurrentState == AIState.Patrol)
            {
                CurrentState = AIState.Follow;
            }

            //BotAnimator.SetBool(k_AnimAlertedParameter, true);
        }

        void OnLostTarget()
        {
            if (CurrentState == AIState.Follow
                || CurrentState == AIState.Attack)
            {
                CurrentState = AIState.Patrol;
            }

            //BotAnimator.SetBool(k_AnimAlertedParameter, false);
        }

        void OnDamaged()
        {
            // todo move speed
            //BotAnimator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }
}