using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class Jetpack : BaseBehaviour
    {
        public bool UnlockAtStart = false;

        public ParticleSystem[] jetpackVFXs;

        [Header("Settings")]
        public float JetpackAcceleration = 7f;
        [Range(0f, 1f)]
        [Tooltip( "This will affect how much using the jetpack will cancel the gravity value, to start going up faster. 0 is not at all, 1 is instant")]
        public float JetpackDownwardVelocityCancelingFactor = 1f;

        [Header("Durations")] 
        [Tooltip("Time it takes to consume all the jetpack fuel")]
        public float ConsumeDuration = 5f;

        [Tooltip("Time it takes to completely refill the jetpack while on the ground")]
        public float RefillDurationGrounded = 2f;

        [Tooltip("Time it takes to completely refill the jetpack while in the air")]
        public float RefillDurationInTheAir = 2f;

        [Tooltip("Delay after last use before starting to refill")]
        public float RefillDelay = 1.5f;

        [Header("Audio")] 
        [Tooltip("Sound loop")]
        public AudioClip JetpackSfx;

        PlayerController _playerController;
        PlayerInputManager _inputManager;
        bool _canUseJetpack;
        float _lastTimeOfUse;

        // stored ratio for jetpack resource (1 is full, 0 is empty)
        public float CurrentFillRatio { get; private set; }
        public bool IsJetpackUnlocked { get; private set; }

        public bool IsPlayerGrounded() => _playerController.IsGrounded;

        public UnityAction<bool> OnUnlockJetpack;

        AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            IsJetpackUnlocked = UnlockAtStart;

            _playerController = GetComponent<PlayerController>();
            if(_playerController == null)
            {
                _playerController = GetComponentInParent<PlayerController>();
            }

            _inputManager = GetComponent<PlayerInputManager>();
            if(_inputManager == null)
            {
                _inputManager = GetComponentInParent<PlayerInputManager>();
            }

            CurrentFillRatio = 1f;

            _audioSource.clip = JetpackSfx;
            _audioSource.loop = true;
        }

        void Update()
        {
            if (IsPlayerGrounded())
            {
                _canUseJetpack = false;
            }
            // in air and press jump
            else if (!_playerController.HasJumpedThisFrame 
                && _inputManager.GetJumpInputDown())
            {
                _canUseJetpack = true;
            }

            // in use
            bool isInUse = _canUseJetpack 
                && IsJetpackUnlocked 
                && CurrentFillRatio > 0f 
                && _inputManager.GetJumpInputHeld();

            if (isInUse)
            {
                // for refill delay
                _lastTimeOfUse = Time.time;

                float totalAcceleration = JetpackAcceleration;
                // cancel out gravity
                totalAcceleration += _playerController.GravityDownForce;

                if (_playerController.CharacterVelocity.y < 0f)
                {
                    // handle making the jetpack compensate for character's downward velocity with bonus acceleration
                    totalAcceleration += -_playerController.CharacterVelocity.y / Time.deltaTime 
                        * JetpackDownwardVelocityCancelingFactor;
                }

                // apply the acceleration to character's velocity
                _playerController.CharacterVelocity 
                    += Vector3.up * totalAcceleration * Time.deltaTime;

                // consume fuel
                CurrentFillRatio = CurrentFillRatio 
                    - (Time.deltaTime / ConsumeDuration);

                // VFX
                for (int i = 0; i < jetpackVFXs.Length; i++)
                {
                    var vfxEmission = jetpackVFXs[i].emission;
                    vfxEmission.enabled = true;
                }

                // sound
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
            else // not in use
            {
                // refill over time
                if (IsJetpackUnlocked 
                    && Time.time - _lastTimeOfUse >= RefillDelay)
                {
                    var refillTime = _playerController.IsGrounded
                        ? RefillDurationGrounded
                        : RefillDurationInTheAir;
                    float refillRate = 1 / refillTime;
                    CurrentFillRatio = CurrentFillRatio + Time.deltaTime * refillRate;
                }
                CurrentFillRatio = Mathf.Clamp01(CurrentFillRatio);

                // stop vfx
                for (int i = 0; i < jetpackVFXs.Length; i++)
                {
                    var emissionModulesVfx = jetpackVFXs[i].emission;
                    emissionModulesVfx.enabled = false;
                }

                // stop sound
                if (_audioSource.isPlaying)
                {
                    _audioSource.Stop();
                }
            }
        }

        public bool TryUnlock()
        {
            if (IsJetpackUnlocked)
                return false;

            OnUnlockJetpack.Invoke(true);

            IsJetpackUnlocked = true;
            _lastTimeOfUse = Time.time;
            
            return true;
        }

        // End
    }
}