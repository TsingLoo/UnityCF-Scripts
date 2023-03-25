using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class WeaponAnimationController : BaseAnimationController
    {
        Animator _animator;
        protected int weaponLayer = 0;

        Animator _fbxAnimator;
        WeaponController _weapon;
        PawnAnimationController _pawnAnimCon;

        [Header("Combo Animations")]
        /// <summary>
        /// cool down time to reset combo click to 0
        /// </summary>
        public float ClickResetTime = 1f;
        [HideInInspector] public int _clickCount = 0;

        [HideInInspector]
        public AnimatorOverrideController _animOverrideCon;

        List<AnimationClip> _anim1Ps = new List<AnimationClip>();
        List<AnimationClip> _anim1PCombos = new List<AnimationClip>();

        protected AudioSource _audioSource;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            // weapon data
            _weapon = GetComponent<WeaponController>();
            _weaponData = _weapon.WeaponData;
            _anim1Ps = _weapon.AnimClips;
            _anim1PCombos = _weapon.AnimCombos;

            _animator = GetComponentInChildren<Animator>();

            if (_animator)
            {
                RelpaceAnim1Ps();
            }
        }


        private void FixedUpdate()
        {
            UpdateParameters();

            // Melee Combo:
            // can be simplify by using click up, will lost combo when not holding trigger
            //if(!_weapon.TriggerHolding)
            if (GameFlowManager.Instance.ComboTimerOut())
            {
                _clickCount = 0;
            }
        }

        private void UpdateParameters()
        {
            if (_animator)
            {
                _animator.SetBool(nameof(_pawnAnimCon.IsRunning),
                    _pawnAnimCon.IsRunning);
            }
        }

        private void RelpaceAnim1Ps()
        {
            #region Animations Override
            // fbx animator
            var animators = GetComponentsInChildren<Animator>();
            if (animators.Count() > 1)
            {
                _fbxAnimator = animators[1];
                _animator.avatar = _fbxAnimator.avatar;

                // has fps weapon
                if (_weapon.HasFPSWeapon())
                {
                    Debug.Assert(_fbxAnimator.avatar != null);
                }
            }

            // override
            _animOverrideCon = new AnimatorOverrideController
                (_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _animOverrideCon;

            // RelpaceAllAnimations
            foreach (var clip in _anim1Ps)
            {
                var replacedAnim = _anim1Ps
                    .Where(it => it.name.EndsWith(clip.GetNameAffix()))
                    .FirstOrDefault();
                if (replacedAnim != null)
                {
                    _animOverrideCon[Join(AnimNames.Dummy, clip.GetNameAffix())]
                        = replacedAnim;
                }
            }
            #endregion

        }

        #region Clip events
        void FinishReload()
        {
            _weapon.FinishReload();
        }

        void MeleeAttack()
        {
            _weapon.MeleeAttack();
        }

        void MeleeHeavyAttack()
        {
            _weapon.MeleeHeavyAttack();
        }
        #endregion

        #region Draw
        internal void TriggerDraw()
        {
            PlayAnimState(Join(AnimNames.Dummy, AnimNames.Draw));

            // set new animations/parameters for weapon and player animator
            this.SwitchWeaponAnims();

            // player
            _pawnAnimCon
                .TriggerDraw();
        }


        /// <summary>
        /// Set after Draw, since parameters in Animator 
        /// should be overriten for each weapon
        /// </summary>
        public void SwitchWeaponAnims()
        {
            // should be called here
            _pawnAnimCon = _weapon.Owner
                .GetComponentInChildren<PawnAnimationController>();

            if (_weapon.HasFPSWeapon())
            {
                ChangeAnims1P();
            }

            _pawnAnimCon.ChangeAnim3P(_weaponData);
        }

        private void ChangeAnims1P()
        {
            // speed
            foreach (var clipDto in _weaponData.AnimDtos)
            {
                var speedPara = Join(AnimNames.AnimSpeed,
                        AnimNames.Dummy,
                        clipDto.AnimNameAffix);
                var animSpeed = clipDto.Speed;

                _animator.SetFloat(speedPara, animSpeed);
            }

            // parameters
            _animator.SetBool(nameof(_weapon.HasRun), _weapon.HasRun);
            _animator.SetBool(nameof(_weapon.HasFireAfter), _weapon.HasFireAfter);
        }
        #endregion

        #region Fire

        internal void TriggerFire()
        {
            // combo
            if (_anim1PCombos.Count > 0)
            {
                TriggerCombo();
            }
            else // normal fire
            {
                if (_weapon.IsLastFire())
                {
                    PlayAnimState(Join(AnimNames.Dummy, AnimNames.FireLast));
                }
                else
                {
                    // when fireGap is small,
                    // fire animation is not over, play from beginning
                    if (_animator)
                    {
                        _animator.Play(Join(AnimNames.Dummy, AnimNames.Fire),
                            weaponLayer,
                            normalizedTime: 0f);
                    }
                }

                // player
                _pawnAnimCon
                    .TriggerFire();
            }
        }

        // todo crossFade
        private void TriggerCombo()
        {
            GameFlowManager.Instance.ComboTimer = _weapon.WeaponData.FireGap
                + ClickResetTime;

            var useFullBodyCombo = false;
            if (!_weapon.HasFPSWeapon())
            {
                useFullBodyCombo = true;
            }

            PlayAnimClip(_anim1PCombos[_clickCount], useFullBodyCombo);
            _clickCount++;

            if (_clickCount >= _anim1PCombos.Count)
            {
                _clickCount = 0;
            }
        }

        internal void TriggerFireReady()
        {
            PlayAnimState(Join(AnimNames.Dummy, AnimNames.FireReady));
        }

        internal void TriggerHeavy()
        {
            // weapon anim
            PlayAnimState(Join(AnimNames.Dummy, AnimNames.Heavy));

            // player anim
            var useFullBodyCombo = false;
            if (!_weapon.HasFPSWeapon())
            {
                useFullBodyCombo = true;
            }
            _pawnAnimCon.PlayWeaponAnimState(Join(AnimNames.Dummy, AnimNames.Heavy),
                useFullBodyCombo);

            //_pawnAnimCon
            //    .TriggerHeavy(useFullBodyCombo);
        }

        #endregion

        #region Reload
        internal void TriggerReload()
        {
            PlayAnimState(Join(AnimNames.Dummy, AnimNames.Reload));

            _pawnAnimCon
                .TriggerReload();
        }
        #endregion

        #region Helper function

        protected void PlayAnimClip(AnimationClip animationClip,
            bool useFullBodyCombo = false)
        {
            // Dummy_Fire
            var stateName = Join(AnimNames.Dummy,
                animationClip.GetNameAffix());

            // weapon
            this.PlayAnimState(stateName);

            // player
            _pawnAnimCon.PlayWeaponAnimState(stateName, useFullBodyCombo);
        }


        public void PlayAnimState(string animStateName)
        {
            if (_animator)
            {
                _animator.Play(animStateName, weaponLayer);
            }
        }

        internal void TriggerPutAway()
        {
            // weapon1P
            if (_animator)
            {
                _animator.WriteDefaultValues();
            }

            // player
        }

        /// <summary>
        /// Check State
        /// </summary>
        /// <returns></returns>
        internal EWeaponState GetWeaponState()
        {
            var newState = EWeaponState.Idle;

            // fire anim could be longer than fire gap
            if (InAnimState(AnimNames.Fire, _weapon.WeaponData.FireGap)
                || InAnimState(AnimNames.Heavy))
            {
                newState = EWeaponState.Fire;
            }
            else if (InAnimState(AnimNames.FireReady))
            {
                newState = EWeaponState.FireReady;
            }
            else if (InAnimState(AnimNames.Reload))
            {
                newState = EWeaponState.Reload;
            }
            else if (InAnimState(AnimNames.Draw))
            {
                newState = EWeaponState.Draw;
            }

            return newState;
        }

        public bool InAnimState(string animAffix,
            float outDuration = 0)
        {
            var inState = false;

            int stateNameHash = Animator.StringToHash
                (Join(AnimNames.Dummy, animAffix));

            AnimatorStateInfo stateInfo;
            if (_animator)
            {
                stateInfo = _animator
                    .GetCurrentAnimatorStateInfo(weaponLayer);
            }
            else // pawn
            {
                stateInfo = _pawnAnimCon.GetWeaponStateInfo();
            }

            if (stateInfo.shortNameHash == stateNameHash)
            {
                inState = true;

                if (outDuration > 0
                    && stateInfo.length > outDuration)
                {
                    inState = false;
                }
            }

            return inState;
        }

        public override void PlayOneShot(AudioClip audioClip) 
        {
            _audioSource.PlayOneShot(audioClip);
        }
        #endregion
        // End
    }
}