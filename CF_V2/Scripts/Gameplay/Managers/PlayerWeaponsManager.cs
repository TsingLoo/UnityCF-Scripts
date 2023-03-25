using System;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerWeaponsManager : PawnWeaponsManager
    {
        protected PlayerController _playerController;

        [Header("References")]
        public Camera Camera1P_Weapon;

        public AudioClip AimingSound;

        public Transform DefaultWeaponPosition;

        public Transform AimingWeaponPosition;

        [Tooltip("Position for innactive weapons")]
        public Transform DownWeaponPosition;

        [Header("Weapon Bob")]
        [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
        public float BobFrequency = 10f;

        [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
        public float BobSharpness = 10f;

        [Tooltip("Distance the weapon bobs when not aiming")]
        public float DefaultBobAmount = 0.05f;

        [Tooltip("Distance the weapon bobs when aiming")]
        public float AimingBobAmount = 0.02f;

        [Header("Weapon Recoil")]
        [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
        public float RecoilSharpness = 50f;

        [Tooltip("Maximum distance the recoil can affect the weapon")]
        public float MaxRecoilDistance = 0.5f;

        [Tooltip("How fast the weapon moves back from recoil")]
        public float RecoilRestitutionSharpness = 10f;

        [Header("FOV")]
        public float DefaultFov = 60f;

        [Tooltip("Portion of the regular FOV to apply to the weapon camera")]
        public float WeaponFovMultiplier = 1f;

        public bool IsRunning()
        {
            return _playerController.IsRunning;
        }
        public bool IsAiming { get; private set; }
        public bool IsPointingAtEnemy { get; private set; }

        PlayerInputHandler m_InputHandler;
        Vector3 m_LastCharacterPosition;

        float m_WeaponBobFactor;
        Vector3 m_WeaponMainLocalPosition;
        Vector3 m_WeaponBobLocalPosition;
        Vector3 m_WeaponRecoilLocalPosition;
        Vector3 _accumulatedWeaponRecoil; // weapon position

        /// <summary>
        /// Crosshair
        /// </summary>
        public float accumulatedCrosshairRecoil;
        private int shotsFired; // todo ref?, not used

        // bullet position in angle (before devided by 180)
        public Vector2 accumulatedBulletRecoil; // only y is used
        public Vector2 spreadThisShot; // cache for use
        public float bulletSpreadAngleApply = 0f;

        // camera recoil, same as bullet recoil but reset faster
        public Vector2 accumulatedCameraRecoil;

        protected override void Start()
        {
            base.Start();

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerWeaponsManager>(m_InputHandler, this,
                gameObject);

            _playerController = GetComponent<PlayerController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerController, PlayerWeaponsManager>(
                _playerController, this, gameObject);


            SetFov(DefaultFov);

            OnSwitchedToWeapon += OnWeaponSwitched;

        }

        protected override void Update()
        {
            base.Update();

            #region WeaponHandle
            WeaponController currentWeapon = GetCurrentWeapon();

            // update weapon handle
            if (currentWeapon != null)
            {
                // reload
                if (m_InputHandler.GetReloadButtonDown())// auto reload
                {
                    HandleWeaponReload();
                }

                // aiming
                if (currentWeapon.CanAim()
                    && m_InputHandler.GetAimInputDown())
                {
                    SwitchAiming();
                }
                // aim hold, not in use
                // IsAiming = m_InputHandler.GetAimInputHeld();

                // heavy
                if (currentWeapon.WeaponData.HasHeavy
                    && m_InputHandler.GetHeavyInputDown())
                {
                    currentWeapon.TryHeavyAttack();
                }

                // fire
                bool hasFired = currentWeapon.HandleShootInputs(
                    m_InputHandler.GetFireInputDown(),
                    m_InputHandler.GetFireInputHeld(),
                    m_InputHandler.GetFireInputReleased());

                // weapon status
                currentWeapon.TriggerHolding = m_InputHandler.GetFireInputHeld();

                // Handle accumulating recoil
                if (hasFired)
                {
                    shotsFired++;

                    AddRecoil(currentWeapon);
                }
            }

            // weapon switch / changeWeapon
            if (Input.GetButtonDown("SwitchWeapon"))
            {
                SwitchWeapon();
            }

            #region weapon select
            // switch up / down
            int switchWeaponInput = m_InputHandler.GetSwitchWeaponInput();
            if (switchWeaponInput != 0)
            {
                bool switchUp = switchWeaponInput > 0;
                ChangeToNextWeapon(switchUp);
            }
            // button select
            else
            {
                switchWeaponInput = m_InputHandler.GetSelectWeaponInput();
                if (switchWeaponInput > 0
                    && switchWeaponInput != CurrentBagPos)
                {
                    ChangeToWeapon(switchWeaponInput);
                }
            }
            #endregion
            #endregion

            #region Pointing at enemy
            IsPointingAtEnemy = false;
            if (currentWeapon)
            {
                if (Physics.Raycast(Camera1P_Weapon.transform.position, Camera1P_Weapon.transform.forward, out RaycastHit hit,
                    1000, -1, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.GetComponentInParent<Health>() != null)
                    {
                        IsPointingAtEnemy = true;
                    }
                }
            }

            #endregion

            UpdateBulletRecoil();
        }



        private void SwitchWeapon()
        {
            if (_lastBagPos != CurrentBagPos)
            {
                ChangeToWeapon(_lastBagPos);
            }
        }

        protected override void OnWeaponSwitched(WeaponController newWeapon)
        {
            base.OnWeaponSwitched(newWeapon);

            ResetRecoil();
        }

        // Update various animated features in LateUpdate because
        // it needs to override the animated arm position
        void LateUpdate()
        {
            UpdateWeaponAiming();

            // todo if use            
            //UpdateWeaponBob();
            UpdateWeaponRecoil();

            // Set final weapon socket position based on all the combined animation influences
            WeaponPosition1P.localPosition =
                m_WeaponMainLocalPosition
                + m_WeaponBobLocalPosition
                + m_WeaponRecoilLocalPosition;
        }


        // Sets the FOV of the main camera and the weapon camera simultaneously
        public void SetFov(float fov)
        {
            // 1P
            _playerController.Camera1P_Main.fieldOfView = fov;
            Camera1P_Weapon.fieldOfView = fov * WeaponFovMultiplier;

            // 3P
            //todo in the future, cinemachine
            //_pawnController.Camera3PController.m_Lens.FieldOfView = fov;
        }

        #region Dynamic Aiming
        /// <summary>
        /// StartAiming
        /// </summary>
        private void SwitchAiming()
        {
            IsAiming = !IsAiming;
            GetCurrentWeapon().AnimController.PlayOneShot(AimingSound);
        }

        public override void StopAiming()
        {
            IsAiming = false;
        }


        // Updates weapon position and camera FOV
        void UpdateWeaponAiming()
        {
            if (!GetCurrentWeapon() || !GetCurrentWeapon().WeaponData.HasAim)
            {
                return;
            }

            WeaponController activeWeapon = GetCurrentWeapon();
            if (activeWeapon)
            {
                if (IsAiming)
                {
                    m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                        AimingWeaponPosition.localPosition + activeWeapon.AimOffset,
                        activeWeapon.aimAnimSpeed * Time.deltaTime);

                    SetFov(Mathf.Lerp
                        (_playerController.Camera1P_Main.fieldOfView,
                        activeWeapon.AimZoomRatio * DefaultFov,
                        activeWeapon.aimAnimSpeed * Time.deltaTime));
                }
                else // not aim
                {
                    m_WeaponMainLocalPosition = Vector3.Lerp
                        (m_WeaponMainLocalPosition,
                        DefaultWeaponPosition.localPosition,
                        activeWeapon.aimAnimSpeed * Time.deltaTime);

                    SetFov(Mathf.Lerp
                        (_playerController.Camera1P_Main.fieldOfView,
                        DefaultFov,
                        activeWeapon.aimAnimSpeed * Time.deltaTime));
                }
            }
        }
        #endregion

        // Running
        // Updates the weapon bob
        void UpdateWeaponBob()
        {
            if (Time.deltaTime > 0f)
            {
                Vector3 playerCharacterVelocity =
                    (_playerController.transform.position - m_LastCharacterPosition) / Time.deltaTime;

                // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
                float characterMovementFactor = 0f;
                if (_playerController.IsGrounded)
                {
                    characterMovementFactor =
                        Mathf.Clamp01(playerCharacterVelocity.magnitude /
                                      (_playerController.MaxSpeedOnGround *
                                       _playerController.SprintSpeedModifier));
                }

                m_WeaponBobFactor =
                    Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

                // Calculate vertical and horizontal weapon bob values based on a sine function
                float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
                float frequency = BobFrequency;
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
                                  m_WeaponBobFactor;

                // Apply weapon bob
                m_WeaponBobLocalPosition.x = hBobValue;
                m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

                m_LastCharacterPosition = _playerController.transform.position;
            }
        }

        #region Recoil
        private void AddRecoil(WeaponController currentWeapon)
        {
            if (!currentWeapon.HasRecoil())
            {
                return;
            }

            // weapon position:
            _accumulatedWeaponRecoil += Vector3.back * currentWeapon.RecoilForce;
            _accumulatedWeaponRecoil = Vector3.ClampMagnitude(_accumulatedWeaponRecoil, MaxRecoilDistance);

            // crosshair:
            accumulatedCrosshairRecoil += 0.5f;

            // bullets:
            // spread angle
            if(bulletSpreadAngleApply <= currentWeapon.bulletSpreadAngle)
            {
                bulletSpreadAngleApply += currentWeapon.bulletSpreadAngle 
                    / currentWeapon.bulletSpreadAddBullets;
            }

            // spread
            spreadThisShot = bulletSpreadAngleApply
                * UnityEngine.Random.insideUnitCircle;

            // y
            accumulatedBulletRecoil.y += currentWeapon.yRecoilPerShot;
            accumulatedCameraRecoil.y += currentWeapon.yRecoilPerShot;
        }

        private void UpdateBulletRecoil()
        {
            if (!GetCurrentWeapon() || !GetCurrentWeapon().HasRecoil())
            {
                return;
            }

            // crosshair recoil decrease
            accumulatedCrosshairRecoil -= Time.deltaTime 
                * GetCurrentWeapon().crosshairRecoverSpeed;
            accumulatedCrosshairRecoil = Mathf.Clamp(accumulatedCrosshairRecoil, 
                0,
                GetCurrentWeapon().crosshairRecoilMax);

            #region Bullet position

            // todo del
            //if(accumulatedCrosshairRecoil == 0)
            //{
            //    ResetBulletRecoil();
            //}

            // reset when not firing
            if (m_InputHandler.GetFireInputReleased())
            {

                DelayAction(0.5f, () => { shotsFired = 0; });
            }
            else // firing
            {
                // bulet position decrease:
                // y, move down
                accumulatedBulletRecoil.y -= Time.deltaTime
                    * GetCurrentWeapon().yRecoilRecoverSpeed;
                accumulatedBulletRecoil.y = Mathf.Clamp(accumulatedBulletRecoil.y,
                    0,
                    GetCurrentWeapon().yRecoilMax);

                bulletSpreadAngleApply -= Time.deltaTime
                    * GetCurrentWeapon().bulletSpreadAngle;
                bulletSpreadAngleApply = Mathf.Clamp(bulletSpreadAngleApply, 0, GetCurrentWeapon().bulletSpreadAngle);
                // reset in below function

                // camera recoil
                // y, move down
                accumulatedCameraRecoil.y -= Time.deltaTime
                    * GetCurrentWeapon().cameraRecoilRecoverSpeed;
                accumulatedCameraRecoil.y = Mathf.Clamp(accumulatedCameraRecoil.y,
                    0,
                    GetCurrentWeapon().cameraRecoilMax);
            }
            #endregion

        }

        private void ResetRecoil()
        {
            // crosshair
            accumulatedCrosshairRecoil = 0f;

            // bullet
            accumulatedBulletRecoil = new Vector2();
            spreadThisShot = new Vector2();
            bulletSpreadAngleApply = 0f;

            // camera
            accumulatedCameraRecoil = new Vector2();

            // weapon
        }

        // Updates the weapon recoil animation
        void UpdateWeaponRecoil()
        {
            if (!GetCurrentWeapon() || !GetCurrentWeapon().HasRecoil())
            {
                return;
            }

            // if the accumulated recoil is further away
            // from the current position,
            // make the current position move towards
            // the recoil target
            if (m_WeaponRecoilLocalPosition.z >= _accumulatedWeaponRecoil.z * 0.99f)
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, _accumulatedWeaponRecoil,
                    RecoilSharpness * Time.deltaTime);
            }
            // otherwise, move recoil position to make it recover towards its resting pose
            else
            {
                m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero,
                    RecoilRestitutionSharpness * Time.deltaTime);
                _accumulatedWeaponRecoil = m_WeaponRecoilLocalPosition;
                shotsFired = 0;
            }
        }
        #endregion

        public void SetCameraLayer(LayerMask camera1PWeaponLayer)
        {
            Camera1P_Weapon.cullingMask = GameFlowManager.Instance
                .Camera1PWeaponLayer;
        }

        public int GetShotsFired()
        {
            return shotsFired;
        }


        // End
    }
}