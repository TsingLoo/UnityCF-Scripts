using System;
using Unity.FPS.Game;
using Unity.FPS.Inventory;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// PlayerPawn
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
    public class PlayerController : PawnController
    {
        protected new PlayerWeaponsManager _weaponsManager;


        #region Camera
        // 3p
        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;

        float m_CameraVerticalAngle = 0f;
        public float MinCameraAngle = -89.0f;
        public float MaxCameraAngle = 89.0f;

        // multi constraint aim
        public Transform aimPos;
        public Vector3 aimPosOffset;
        [SerializeField] float aimSmoothSpeed = 20;

        [Header("Body rotate")]
        public Vector3 rotateOffset; // compensate pointing offset
        #endregion

        #region Input / Movement
        protected PlayerInputHandler _inputHandler;
        CharacterController m_Controller;

        Vector3 m_GroundNormal;

        // ground check
        [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
        public float GroundCheckDistance = 1f;//0.05
        protected const float k_GroundCheckDistanceInAir = 0.07f;
        float chosenGroundCheckDistance;

        #endregion

        UI_Inventory uiInventory;
        UI_PawnEquipment uiEquipment;

        protected override void Init()
        {
            base.Init();

            m_Controller = GetComponent<CharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerController>(m_Controller,
                this, gameObject);

            _inputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerController>(_inputHandler,
                this, gameObject);

            _weaponsManager = GetComponent<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerController>(
                _weaponsManager, this, gameObject);

            // set in bot
            ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
            if (actorsManager != null)
                actorsManager.SetPlayer(gameObject);

            // player camera face, todo not working
            //Camera3P.transform.localRotation = Quaternion.identity;
            //CinemachineCameraTarget.transform.localRotation = Quaternion.identity;
        }

        protected override void Start()
        {
            base.Start();

            uiInventory = GameFlowManager.Instance.GetComponentInChildren<UI_Inventory>();
            uiEquipment = GameFlowManager.Instance.GetComponentInChildren<UI_PawnEquipment>();

            SetCameraLayer();
            SwitchView(isFirstView: true);

            m_Controller.enableOverlapRecovery = true;
            // force the crouch state to false when starting
            SetCrouchingState(false, true);




        }

        protected override void Update()
        {
            base.Update();

            if (_inputHandler.GetInputDown(ButtonNames.ThrowWeapon))
            {
                ThrowWeapon();
            }

            if (_inputHandler.GetInputDown(ButtonNames.SwitchCamera))
            {
                SwitchView(!isFirstPerson);
            }

            UpdateJump();

            UpdateBodyHeight(false);

        }

        private void SetCameraLayer()
        {
            Camera1P_Main.cullingMask = GameFlowManager.Instance
                .Camera1PLayer;
            _weaponsManager.SetCameraLayer(GameFlowManager.Instance
                .Camera1PWeaponLayer);


            Camera3P.cullingMask = GameFlowManager.Instance
                .Camera3PLayer;
        }


        protected override void UpdateMovement()
        {
            IsSprinting = _inputHandler.GetInputHeld(ButtonNames.Sprint);

            // character movement handling
            {
                if (IsSprinting)
                {
                    IsSprinting = SetCrouchingState(false, false);
                }

                float speedModifier = IsSprinting ? SprintSpeedModifier : 1f;

                // WASD
                // converts move input to a worldspace vector based on our character's transform orientation
                Vector3 worldspaceMoveInput = transform.TransformVector
                    (_inputHandler.GetMoveInput());

                // handle grounded movement
                if (IsGrounded)
                {
                    Vector3 targetVelocity = worldspaceMoveInput
                        * MaxSpeedOnGround
                        * speedModifier;

                    #region set status
                    if (targetVelocity.magnitude > 0.01)
                    {
                        if (IsCrouching)
                        {
                            IsWalking = true;
                        }
                        else
                        {
                            IsRunning = true;
                        }
                    }
                    else // not moving
                    {
                        IsWalking = false;
                        IsRunning = false;
                    }
                    #endregion

                    // crouch
                    if (IsCrouching)
                    {
                        IsRunning = false;

                        targetVelocity *= MaxSpeedCrouchedRatio;
                    }
                    targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) *
                                     targetVelocity.magnitude;

                    // smooth speed
                    CharacterVelocity = Vector3
                        .Lerp(CharacterVelocity,
                        targetVelocity,
                        MovementSharpnessOnGround * Time.deltaTime);

                    //todo
                    // rolling
                    //if(IsGrounded && _inputHandler.GetInputDown(ButtonNames.Roll))
                    //{
                    //    AnimController.TriggerRoll();
                    //}

                    // jumping
                    if (IsGrounded && _inputHandler.GetJumpInputDown())
                    {
                        // force the crouch state to false
                        if (SetCrouchingState(false, false))
                        {
                            // start by canceling out the vertical component of our velocity
                            CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);

                            // then, add the jumpSpeed value upwards
                            CharacterVelocity += Vector3.up * JumpForce;

                            // anims
                            AnimController.TriggerJump();

                            // play sound
                            AnimController.PlayOneShot(JumpSfx);

                            // remember last time we jumped because we need to prevent snapping to ground for a short time
                            m_LastTimeJumped = Time.time;
                            HasJumpedThisFrame = true;

                            // Force grounding to false
                            IsGrounded = false;
                            m_GroundNormal = Vector3.up;
                        }
                    }

                    // footstep
                }
                else // in air
                {
                    IsRunning = false;
                    IsCrouching = false;

                    // add air acceleration
                    CharacterVelocity += worldspaceMoveInput * AccelerationSpeedInAir * Time.deltaTime;

                    // limit air speed to a maximum, but only horizontally
                    float verticalVelocity = CharacterVelocity.y;
                    Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);
                    CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                    // apply the gravity to the velocity
                    CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
                }
            }

            // apply velocity
            Vector3 capsuleBottomBeforeMove = GetCapsuleBottomCenterPoint();
            Vector3 capsuleTopBeforeMove = GetCapsuleTopCenterPoint(m_Controller.height);
            m_Controller.Move(CharacterVelocity * Time.deltaTime);

            // detect obstructions
            m_LatestImpactSpeed = Vector3.zero;
            if (Physics.CapsuleCast(capsuleBottomBeforeMove,
                capsuleTopBeforeMove,
                m_Controller.radius,
                CharacterVelocity.normalized,
                out RaycastHit hit,
                CharacterVelocity.magnitude * Time.deltaTime,
                _moveCheckLayer,
                QueryTriggerInteraction.Ignore))
            {
                // We remember the last impact speed because the fall damage logic might need it
                m_LatestImpactSpeed = CharacterVelocity;

                CharacterVelocity = Vector3.ProjectOnPlane
                    (CharacterVelocity, hit.normal);
            }
        }

        private void UpdateJump()
        {
            bool wasGrounded = IsGrounded;
            GroundCheck();

            // landing
            if (IsGrounded && !wasGrounded)
            {
                // Fall damage
                float fallSpeed = -Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y);
                float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) /
                                       (MaxSpeedForFallDamage - MinSpeedForFallDamage);
                if (RecievesFallDamage && fallSpeedRatio > 0f)
                {
                    float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                    m_Health.TakeDamage(dmgFromFall, null);

                    // fall damage SFX
                    AnimController.PlayOneShot(GetClip(AllClips.PLAYER_DROP_SCREAM));
                }
                else
                {
                    // land SFX
                    AnimController.PlayOneShot(LandSfx);
                }
            }

            // crouching
            if (_inputHandler.GetCrouchInputDown())
            {
                SetCrouchingState(!IsCrouching, false);
            }
        }

        protected override void UpdateBodyHeight(bool force)
        {
            base.UpdateBodyHeight(force);

            m_Controller.height = _bodyHeight;
            m_Controller.center = _bodyCenter;

            //var camera1PHeight = IsCrouching ?
            //    CameraHeightCrouching
            //    : CameraHeightStanding;

            //var modelPosition = IsCrouching ?
            //    AnimController.ModelPositionCrouch
            //    : AnimController.ModelPositionStand;
            //// Update height instantly
            //if (force)
            //{
            //    m_Controller.height = _targetBodyHeight;
            //    m_Controller.center = Vector3.up * m_Controller.height * 0.5f;

            //    Camera1P_Main.transform.localPosition = camera1PHeight * Vector3.up;
            //    m_Actor.AimPoint.transform.localPosition = m_Controller.center;

            //    // model
            //    _charactersPosition.transform.localPosition
            //        = new Vector3(0, modelPosition, 0);
            //}
            //// Update smooth height
            //else if (m_Controller.height != _targetBodyHeight)
            //{
            //    // resize the capsule
            //    m_Controller.height = Mathf.Lerp(m_Controller.height,
            //        _targetBodyHeight,
            //        CrouchingSharpness * Time.deltaTime);
            //    m_Controller.center = Vector3.up * m_Controller.height * 0.5f;

            //    // camera
            //    Camera1P_Main.transform.localPosition = Vector3.Lerp
            //        (Camera1P_Main.transform.localPosition,
            //        camera1PHeight * Vector3.up,
            //        CrouchingSharpness * Time.deltaTime);
            //    m_Actor.AimPoint.transform.localPosition = m_Controller.center;

            //    // model
            //    _charactersPosition.transform.localPosition = Vector3.Lerp
            //        (_charactersPosition.transform.localPosition,
            //        new Vector3(0, modelPosition, 0),
            //        Time.deltaTime / AnimController.CrouchTransitionTime);
            //}
        }


        private void LateUpdate()
        {
            UpdateCamera();


            // rig is effected by model x scale -1
            //UpdateAimPosition();

            // after camera
            UpdateUpperBodyRotate();
        }

        /// <summary>
        /// picth up/down
        /// </summary>
        private void UpdateUpperBodyRotate()
        {
            // after update camera
            // todo use a new parameter, put in base pawn, or simply use rig for pawn or even player
            var rotateAngle = Mathf.Clamp(m_CameraVerticalAngle,
                -45f,
                45f);

            // Y rotation, rotate by X
            characterModel.spine.transform.localEulerAngles
                = rotateOffset + new Vector3(0, rotateAngle, 0); // Todo: here rotate by x to compensate model x scale -1
            //// spine1
            //characterModel.spine1.transform.localEulerAngles
            //    = new Vector3(0, rotateAngle, 0);
            //// neck
            //characterModel.neck.transform.localEulerAngles
            //    = new Vector3(0, rotateAngle, 0);

            #region refer
            //var dir = aimPos.position - characterModel.spine.transform.position;
            //var newRotation = Quaternion.LookRotation(dir);

            //characterModel.spine.transform.localRotation
            //    = Quaternion.identity;

            //AnimController._animator.SetBoneLocalRotation(HumanBodyBones.Spine, newRotation);
            #endregion
        }

        /// <summary>
        /// SwitchView
        /// </summary>
        /// <param name="isFirstView"></param>
        protected void SwitchView(bool isFirstView)
        {
            Camera1P_Main.enabled = isFirstView;

            Camera3P.enabled = !isFirstView;

            isFirstPerson = isFirstView;

            // todo tps?
            //_weaponsManager.GetCurrentWeapon().ChangeMuzzlePosition(isFirstView);
        }

        #region Update camera
        protected void UpdateCamera()
        {
            var rotationSpeedFinal = RotationSpeed * RotationMultiplier;
            var rotationX = _inputHandler.GetMouseX() * rotationSpeedFinal;
            var rotationY = _inputHandler.GetMouseY() * rotationSpeedFinal;

            #region 1P
            // X rotation, rotate by Y
            transform.Rotate(new Vector3(0f, rotationX, 0f),
                Space.Self);

            // Y rotation, rotate by X
            m_CameraVerticalAngle += rotationY;

            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle,
                MinCameraAngle,
                MaxCameraAngle);

            Camera1P_Main.transform.localEulerAngles
                = new Vector3(m_CameraVerticalAngle, 0, 0);
            #endregion

            #region 3P
            // Camera3P:
            var LockCameraPosition = false;

            if (!LockCameraPosition)
            {
                _cinemachineTargetYaw += rotationX; //_inputHandler.GetMouseX() * rotationSpeedFinal;
                _cinemachineTargetPitch += rotationY; //_inputHandler.GetMouseY() * rotationSpeedFinal;
            }

            // limited 360 degrees
            _cinemachineTargetYaw = AngleHelper.ClampAngle360
                (_cinemachineTargetYaw);
            _cinemachineTargetPitch = AngleHelper.ClampAngle360
                (_cinemachineTargetPitch, MinCameraAngle, MaxCameraAngle);

            // Move Cinemachine Target
            CinemachineCameraTarget.transform.rotation = Quaternion
                .Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
            #endregion
        }

        public float RotationMultiplier
        {
            get
            {
                if (_weaponsManager.IsAiming)
                {
                    return AimingRotationMultiplier;
                }

                return 1f;
            }
        }


        #endregion

        private void UpdateAimPosition()
        {
            if (aimPos != null)
            {
                Ray ray = Camera1P_Main
                    .ViewportPointToRay(Vector3.one * 0.5f);

                Vector3 hitPosition = ray.origin + ray.direction * 100.0f;
                Vector3 newAimPos = hitPosition + aimPosOffset;

                aimPos.position = Vector3.Lerp(aimPos.position,
                    newAimPos,
                    aimSmoothSpeed * Time.deltaTime);
            }
        }


        public void ToggleInventory()
        {
            var tempShowing = uiInventory.isShowing;

            uiInventory.Show(!tempShowing);
            uiEquipment.Show(!tempShowing);

            if (uiInventory.isShowing)
            {
                uiInventory.SetInventory(PawnInventory);
                uiEquipment.SetPawnEquipment(PawnEquipment);
            }
        }

        public bool IsInventoryShowing()
        {
            return uiInventory.isShowing;
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();

            AnimController.XInput = _inputHandler.GetMoveInputX();
            AnimController.YInput = _inputHandler.GetMoveInputY();
            AnimController.MoveSpeed = new Vector3
                (m_Controller.velocity.x, m_Controller.velocity.y, 0)
                .magnitude;

        }

        #region Movement


        void GroundCheck()
        {
            // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
            chosenGroundCheckDistance =
                IsGrounded ?
                (m_Controller.skinWidth + GroundCheckDistance)
                : k_GroundCheckDistanceInAir;

            // reset values before the ground check
            IsGrounded = false;
            m_GroundNormal = Vector3.up;

            // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
            if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
            {
                // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
                if (Physics.CapsuleCast(GetCapsuleBottomCenterPoint(),
                    GetCapsuleTopCenterPoint(m_Controller.height),
                    m_Controller.radius,
                    Vector3.down,
                    out RaycastHit hit,
                    chosenGroundCheckDistance,
                    GroundCheckLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    // storing the upward direction for the surface found
                    m_GroundNormal = hit.normal;

                    // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                    // and if the slope angle is lower than the character controller's limit
                    if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                        IsNormalUnderSlopeLimit(m_GroundNormal))
                    {
                        IsGrounded = true;

                        // handle snapping to the ground
                        if (hit.distance > m_Controller.skinWidth)
                        {
                            m_Controller.Move(Vector3.down * hit.distance);
                        }
                    }
                }
            }
        }

        // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
        bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
        }


        protected override void UpdateRootMotion()
        {
            base.UpdateRootMotion();


            // only update horizontal
            var animMove = new Vector3(_deltaPosition.x, 0, _deltaPosition.z);//_rigid.position += _deltaPosition;
            m_Controller.Move(animMove);
            //CharacterVelocity += animMove / Time.deltaTime;

            _deltaPosition = Vector3.zero;

        }
        #endregion

        #region Crouch
        // Gets the center point of the bottom hemisphere of the character controller capsule    
        Vector3 GetCapsuleBottomCenterPoint()
        {
            return transform.position + (transform.up * m_Controller.radius);
        }

        // Gets the center point of the top hemisphere of the character controller capsule    
        Vector3 GetCapsuleTopCenterPoint(float atHeight)
        {
            return transform.position + (transform.up * (atHeight - m_Controller.radius));
        }

        /// <summary>
        /// Returns false if there was an obstruction
        /// </summary>
        /// <param name="crouched"></param>
        /// <param name="ignoreObstructions"></param>
        /// <returns>false if there was an obstruction</returns>
        bool SetCrouchingState(bool crouched, bool ignoreObstructions)
        {
            // set appropriate heights
            if (crouched)
            {
                _targetBodyHeight = CapsuleHeightCrouching;
            }
            else // standing
            {
                // Detect obstructions
                if (!ignoreObstructions)
                {
                    Collider[] standingOverlaps = Physics.OverlapCapsule(
                        GetCapsuleBottomCenterPoint(),
                        GetCapsuleTopCenterPoint(CapsuleHeightStanding),
                        m_Controller.radius,
                        _moveCheckLayer,
                        QueryTriggerInteraction.Ignore);

                    foreach (Collider c in standingOverlaps)
                    {
                        if (c != m_Controller)
                        {
                            return false;
                        }
                    }
                }

                _targetBodyHeight = CapsuleHeightStanding;
            }

            if (OnStanceChanged != null)
            {
                OnStanceChanged.Invoke(crouched);
            }

            IsCrouching = crouched;
            return true;
        }

        #endregion

        public override bool IsPlayer()
        {
            return true;
        }

        public override EditorLayer GetWeapon1PLayer()
        {
            return EditorLayer.Weapon1P;
        }

        public override EditorLayer GetWeapon3PLayer()
        {
            return EditorLayer.Weapon3P;
        }

        public override Ray GetShotRay()
        {
            // recoil
            // y recoil set in camera recoil
            var spread = (_weaponsManager.spreadThisShot + _weaponsManager.accumulatedBulletRecoil)
                / 180;
            if (_weaponsManager.IsAiming)
            {
                spread /= 2;
            }
            Debug.Log(spread);

            // ray
            Ray ray = (isFirstPerson ? Camera1P_Main : Camera3P) // TPS is off centered
                .ViewportPointToRay
                (Vector3.one * 0.5f + (Vector3)spread);

            return ray;
        }

        protected override void OnDie()
        {
            base.OnDie();

            EventManager.Broadcast(new PlayerDeathEvent());
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            var buttomCenter = GetCapsuleBottomCenterPoint();

            Gizmos.DrawLine(buttomCenter,
                buttomCenter + transform.up * (-chosenGroundCheckDistance));
        }

        public void Respawn(Transform respawnTransform)
        {
            this.transform.position = respawnTransform.position;
            Physics.SyncTransforms();

            IsDead = false;
            AnimController.ResetAnimator();
            _weaponsManager.ChangeToNextWeapon();
            m_Health.ResetHealth();
        }

        // Called in anim?
        public void RespawnFinished()
        {
            //m_Respawning = false;
            //m_Health.isInvulnerable = false;
        }
        // End
    }
}