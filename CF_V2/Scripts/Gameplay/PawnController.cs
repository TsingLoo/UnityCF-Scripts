using System;
using System.Collections;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class PawnController : BaseBehaviour
    {
        public string PawnName;

        public PawnInventory PawnInventory;
        public PawnEquipment PawnEquipment;

        protected PawnWeaponsManager _weaponsManager;
        [HideInInspector] public Transform WeaponPosition3P;

        [Range(0.1f, 1f)]
        [Tooltip("Rotation speed multiplier when aiming")]
        public float AimingRotationMultiplier = 0.6f;
        [Header("Camera1P")]
        public Camera Camera1P_Main;
        // camera1p weapon set in weapons manager

        [Header("Camera3P")]
        public Camera Camera3P;
        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;
        public float CameraHeightCrouching = 1.45f;

        [Header("Stance")]
        public float CameraHeightStanding = 1.7f;
        public float CapsuleHeightCrouching = 0.9f;
        public float CapsuleHeightStanding = 1.8f;
        //todo
        //public CinemachineVirtualCamera Camera3PController;
        // cinemachine
        public GameObject CinemachineCameraTarget;

        [Tooltip("Speed of crouching transitions")]
        public float CrouchingSharpness = 15f;
        public float FallDamageAtMaxSpeed = 30f;

        public float FallDamageAtMinSpeed = 10f;

        [Header("Audio Clips")]
        public AudioClip FootstepSfx;

        [Header("Audio")]
        [Tooltip("Amount of footstep sounds 1 meter")]
        public float FootstepSfxFrequency = 0.3f;
        public float FootstepSfxFrequencyWalk = 0.2f;

        [Header("General")]
        [Tooltip("Force applied downward when in the air")]
        public float GravityDownForce = 20f;

        [Tooltip("Physic layers checked to consider the player grounded")]
        public LayerMask GroundCheckLayers = -1;

        #region Jump
        [Header("Jump")]
        [Tooltip("Force applied upward when jumping")]
        public float JumpForce = 9f;
        public float AccelerationSpeedInAir = 0f;

        public AudioClip JumpSfx;
        public AudioClip LandSfx;

        [Tooltip("Height at which the player dies instantly when falling off the map")]
        public float KillHeight = -50f;

        // todo ref
        protected const float k_JumpGroundingPreventionTime = 0.2f;
        protected float m_LastTimeJumped = 0f;
        protected Vector3 m_LatestImpactSpeed;
        #endregion

        // move / footstep
        protected Vector3 m_CharacterVelocity;
        protected float m_FootstepDistanceCounter;


        [Tooltip("Max movement speed when crouching")]
        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;
        [Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
        public float MaxSpeedForFallDamage = 40f;

        [Tooltip("Max movement speed when not grounded")]
        public float MaxSpeedInAir = 10f;

        [Header("Movement")]
        public float MaxSpeedOnGround = 10f;
        public float MinSpeedForFallDamage = 25f;

        [Tooltip("Sharpness for the movement when grounded, " +
            "a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
        public float MovementSharpnessOnGround = 15;

        public UnityAction<bool> OnStanceChanged;

        [Header("Fall Damage")]
        public bool RecievesFallDamage;

        [Header("Rotation")]
        [Tooltip("Rotation speed for moving the camera")]
        public float RotationSpeed = 200f;
        public float SprintSpeedModifier = 1.5f;
        [HideInInspector] public bool useRootMotion = true;

        protected bool isFirstPerson = true;
        protected Transform _charactersPosition;


        /// <summary>
        /// weapon driven root motion
        /// </summary>
        protected Vector3 _deltaPosition;

        /// <summary>
        /// can crouch 
        /// </summary>
        protected LayerMask _moveCheckLayer;
        protected Actor m_Actor;


        protected Health m_Health;
        protected float _bodyHeight;
        protected Vector3 _bodyCenter;
        // slowly change to this value
        protected float _targetBodyHeight;

        public CharacterModel characterModel;

        public PawnAnimationController AnimController { get; protected set; }

        // Player status
        public Vector3 CharacterVelocity { get; set; }
        public bool HasJumpedThisFrame { get; protected set; }
        public bool IsDead { get; protected set; }

        public bool IsWalking { get; set; }
        public bool IsRunning { get; set; }
        public bool IsSprinting { get; set; }

        public bool IsCrouching { get; set; }
        
        public bool IsGrounded { get; set; }



        void Awake()
        {
            Init();

            OnStanceChanged += OnCrouchChanged;

            if (PawnEquipment)
            {
                PawnEquipment.OnEquipmentChanged
                    += PawnEquipment_OnEquipmentChanged;
            }
        }

        protected virtual void Init()
        {
            #region Character
            AnimController = GetComponentInChildren<PawnAnimationController>();
            AnimController.SetOwner(gameObject);

            _charactersPosition = this.transform.DeepFind("Characters");
            characterModel = GetComponentInChildren<CharacterModel>();

            #endregion

            _weaponsManager = GetComponent<PawnWeaponsManager>();
            Debug.Assert(_weaponsManager != null);
            _weaponsManager.IsPlayer = IsPlayer();
            _weaponsManager.Weapon1PLayer = GetWeapon1PLayer();
            _weaponsManager.Weapon3PLayer = GetWeapon3PLayer();

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PawnController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerController>(m_Actor, this, gameObject);
        }

        // todo ref, call from anim?
        protected void UpdateFootStep()
        {
            if (IsGrounded)
            {
                // footsteps sound
                float chosenFootstepSfxFrequency =
                    IsSprinting ?
                    FootstepSfxFrequencyWalk
                    : FootstepSfxFrequency;

                if (m_FootstepDistanceCounter
                    >= 1f / chosenFootstepSfxFrequency)
                {
                    m_FootstepDistanceCounter = 0f;
                    AnimController.PlayOneShot(FootstepSfx);
                }

                // keep track of distance traveled for footsteps sound
                m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
            }
        }


        private void FixedUpdate()
        {
            // todo not in use
            //UpdateRootMotion();
        }

        // Gets a reoriented direction that is tangent to a given slope
        public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        public void OnUpdateRootMotion(object deltaPosition)
        {
            // check state r1 attack
            if (useRootMotion)
            {
                this._deltaPosition += (Vector3)deltaPosition;
            }
        }

        protected virtual void UpdateAnimator()
        {
            AnimController.IsRunning = IsRunning;
            AnimController.IsWalking = IsSprinting;
            AnimController.IsCrouching = IsCrouching;
            AnimController.IsFalling = !IsGrounded;
        }

        protected virtual void UpdateRootMotion()
        {
        }


        protected virtual void SetInventory()
        {
            if (PawnInventory)
            {
                PawnInventory.useItemAction = OnInventoryUseItem;
                PawnInventory.equipItemAction = OnInventoryEquipItem;
            }

            // pawn
            if (PawnEquipment)
            {
                _weaponsManager.SetWeaponItems(PawnEquipment.Items);
            }
            else // bot
            {
                _weaponsManager.SetStartWeapons();
            }

        }

        void OnCrouchChanged(bool crouch) { }

        protected virtual void OnDie()
        {
            if (!IsDead)
            {
                IsDead = true;

                // un draw weapon
                _weaponsManager.ChangeToWeapon(-1, true);

                // anim
                AnimController.PlayFullBodyAnim(BodyAnims.Death1);
                // sound
                AnimController.PlayOneShot(characterModel.voiceAsset.die);
            }
        }

        protected virtual void Start()
        {
            SetWeaponSockets();
            SetInventory();

            // todo ref
            _moveCheckLayer = LayerHelper.GetAllLayer()
                .RemoveLayer(EditorLayer.Player.GetIntValue());


            m_Health.OnDamaged += OnDamaged;
            m_Health.OnKilledBy += OnKilledBy;
            m_Health.OnDie += OnDie;

            UpdateBodyHeight(true);
        }

        // todo duplicate HandR
        // set in unity?
        protected void SetWeaponSockets()
        {
            // set hand r
            if (WeaponPosition3P == null)
            {
                Debug.Assert(characterModel != null);
                var handRSocket = characterModel.sockets
                    .FirstOrDefault(it => it.name == "HandR");

                // (this will duplicate the new object)
                //Instantiate(new GameObject() { name = "HandR" }, handRSocket.node, false);
                var handR = new GameObject() { name = "HandR" };
                handR.transform.parent = handRSocket.node;
                handR.transform.localPosition = handRSocket.offset;
                handR.transform.localRotation = Quaternion.Euler(handRSocket.euler);
                handR.transform.localScale = Vector3.one;

                WeaponPosition3P = handR.transform;
                // todo ref
                _weaponsManager.WeaponPosition3P = WeaponPosition3P;
            }
        }


        protected virtual void OnKilledBy(GameObject damageSource)
        {
            // by player
            if (damageSource.GetComponentInParent<PlayerController>() != null)
            {
                EventManager.Broadcast(new KillMarkEvent());
            }
        }

        protected virtual void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && damageSource != this)
            {
                AnimController.PlayUpperBodyAnim(BodyAnims.Hit_Forward);
            }
        }

        protected virtual void Update()
        {
            // check for Y kill
            if (!IsDead && transform.position.y < KillHeight)
            {
                m_Health.Kill();
            }

            HasJumpedThisFrame = false;

            // update jump

            // update body height

            UpdateMovement();
            UpdateFootStep();

            UpdateAnimator();
        }

        protected virtual void UpdateMovement()
        {
            throw new NotImplementedException();
        }



        #region Bag

        /// <summary>
        /// Weapons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PawnEquipment_OnEquipmentChanged(object sender,
            PawnEquipment.OnEquipmentChangeEventArgs e)
        {
            // todo reset in next level?
            //return;

            var item = e.item;
            var weapon = e.item.Item1P.GetComponent<WeaponController>();

            // add
            if (e.changeType == PawnEquipment.EItemChangeType.Add)
            {
                var oldWeapon = _weaponsManager.GetWeapon(item.BagPosition);
                if (oldWeapon != null)
                {
                    _weaponsManager.RemoveWeapon(item.BagPosition);
                }

                _weaponsManager.AddWeapon(e.item);
            }
            else // remove
            {
                _weaponsManager.RemoveWeapon(item.BagPosition);
            }
        }

        public void OnInventoryUseItem(Item inventoryItem)
        {
            Debug.Log("Use Item: " + inventoryItem);
        }

        public void OnInventoryEquipItem(Item inventoryItem)
        {
            PawnEquipment.TryEquipItem(inventoryItem, force: true);
        }
        #endregion


        protected void ThrowWeapon()
        {
            var slotItem = PawnEquipment
                .GetSlotItem((EWeaponBagPosition)_weaponsManager.CurrentBagPos);

            if (PawnEquipment.TryUnEquipItem(slotItem))
            {
                // new
                var newPickup = Instantiate(slotItem.Item3P, // todo generic ItemPickupPrefab?
                    _weaponsManager.WeaponPosition3P.position,
                    rotation: Quaternion.identity);//todo rotation

                // throw
                newPickup.transform.forward = transform.forward;
                newPickup.GetComponent<Pickup>().Throw();
            }
        }

        public virtual EditorLayer GetWeapon1PLayer()
        {
            return EditorLayer.BotWeapon1P;
        }

        public virtual EditorLayer GetWeapon3PLayer()
        {
            return EditorLayer.BotWeapon3P;
        }

        public virtual bool IsPlayer()
        {
            return false;
        }

        public virtual Ray GetShotRay()
        {
            throw new NotImplementedException();
        }

        // todo ref, update model position only
        // use other way to check can stand
        // camera height in player only
        protected virtual void UpdateBodyHeight(bool force)
        {
            var camera1PHeight = IsCrouching ?
                CameraHeightCrouching
                : CameraHeightStanding;

            var newModelPosition = IsCrouching ?
                AnimController.ModelPositionCrouch
                : AnimController.ModelPositionStand;
            // Update height instantly
            if (force)
            {
                _bodyHeight = _targetBodyHeight;
                _bodyCenter = Vector3.up * _bodyHeight * 0.5f;

                Camera1P_Main.transform.localPosition = camera1PHeight * Vector3.up;
                m_Actor.AimPoint.transform.localPosition = _bodyCenter;

                // model
                _charactersPosition.transform.localPosition
                    = new Vector3(0, newModelPosition, 0);
            }
            // Update smooth height
            else if (_bodyHeight != _targetBodyHeight)
            {
                // resize the capsule
                _bodyHeight = Mathf.Lerp(_bodyHeight,
                    _targetBodyHeight,
                    CrouchingSharpness * Time.deltaTime);
                _bodyCenter = Vector3.up * _bodyHeight * 0.5f;

                // camera
                Camera1P_Main.transform.localPosition = Vector3.Lerp
                    (Camera1P_Main.transform.localPosition,
                    camera1PHeight * Vector3.up,
                    CrouchingSharpness * Time.deltaTime);
                m_Actor.AimPoint.transform.localPosition = _bodyCenter;

                // model
                _charactersPosition.transform.localPosition = Vector3.Lerp
                    (_charactersPosition.transform.localPosition,
                    new Vector3(0, newModelPosition, 0),
                    Time.deltaTime / AnimController.CrouchTransitionTime);
            }
        }

        public AudioClip GetClip(string clipName)
        {
            var sex = characterModel.sex;
            if (sex == CharacterModel.Sex.Woman)
            {
                clipName += "_" + sex.GetCode();
            }

            var clip = Resources.Load<AudioClip>($"Sound/PLAYER/{clipName}");
            return clip;
        }

        // End
    }
}