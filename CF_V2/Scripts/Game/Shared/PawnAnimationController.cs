using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(AudioSource), typeof(Animator))]
    public class PawnAnimationController : BaseAnimationController
    {
        [Header("Crouch")]
        public float ModelPositionStand = 1f;
        public float ModelPositionCrouch = 0.7f;

        // todo fix height in animation
        /// <summary>
        /// in animator
        /// </summary>
        public float CrouchTransitionTime = 0.25f;

        protected AudioSource _audioSource;

        /// <summary>
        /// TPS Melee weapon using Hit detection
        /// </summary>
        MeleeWeapon _meleeWeapon;
        // todo ShieldCollider

        GameObject _ownerPawn;
        CharacterModel _characterModel;

        #region Init
        protected Animator _animator;
        // body anims
        List<AnimationClip> _pawnAnims = new List<AnimationClip>();

        /// <summary>
        /// lower body
        /// should be the 1st
        /// </summary>
        protected int _baseLayer = 0;
        /// <summary>
        /// upper body
        /// </summary>
        protected int _weaponLayer = 1;
        protected int _fullBodyLayer = 2;

        // todo BasePawnController
        public GameObject Owner { get; set; }
        List<AnimatorOverrideController> _animOverrideCons;
        [HideInInspector]
        protected AnimatorOverrideController _animOverrideCon;

        public float XInput { get; set; }
        public float YInput { get; set; }

        public bool IsRunning { get; set; }
        public bool IsWalking { get; set; }
        public bool IsFalling { get; set; }
        public bool IsCrouching { get; set; }

        /// <summary>
        /// speed in xy plane
        /// </summary>
        public float MoveSpeed { get; set; }

        // death audio clip
        AudioClip death;

        // player
        float AnimSpeed_Run = 2.5f;
        float AnimSpeed_Jump = 2f;
        float AnimSpeed_Death = 2.5f;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Debug.Assert(_audioSource != null);

            InitAnims();

        }

        private void Start()
        {
        }

        // and anim audios
        private void InitAnims()
        {
            _characterModel = GetComponentInChildren<CharacterModel>();
            _animator = GetComponentInChildren<Animator>();
            _animOverrideCons = Resources.LoadAll<AnimatorOverrideController>
                ("Animations/Controller").ToList();

            // override called in draw: ChangeAnim3P

            // load body anims
            var bodyAnims = Resources.LoadAll<AnimationClip>
                ($"Animations/Woman");
            _pawnAnims.AddRange(bodyAnims);


            //todo ref
            // currently not in use in animator
            //SetAnimSpeed(nameof(AnimSpeed_Run), AnimSpeed_Run);
            //SetAnimSpeed(nameof(AnimSpeed_Jump), AnimSpeed_Jump);
            //SetAnimSpeed(nameof(AnimSpeed_Death), AnimSpeed_Death);
        }

        #endregion

        private void Update()
        {
            _animator.SetFloat("XInput", XInput);
            _animator.SetFloat("YInput", YInput);

            _animator.SetBool(nameof(IsRunning), IsRunning);
            _animator.SetBool(nameof(IsWalking), IsWalking);
            _animator.SetBool(nameof(IsCrouching), IsCrouching);
            _animator.SetBool(nameof(IsFalling), IsFalling);

            _animator.SetFloat(nameof(MoveSpeed), MoveSpeed);
        }

        #region Change anims

        /// <summary>
        /// call in draw
        /// change anim and anim speed
        /// </summary>
        /// <param name="weaponData"></param>
        public void ChangeAnim3P(WeaponData weaponData)
        {
            _weaponData = weaponData;

            // override by weapon
            _animOverrideCon = _animOverrideCons.FirstOrDefault
                (x => x.name.EndsWith(_characterModel.sex.GetCode() 
                    + "_" + weaponData.WeaponAnimType.GetCode()));
            Debug.Assert(_animOverrideCon != null);

            _animator.runtimeAnimatorController = _animOverrideCon;


            InitWeapon3P();

            // set speed by weapon
            ChangeAnimSpeed3P(weaponData);
        }

        /// <summary>
        /// For Melee / RPG weapon
        /// </summary>
        private void InitWeapon3P()
        {
            _meleeWeapon = GetComponentInChildren<MeleeWeapon>();
            if (_meleeWeapon)
            {
                _meleeWeapon.SetOwner(_ownerPawn);
            }

            #region RPG Weapons: Add weapon3P events (when no weapon1P)
            if (_weaponData.IsRPGWeapon)
            {
                foreach (var clip in _animator.runtimeAnimatorController.animationClips)
                {
                    var clipDto = _weaponData.GetAnimDto(clip.name);
                    if (clipDto != null)
                    {
                        clip.events = clipDto.AnimClip.events;
                    }
                }
            }
            #endregion
        }

        private void ChangeAnimSpeed3P(WeaponData weaponData)
        {
            // upper body anims
            // no draw
            var animsNeedSync = new List<string>()
            {
                AnimNames.Fire,
                AnimNames.Heavy,
                AnimNames.Reload,
                AnimNames.Combo1,
                AnimNames.Combo2,
                AnimNames.Combo3,
                AnimNames.Combo4,
            };

            foreach (var animClip1P in weaponData.AnimDtos)
            {
                if (animClip1P.SyncPawnAnim)
                {
                    var nameAffix = animClip1P.AnimNameAffix;
                    if (animsNeedSync.Contains(nameAffix))
                    {
                        var anim3Ps = _animator.runtimeAnimatorController
                            .animationClips;
                        var animClip3P = anim3Ps
                            .FirstOrDefault(it => it.name
                            .EndsWith(nameAffix));
                        Debug.Assert(animClip3P != null);

                        if (animClip3P != null)
                        {
                            var animTime = animClip1P.RealTime;

                            // set speed by time, since 2 speed could be different
                            var animSpeed3P = animClip3P
                                .GetSpeedByTime(animTime);

                            var speedParaName = "WeaponAnimSpeed_Dummy_" + nameAffix;
                            _animator.SetFloat(speedParaName, animSpeed3P);
                        }
                    }
                }
            }
        }


        #endregion


        #region Anim Events

        #region RPG Weapon
        /// <summary>
        /// BeginAttack
        /// </summary>
        public void AttackStart()
        {
            _meleeWeapon.BeginAttack();
        }

        public void AttackEnd()
        {
            _meleeWeapon.EndAttack();
        }
        #endregion

        void FootStep(int a)
        {
            // todo
        }

        #endregion





        #region Helper function

        public void PlayWeaponAnimState(string animStateName,
            bool useFullBodyCombo = false)
        {
            _animator.Play(animStateName, _weaponLayer);

            // Action Pawn
            if (useFullBodyCombo)
            {
                _animator.SetLayerWeight(ActionPawnAnimLayer.Combat, 1f);
            }
        }

        protected void PlayAnimClip(AnimationClip animationClip)
        {
            var dummyState = Join(AnimNames.Dummy, animationClip.GetNameAffix());
            this.PlayWeaponAnimState(dummyState);
        }

        public void SetAnimSpeed(string animSpeedName, float speed)
        {
            _animator.SetFloat(animSpeedName, speed);
        }
        #endregion



        public AnimatorStateInfo GetWeaponStateInfo()
        {
            return _animator
                    .GetCurrentAnimatorStateInfo(_weaponLayer);
        }


        #region Triggers
        public void TriggerFire()
        {
            _animator.Play(Join(AnimNames.Dummy, AnimNames.Fire),
                _weaponLayer);
        }

        public void TriggerDraw()
        {
            // todo no draw anim, use Idle
            _animator.Play(Join(AnimNames.Dummy, AnimNames.Idle),
                _weaponLayer);
        }


        public void TriggerFireReady()
        {
            _animator.Play(Join(AnimNames.Dummy, AnimNames.FireReady),
                _weaponLayer);
        }


        public void TriggerReload()
        {
            _animator.Play(Join(AnimNames.Dummy, AnimNames.Reload),
                _weaponLayer);
        }

        public void TriggerJump()
        {
            _animator.SetTrigger("Jump");
        }

        public void TriggerRoll()
        {
            _animator.SetTrigger("Roll");
        }

        /// <summary>
        /// Dummy_Idle
        /// </summary>
        /// <param name="Idle">anim affix</param>
        public void PlayFullBodyAnim(string animAffix)
        {
            // replace Dummy_BodyAnim
            var dummyState = Join(AnimNames.Dummy, AnimNames.BodyAnim);

            PlayReplacedAnim(animAffix, dummyState, _fullBodyLayer);
        }

        public void PlayUpperBodyAnim(string animAffix)
        {
            // replace Dummy_BodyAnim
            var dummyState = Join(AnimNames.Dummy, AnimNames.BodyAnim);

            PlayReplacedAnim(animAffix, dummyState, _weaponLayer);
        }

        protected void PlayReplacedAnim(string animAffix,
            string dummyState,
            int layer)
        {
            var replacedAnim = _pawnAnims
                    .Where(it => it.name.EndsWith(animAffix))
                    .FirstOrDefault();

            if (replacedAnim != null)
            {
                _animOverrideCon[dummyState]
                    = replacedAnim;
            }

            // play dummyState
            _animator.Play(dummyState, layer);
        }


        //internal void TriggerHeavy(bool useFullBodyCombo = false)
        //{
        //    PlayWeaponAnimState(Join(AnimNames.Dummy, AnimNames.Heavy),
        //        useFullBodyCombo);
        //}
        #endregion

        public override void PlayOneShot(AudioClip audioClip)
        {
            _audioSource.PlayOneShot(audioClip);
        }

        public void SetOwner(GameObject owner)
        {
            _ownerPawn = owner;
        }

        public void ResetAnimator()
        {
            _animator.SetTrigger("Reset");
            _animator.WriteDefaultValues();
        }

    }
}