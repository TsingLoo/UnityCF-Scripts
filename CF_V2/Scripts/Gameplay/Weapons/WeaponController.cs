using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.FPS.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [System.Serializable]
    public struct CrosshairData
    {
        public Sprite CrosshairSprite;
        public int CrosshairSize;
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public class WeaponController : MonoBehaviour
    {
        [Header("Information")]
        public string WeaponName;
        public string WeaponAssetName;

        public GameObject weapon3P { get; set; }// GameObject cannot use get set, turn null when init new copy?
        public bool IsPlayer { get; set; }

        [HideInInspector]
        public WeaponAnimationController AnimController;

        public EWeaponState CurrentState { get; set; }

        [Header("Fire Mode")]
        public EProjectileType BulletType = EProjectileType.Raycast;
        [Tooltip("Amount of bullets per shot")]
        public int BulletsPerShot = 1;
        // todo shot gun
        //public int BulletPellets = 8;

        public bool DisableOnEmpty;

        // weapon position
        [Header("Weapon Recoil")]
        [Range(0f, 2f)]
        public float RecoilForce = 0;//1
        public UnityAction OnWeaponFire;

        // crosshair
        [Header("Crosshair Recoil")]
        public float crosshairRecoilMax = 1f;
        // lower than this may have lag
        public float crosshairRecoverSpeed = 3f;

        // bullet recoil, also effect camera recoil
        #region Bullet position
        // base accuracy
        [Header("Bullet Recoil")]
        public float bulletSpreadAngle = 3f;
        // bullets need to reach bulletSpreadAngle
        public float bulletSpreadAddBullets = 7f;

        public float yRecoilPerShot = 5f;
        public float yRecoilMax = 15f;
        public float yRecoilRecoverSpeed = 30f;
        #endregion
        [Header("Camera Recoil")]
        public float cameraRecoilRecoverSpeed = 30f;
        public float cameraRecoilMax = 15f;

        [Header("Projectile")]
        public ProjectileBase ProjectilePrefab;
        public float ProjectileForce;

        [Header("Aiming")]
        public Vector3 AimOffset;
        [Range(0f, 1f)]
        public float AimZoomRatio = 0.7f;
        public float aimAnimSpeed = 100f; // todo use aim time?

        public GameObject aimScope;
        public bool stopAimWhenFire;
        // mouse sensitive
        public float aimSenseFactor;

        /// <summary>
        /// muzzle use
        /// </summary>
        public Transform WeaponMuzzle;
        public Transform WeaponMuzzle1P;
        public Transform WeaponMuzzle3P;

        [Header("Muzzle Parent")]
        public string MuzzleAttachName;
        public Vector3 MuzzleAttachOffset;
        public bool IsQCModel = false;
        public float ModelScaleFactor = 0.01f;

        [Header("Muzzle Flash")]
        public GameObject MuzzleFlashPrefab;
        public bool UnparentMuzzleFlash;

        // shell
        [Header("Shell")]
        public Transform EjectionPort;
        public GameObject ShellCasing;

        [Range(0.0f, 5.0f)]
        public float ShellCasingEjectionForce = 2.0f;

        public int ShellPoolSize = 100;

        /// <summary>
        /// Shot Timer Out
        /// </summary>
        public bool FireTimerOut { get; private set; } = true;


        int _ammoContent;
        int _ammoCarry;

        public int GetAmmoContent() => _ammoContent;
        public int GetAmmoCarry() => _ammoCarry;

        [Header("Charge")]
        public float MaxChargeDuration = 2f;
        // bow: 1
        public float AmmoUsedOnStartCharge = 0f;
        public float AmmoUsageRateWhileCharging = 1f;

        // bow: false
        public bool FireNeedFullyCharged;

        [Header("Charge - M134")]
        // auto fire when fully chareged
        public bool AutomaticReleaseOnCharged;
        public bool OnlyChargeFirst;

        [Header("Audio")]
        // todo ref
        public AudioClip ShootSfx;

        public AudioClip ChangeWeaponSfx;

        [Tooltip("Continuous Shooting Sound")]
        public bool UseContinuousShootSound = false;
        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuousShootEndSfx;
        AudioSource m_ContinuousShootAudioSource = null;

        bool m_WantsToShoot = false;

        public UnityAction OnShoot;
        public event Action OnShootProcessed;

        public float LastChargeTriggerTimestamp { get; private set; }
        Vector3 m_LastMuzzlePosition;

        /// <summary>
        /// Owner GameObject
        /// </summary>
        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; private set; }
        public float CurrentAmmoRatio { get; private set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        bool _fireChargeDone { get; set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        AudioSource m_ShootAudioSource;

        private Queue<Rigidbody> m_ShellPool;

        #region Load data / read data
        public WeaponData WeaponData { get; set; }

        /// <summary>
        /// 1P / 3P
        /// </summary>
        public List<AnimationClip> AnimClips { get; set; } = new List<AnimationClip>();
        /// <summary>
        /// 1P / 3P
        /// </summary>
        public List<AnimationClip> AnimCombos { get; set; } = new List<AnimationClip>();

        // M134
        public bool HasFireAfter { get; private set; } = false;

        // CSOL
        public bool HasRun { get; private set; } = false;

        bool dataRead = false;
        bool excelRead = false;

        #endregion

        // Grenade
        public bool TriggerHolding { get; set; }
        public GameObject WeaponRoot;

        void Awake()
        {
            //transform.DeepFind(nameof(WeaponRoot));
            WeaponRoot = gameObject.DeepFind(nameof(WeaponRoot));

            if (WeaponAssetName.IsValid())
            {
                LoadAnimations();

                LoadWeaponData();

            }

            AnimController = GetComponent<WeaponAnimationController>();

            // ammo
            _ammoContent = WeaponData.ClipSize;
            _ammoCarry = WeaponData.ClipSize * WeaponData.DefaultClips;

            if (WeaponData.WeaponFireMode == EWeaponFireMode.Charge && MaxChargeDuration == 0)
            {
                MaxChargeDuration = WeaponData.AnimDtos
                    .FirstOrDefault(it => it.AnimNameAffix == AnimNames.FireReady)
                    .RealTime;
            }
            OnShootProcessed += LogShoot;


            m_ShootAudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this,
                gameObject);

            if (UseContinuousShootSound)
            {
                m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
                m_ContinuousShootAudioSource.playOnAwake = false;
                m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
                m_ContinuousShootAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);

                m_ContinuousShootAudioSource.loop = true;
            }

            // init shell pool
            if (EjectionPort != null)
            {
                m_ShellPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    m_ShellPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }

            InitMuzzle();
        }


        void Start()
        {
            // muzzle
            SetWeaponMuzzlePosition();

            // ignore raycast
            SetIgnoreRaycast();
        }

        private void SetIgnoreRaycast()
        {
            m_IgnoredColliders = new List<Collider>();
            // self
            Collider[] selfColliders = GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(selfColliders);
            // owner
            Collider[] ownerColliders = Owner.GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(ownerColliders);
        }



        /// <summary>
        /// Called in WeaponDraw
        /// Show(): gameObject.SetActive(true)
        /// </summary>
        private void OnEnable()
        {
            FireTimerOut = true;

            if (WeaponData.WeaponFireMode == EWeaponFireMode.Charge)
            {
                ResetCharge();
            }
        }

        private void OnDisable()
        {

        }

        #region Load data / read data
        // and anim audios
        private void LoadAnimations()
        {
            #region load anims in folder
            var assetName = WeaponAssetName;

            // todo maybe use
            #region Load from FBX
            //var assetFolder = $"Assets/Resources/Weapons/{assetName}";
            //var assetPath = $"{assetFolder}/{assetName}.fbx";
            //var assetRepresentationsAtPath = AssetDatabase
            //    .LoadAllAssetRepresentationsAtPath(assetPath);
            //foreach (var assetRepresentation in assetRepresentationsAtPath)
            //{
            //    var animationClip = assetRepresentation as AnimationClip;

            //    if (animationClip != null)
            //    {
            //        animationClip.name = GetNewName(assetName, animationClip.name);

            //        _allAnimations.Add(animationClip);
            //    }
            //}
            #endregion

            // load animations
            var changedAnims = Resources
                .LoadAll<AnimationClip>($"Weapons/{assetName}/Animations");
            AnimClips.AddRange(changedAnims);

            // todo del
            //if (HasFirstPerson)
            //{
            //}
            //else
            //{
            //    Anim3Ps.AddRange(changedAnims);
            //}

            // check animations
            HasRun = AnimClips.Exists(it => it.name.EndsWith(AnimNames.Run));
            HasFireAfter = AnimClips.Exists(it => it.name.EndsWith(AnimNames.FireAfter));

            #region Animation Looping (Idle, Run)
            foreach (var animationClip in AnimClips)
            {
                // looping
                if (animationClip.name.EndsWith(AnimNames.Idle)
                    || animationClip.name.EndsWith(AnimNames.Run))
                {
                    animationClip.wrapMode = WrapMode.Loop;

                    AnimationClipSettings settings = AnimationUtility
                        .GetAnimationClipSettings(animationClip);
                    settings.loopTime = true;
                    AnimationUtility
                        .SetAnimationClipSettings(animationClip, settings);
                }
            }
            #endregion

            // combo animations
            var comboAnims = AnimClips
                .Where(it => it.name.Contains(AnimNames.Combo))
                .OrderBy(it => it.name)
                .ToList();
            if (comboAnims.HasValue())
            {
                AnimCombos.AddRange(comboAnims);
            }
            #endregion
        }

        /// <summary>
        /// Read weapon data
        /// </summary>
        private void LoadWeaponData()
        {
            // read excel
            var assetName = WeaponAssetName;

            #region Load Data
            // data

            var dataFilePath = Path.Combine(GlobalConstants.ResourceFolder,
                $"DB/WeaponData" + ExcelHelper.Extension);

            //var dataFilePath = Path.Combine(GlobalConstants.ResourceFolder,
            //    $"Weapons/{assetName}/WeaponData",
            //    assetName + "_Data" + ExcelHelper.Extension);

            var jsonFilePath = Path.Combine(GlobalConstants.ResourceFolder,
                $"DB/WeaponDatas",
                assetName + "_Data" + ".txt");

            if (File.Exists(dataFilePath))
            {
                var dataDto = MiniExcelLibs.MiniExcel
                        .Query<WeaponData>
                        (dataFilePath)
                        .ToList()
                        .FirstOrDefault(it => it.WeaponAssetName == assetName);

                WeaponData = dataDto;
                dataRead = true;
                excelRead = true;
            }
            else
            {
                if (File.Exists(jsonFilePath))
                {
                    var jsonStr = File.ReadAllText(jsonFilePath);
                    WeaponData = JsonUtility.FromJson<WeaponData>(jsonStr);
                    dataRead = true;
                }
                else
                {
                    WeaponData = new WeaponData()
                    {
                        //WeaponName = WeaponName,
                        //WeaponAssetName = WeaponAssetName,
                        //WeaponAnimType = weaponAnimType,
                    };
                }
            }
            #endregion

            #region Load Anims

            // amin1p / anim3P
            var animExcelFilePath = Path.Combine(GlobalConstants.ResourceFolder,
                $"Weapons/{assetName}/WeaponData",
                assetName + "_Anims" + ExcelHelper.Extension);
            Debug.Assert(File.Exists(animExcelFilePath));

            // anim events
            var excelEventsFilePath = Path.Combine(GlobalConstants.ResourceFolder,
                $"Weapons/{assetName}/WeaponData",
                assetName + "_Events" + ExcelHelper.Extension);

            if (File.Exists(animExcelFilePath))
            {
                var clipDtos = MiniExcelLibs.MiniExcel
                    .Query<AnimationClipDto>
                    (animExcelFilePath)
                    .ToList();

                var eventDtoList = new List<AnimationEventDto>();
                if (File.Exists(excelEventsFilePath))
                {
                    eventDtoList = MiniExcelLibs.MiniExcel
                        .Query<AnimationEventDto>
                        (excelEventsFilePath)
                        .ToList();
                }

                foreach (var clipDto in clipDtos)
                {
                    // not in use
                    // set time by frame, data from QC file
                    //if (clipDto.TotalFrame > 0)
                    //{
                    //    clipDto.RealTime = clipDto.TotalFrame / clipDto.FPS;
                    //}

                    // set clip:
                    clipDto.AnimClip = AnimClips
                        .FirstOrDefault(it => it.name.EndsWith(clipDto.AnimNameAffix));
                    // in case reads more dto
                    if (clipDto.AnimClip == null)
                    {
                        Debug.LogError(clipDto.AnimName + " not exist");
                    }

                    // check real time
                    if (clipDto.RealTime == 0)
                    {
                        clipDto.RealTime = clipDto.AnimClip.length;
                        clipDto.Speed = 1;
                    }
                    else
                    {
                        // set speed by time
                        clipDto.Speed = clipDto.AnimClip.GetSpeedByTime(clipDto.RealTime);
                    }

                    clipDto.TotalFrame = clipDto.GetTotalFrame();

                    #region Events in Excel
                    var fullAnimTime = clipDto.RealTime;
                    foreach (var animEventDto in eventDtoList)
                    {
                        // match animation
                        // use short name
                        if (clipDto.AnimName.EndsWith(animEventDto.AnimName))// == clipDto.AnimName
                        {
                            // event time
                            float eventTime = animEventDto.RealTime;
                            animEventDto.TotalFrame = clipDto.TotalFrame;
                            if (animEventDto.Frame > 0 && animEventDto.TotalFrame > 0)
                            {
                                var rate = animEventDto.Frame / animEventDto.TotalFrame;
                                // may cause error in animator
                                Debug.Assert(rate <= 1);

                                eventTime = fullAnimTime * rate;
                            }

                            // string para
                            var stringPara = animEventDto.StringParameter;
                            if (animEventDto.SoundName.IsValid())
                            {
                                stringPara = animEventDto.SoundName;

                                if (animEventDto.FunctionName.IsNotValid())
                                {
                                    animEventDto.FunctionName = nameof(AnimController.PlaySound);
                                }
                            }

                            // float para
                            var floatPara = animEventDto.FloatParameter;
                            if (animEventDto.SoundVolume != 0)
                            {
                                floatPara = animEventDto.SoundVolume;
                            }

                            AnimationEvent animEvent = new AnimationEvent
                            {
                                // since anim speeds up, so the eventTime should also update
                                time = eventTime * clipDto.Speed,
                                functionName = animEventDto.FunctionName,

                                // Unity only accept 1 para
                                stringParameter = animEventDto.AnimName
                                    + ":"
                                    + stringPara,
                                floatParameter = floatPara
                            };

                            clipDto.AnimClip.AddEvent(animEvent);
                            clipDto.AnimEventDtos.Add(animEventDto);
                        }
                    }
                    #endregion

                    #region Other Events
                    // reload
                    if (clipDto.AnimClip.name.EndsWith(AnimNames.Reload))
                    {
                        AnimationEvent animEvent = new AnimationEvent
                        {
                            functionName = nameof(FinishReload),
                            time = clipDto.AnimClip.length * 0.9f,
                        };

                        clipDto.AnimClip.AddEvent(animEvent);
                    }
                    #endregion
                }

                WeaponData.AnimDtos = clipDtos;
            }
            else
            {
                //await CreateWeaponAnimDataAsync();
            }
            #endregion

            // load sounds
            WeaponData.AudioClips = Resources
                .LoadAll<AudioClip>($"Weapons/{assetName}/Sounds")
                .ToList();

            // set data
            if (dataRead)
            {
                //WeaponType = WeaponData.WeaponType;
                //FireMode = WeaponData.WeaponFireMode;
                ////WeaponBagPos = WeaponData.WeaponBagPos;
                //WeaponAnimType = WeaponData.WeaponAnimType;

                //FireGap = WeaponData.FireGap;
                //HeavyGap = WeaponData.HeavyGap;
                //HasAim = WeaponData.HasAim;
                //HasHeavy = WeaponData.HasHeavy;

                //ClipSize = WeaponData.ClipSize;
                //DefaultClips = WeaponData.DefaultClips;
                //MaxClips = WeaponData.MaxClips;
            }

            // prepare data
            {
                // fire gap
                // control by animation
                if (WeaponData.FireGap == 0)
                {
                    Debug.LogWarning(WeaponAssetName + ", fire gap is 0");

                    var fireAnim = WeaponData.AnimDtos
                        .FirstOrDefault(it =>
                            it.AnimName.EndsWith(AnimNames.Fire)
                            || it.AnimName.EndsWith(AnimNames.Combo1));
                    Debug.Assert(fireAnim != null);

                    WeaponData.FireGap = fireAnim.RealTime;
                }

                if (WeaponData.HasHeavy
                    && WeaponData.HeavyGap == 0)
                {
                    var heavyAnim = WeaponData.AnimDtos
                        .FirstOrDefault(it =>
                            it.AnimName.EndsWith(AnimNames.Heavy));
                    Debug.Assert(heavyAnim != null);

                    WeaponData.HeavyGap = heavyAnim.RealTime;
                }
            }

            #region create a new excel
            if (!excelRead)
            {
                // todo
                ExcelHelper.SaveAsReplaceAsync
                    (Path.Combine(GlobalConstants.TempFolder,
                        assetName + "_Data" + ExcelHelper.Extension),
                        new List<WeaponData>() { WeaponData });
            }

            //_weapon.OwnerPawn != null
            //    && _weapon.OwnerPawn is PlayerController

            // no need to output, all data be read
            #endregion
        }

        #endregion


        public void AddAmmo(int count)
        {
            _ammoCarry += count;
        }

        // todo could be simplify to 1: EjectShell()
        void EjectShell(int bulletsPerShotFinal = 1)
        {
            if (EjectionPort != null)
            {
                for (int i = 0; i < bulletsPerShotFinal; i++)
                {
                    Rigidbody nextShell = m_ShellPool.Dequeue();

                    nextShell.transform.position = EjectionPort.transform.position;
                    nextShell.transform.rotation = EjectionPort.transform.rotation;
                    nextShell.gameObject.SetActive(true);
                    nextShell.transform.SetParent(null);
                    nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

                    m_ShellPool.Enqueue(nextShell);
                }
            }
        }

        void PlaySFX(AudioClip sfx)
        {
            AudioUtility.CreateSFX(sfx,
                transform.position,
                AudioUtility.AudioGroups.WeaponShoot, 0.0f);
        }

        public void FinishReload()
        {
            int chargeInClip = Mathf.Min(_ammoCarry,
                WeaponData.ClipSize - _ammoContent);
            _ammoContent += chargeInClip;

            AddAmmo(-chargeInClip);
        }

        public void WeaponReload()
        {
            //the state will only change next frame
            CurrentState = EWeaponState.Reload;
            if (AnimController)
            {
                AnimController.TriggerReload();
            }

            ResetCharge();
        }

        public int GetRemainingAmmo()
        {
            return _ammoContent + _ammoCarry;
        }

        void Update()
        {
            UpdateWeaponState();

            UpdateCharge();

            UpdateContinuousShootSound();
            UpdateMuzzle();
        }

        private void UpdateMuzzle()
        {
            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = WeaponMuzzle.position;
            }

        }

        private void UpdateWeaponState()
        {
            EWeaponState newState;
            newState = AnimController.GetWeaponState();

            if (newState != CurrentState)
            {
                CurrentState = newState;
            }
        }

        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    float chargeLeft = 1f - CurrentCharge;

                    // Calculate how much charge ratio to add this frame
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }

                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                    // See if we can actually add this charge
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= _ammoContent)
                    {
                        // Use ammo based on charge added
                        UseAmmo(ammoThisChargeWouldRequire);

                        // set current charge ratio
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        void UpdateContinuousShootSound()
        {
            if (UseContinuousShootSound)
            {
                if (m_WantsToShoot && _ammoContent >= 1f)
                {
                    if (!m_ContinuousShootAudioSource.isPlaying)
                    {
                        m_ShootAudioSource.PlayOneShot(ShootSfx);
                        m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                        m_ContinuousShootAudioSource.Play();
                    }
                }
                else if (m_ContinuousShootAudioSource.isPlaying)
                {
                    m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                    m_ContinuousShootAudioSource.Stop();
                }
            }
        }

        public void WeaponDraw(bool show)
        {
            // draw
            if (show)
            {
                // 1p
                this.Show();
                //FireTimerOut = true;//set in onEnable
                // 3p
                weapon3P.Show();


                CurrentState = EWeaponState.Draw;
                if (AnimController)
                {
                    AnimController.TriggerDraw();
                }

                if (ChangeWeaponSfx)
                    m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
            }
            // put away
            else
            {
                if (AnimController)
                {
                    AnimController.TriggerPutAway();
                }

                // 1p
                this.Hide();
                // 3p
                weapon3P.Hide();
            }

            IsWeaponActive = show;
        }

        private object GetAmmo()
        {
            return _ammoCarry;
        }

        public void UseAmmo(int amount)
        {
            _ammoContent -= amount;

            if (_ammoContent <= 0)
            {
                _ammoContent = 0;
            }
        }

        public void UseAmmo(float amount)
        {
            UseAmmo(Mathf.FloorToInt(amount));
        }

        /// <summary>
        /// HandleFire / WeaponFire
        /// </summary>
        /// <param name="inputDown"></param>
        /// <param name="inputHeld"></param>
        /// <param name="inputUp"></param>
        /// <returns></returns>
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            m_WantsToShoot = inputDown || inputHeld;
            switch (WeaponData.WeaponFireMode)
            {
                case EWeaponFireMode.Semi:
                    {
                        if (inputDown)
                        {
                            return TryShoot();
                        }

                        return false;
                    }


                case EWeaponFireMode.Auto:
                    {
                        if (inputHeld)
                        {
                            return TryShoot();
                        }

                        return false;

                    }

                case EWeaponFireMode.Charge:
                    {
                        if (inputHeld)
                        {
                            // M134
                            if (OnlyChargeFirst && _fireChargeDone)
                            {
                                return TryShoot();
                            }
                            else
                            {
                                TryBeginCharge();
                            }
                        }

                        // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                        if (inputUp
                            || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                        {
                            // M134
                            if (CurrentCharge >= 1f)
                            {
                                _fireChargeDone = true;
                            }
                            if (inputUp)
                            {
                                _fireChargeDone = false;
                            }

                            return TryReleaseCharge();
                        }

                        return false;

                    }

                default:
                    return false;
            }
        }

        public bool TryShoot()
        {
            if (CanFire())
            {
                HandleShoot();

                FireTimerOut = false;
                StartCoroutine(ShootDelay(WeaponData.FireGap));

                return true;
            }

            return false;
        }

        private bool CanFire()
        {
            return FireTimerOut
                && HasAmmoInWeapon()
                && (CurrentState == EWeaponState.Idle || CurrentState == EWeaponState.FireReady);
        }


        public bool TryHeavyAttack()
        {
            if (CanFire())
            {
                HandleHeavyAttack();

                FireTimerOut = false;
                StartCoroutine(ShootDelay(WeaponData.HeavyGap));

                return true;
            }

            return false;
        }

        public void HandleHeavyAttack()
        {
            // attack call in MeleeWeaponHeavy
            // todo heavy state
            CurrentState = EWeaponState.Fire;
            if (AnimController)
            {
                AnimController.TriggerHeavy();
            }
        }


        IEnumerator ShootDelay(float waitForTime)
        {
            yield return new WaitForSeconds(waitForTime);

            FireTimerOut = true;

            // Debug.Log(WeaponAssetName + ": fire reset");
        }

        bool TryBeginCharge()
        {
            if (!IsCharging
                && CanFire()
                && _ammoContent >= AmmoUsedOnStartCharge)
            {
                UseAmmo(AmmoUsedOnStartCharge);

                CurrentState = EWeaponState.FireReady;
                if (AnimController)
                {
                    AnimController.TriggerFireReady();
                }
                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        private bool HasAmmoInWeapon()
        {
            return WeaponData.WeaponType == EWeaponType.Melee
                || _ammoContent > 0;
        }

        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                return ReleaseCharge();
            }

            return false;
        }

        private bool ReleaseCharge()
        {
            var hasFired = false;

            if (FireNeedFullyCharged && CurrentCharge >= 1f // M134
                || !FireNeedFullyCharged)
            {
                HandleShoot();
                hasFired = true;
            }

            CurrentCharge = 0f;
            IsCharging = false;

            return hasFired;
        }

        private void ResetCharge()
        {
            CurrentCharge = 0f;
            IsCharging = false;

            // M134
            _fireChargeDone = false;
        }

        /// <summary>
        /// WeaponFire
        /// </summary>
        void HandleShoot()
        {
            CurrentState = EWeaponState.Fire;
            // animation
            if (AnimController)
            {
                AnimController.TriggerFire();
                if (OnWeaponFire != null)
                {
                    OnWeaponFire.Invoke();
                }
            }

            // bullets
            int bulletsPerShotFinal = GetBulletsPerShotFinal();
            UseAmmo(bulletsPerShotFinal);

            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                if (BulletType == EProjectileType.Raycast)
                {
                    ShotRaycast(WeaponData.Damage);
                }
                else
                {
                    ShotProjectile();
                }
            }

            // shell, todo 1 shell 8 pellets?
            EjectShell(bulletsPerShotFinal);

            // muzzle flash
            if (MuzzleFlashPrefab != null)
            {
                GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
                    WeaponMuzzle.rotation, WeaponMuzzle.transform);
                // Unparent the muzzleFlashInstance
                if (UnparentMuzzleFlash)
                {
                    muzzleFlashInstance.transform.SetParent(null);
                }

                // if show after draw, destroy in weapon on disable
                Destroy(muzzleFlashInstance, 0.1f);
            }

            // sound
            if (!UseContinuousShootSound)
            {
                if (AnimController)
                {
                    // call in events
                    //AnimController.PlaySoundClip(AnimNames.Fire);
                }
                //todo remove, enemy use new weapon
                else if (ShootSfx != null)
                {
                    m_ShootAudioSource.PlayOneShot(ShootSfx);
                }
            }

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        private void ShotProjectile()
        {
            Ray ray = GetPawnShotRay();
            var shotDirection = ray.direction;

            ProjectileBase newProjectile = Instantiate
                (ProjectilePrefab,
                WeaponMuzzle.position,
                Quaternion.LookRotation(shotDirection));
            newProjectile.Shoot(this);
        }

        #region Raycast
        const float _raycastRange = 1000;
        protected List<Collider> m_IgnoredColliders;

        void ShotRaycast(int damage,
            float raycastDistance = _raycastRange)
        {
            Debug.Assert(raycastDistance > 0);

            Ray ray = GetPawnShotRay();
            // debug ray
            //Debug.DrawLine(ray.origin, ray.origin + 1000 * ray.direction, Color.yellow, 5f);

            #region Hit Detect
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // line cast
            LayerMask shootLayer = LayerHelper.GetAllLayer();
            RaycastHit[] hits = Physics.RaycastAll(ray,
                raycastDistance,
                shootLayer,
                QueryTriggerInteraction.Collide);

            // todo penatrate
            List<RaycastHit> validHits = new List<RaycastHit>();

            foreach (var hit in hits)
            {
                if (IsValidHit(hit)
                    && hit.distance <= closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;

                    validHits.Add(hit);
                    // debug hit
                    Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1f);
                }
            }

            if (foundHit)
            {
                OnHit(closestHit.point,
                    closestHit.normal,
                    closestHit.collider,
                    damage);
            }
            #endregion
        }

        void OnHit(Vector3 point,
            Vector3 normal,
            Collider collider,
            int damage)
        {
            Damageable damageable = collider.GetComponentInParent<Damageable>();
            if (damageable)
            {
                damageable.HandleDamage(damage, false, this.gameObject);
            }

            // impact vfx
            var tag = collider.tag;

            var ImpactVfx = GameFlowManager.Instance.BulletVFXs
                .FirstOrDefault(it => it.name.Contains(tag));
            var ImpactVfxLifetime = GameFlowManager.Instance.BulletVfxTime;
            var ImpactVfxSpawnOffset = GameFlowManager.Instance.BulletHoleOffset;

            // todo default
            if (ImpactVfx == null)
            {
                ImpactVfx = GameFlowManager.Instance.BulletVFXs
                    .FirstOrDefault(it => it.name.Contains(AllMaterials.Metal));
            }

            if (ImpactVfx)
            {
                var spawnPosition = point + (normal * ImpactVfxSpawnOffset);

                // vfx
                GameObject impactVfxInstance = Instantiate(ImpactVfx,
                    spawnPosition,
                    Quaternion.LookRotation(normal));
                if (ImpactVfxLifetime > 0)
                {
                    Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
                }
                
                // hole
                // todo ref
                var bulletHole = GameFlowManager.Instance.BulletHoles
                    .FirstOrDefault(it => it.name.Contains(AllMaterials.Metal));
                // todo add random rotate
                GameObject bulletHoleNew = Instantiate(bulletHole,
                    spawnPosition,
                    Quaternion.LookRotation(normal));
                bulletHoleNew.SelfDestroy(GameFlowManager.Instance.BulletHoleTime);
            }

            // todo blood fx

            // todo
            // sound
            //if (ImpactSfxClip)
            //{
            //    AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
            //}
        }


        protected bool IsValidHit(RaycastHit hit)
        {
            if (!hit.collider.GetComponent<IgnoreHitDetection>()
                && !m_IgnoredColliders.Contains(hit.collider)
                && !IsSameTeam(hit))
            {
                return true;
            }

            return false;
        }

        private bool IsSameTeam(RaycastHit hit)
        {
            var isSameTeam = false;

            var hitActor = hit.collider.GetComponentInParent<Actor>();
            if (hitActor != null)
            {
                var actor = Owner.GetComponentInParent<Actor>();
                if (actor != null
                    && actor.Team == hitActor.Team)
                {
                    isSameTeam = true;
                }
            }

            return isSameTeam;
        }


        #endregion

        private Ray GetPawnShotRay()
        {
            return Owner.GetComponent<PawnController>().GetShotRay();
        }


        private int GetBulletsPerShotFinal()
        {
            int bulletsPerShotFinal = BulletsPerShot;

            // normal charge weapon, calculate bullet use
            if (WeaponData.WeaponFireMode == EWeaponFireMode.Charge
                && !OnlyChargeFirst
                && WeaponData.WeaponType != EWeaponType.Grenade)
            {
                bulletsPerShotFinal = Mathf
                    .CeilToInt(CurrentCharge * BulletsPerShot);
            }

            return bulletsPerShotFinal;
        }

        // simple weapon
        //public Vector3 GetWeaponShotDirection(Transform shootTransform)
        //{
        //    float spreadAngleRatio = BulletSpreadAngle / 180f;// / fov
        //    Vector3 spreadWorldDirection = Vector3.Slerp
        //        (shootTransform.forward,
        //        UnityEngine.Random.insideUnitSphere,
        //        spreadAngleRatio);

        //    return spreadWorldDirection;
        //}

        internal bool IsLastFire()
        {
            return _ammoContent == 1 && _ammoCarry == 0;
        }

        public bool CanReload()
        {
            return CurrentState == EWeaponState.Idle
                && _ammoContent < WeaponData.ClipSize
                    && _ammoCarry > 0;
        }

        internal void MeleeAttack()
        {
            ShotRaycast(WeaponData.Damage, WeaponData.MeleeRangeFire);
        }

        internal void MeleeHeavyAttack()
        {
            ShotRaycast(WeaponData.DamageHeavy, WeaponData.MeleeRangeHeavy);
        }

        public bool NeedAutoReload()
        {
            return WeaponData.WeaponType != EWeaponType.Melee
                && GetAmmoContent() <= 0;
        }

        public bool CanAim()
        {
            return WeaponData.HasAim
                && CurrentState == EWeaponState.Idle;
        }

        /// <summary>
        /// ref
        /// need to control show
        /// </summary>
        /// <param name="weapon3PObj"></param>
        public void SetWeapon3P(ref GameObject weapon3PObj)
        {
            weapon3P = weapon3PObj;

            SetMuzzle3P();
        }

        #region Init muzzle
        // awake:
        private void InitMuzzle()
        {
            // default weapon1p position (not final position)
            WeaponMuzzle1P = this.transform
                    .DeepFind(GlobalConstants.WeaponMuzzle);
            // 3p called when add new weapon

            WeaponMuzzle = WeaponMuzzle1P;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }

        // start:
        private void SetWeaponMuzzlePosition()
        {
            // init position
            if (IsPlayer)
            {
                SetMuzzle1P();
                WeaponMuzzle = WeaponMuzzle1P;
            }
            // bot : called in SetWeapon3P
        }

        // called when add new weapon, SetWeapon3P
        /// <summary>
        /// set position
        /// </summary>
        public void SetMuzzle3P()
        {
            WeaponMuzzle3P = weapon3P.transform
                    .DeepFind(GlobalConstants.WeaponMuzzle);

            if (!IsPlayer)
            {
                WeaponMuzzle = WeaponMuzzle3P;
            }
        }

        /// <summary>
        /// set position
        /// </summary>
        private void SetMuzzle1P()
        {
            // set by offset
            if (MuzzleAttachName.IsValid()
                && MuzzleAttachOffset != null)
            {
                var muzzleAttachParent = this.transform
                    .DeepFind(MuzzleAttachName);
                if (muzzleAttachParent)
                {
                    WeaponMuzzle1P.SetParent(muzzleAttachParent, false);
                    WeaponMuzzle1P.ResetLocalTransform();

                    var transformScale = ModelScaleFactor * ModelScaleFactor;
                    var scaleFactor = transformScale * new Vector3(1f, 1f, 1f);
                    if (IsQCModel)
                    {
                        scaleFactor = transformScale * new Vector3(-1f, 1f, 1f);
                    }

                    WeaponMuzzle1P.localPosition = new Vector3(
                        MuzzleAttachOffset.x * scaleFactor.x,
                        MuzzleAttachOffset.y * scaleFactor.y,
                        MuzzleAttachOffset.z * scaleFactor.z);
                }
            }
        }

        #endregion

        void LogShoot()
        {
            //Debug.Log(WeaponAssetName + ": fired");
        }

        // todo bot
        public void ChangeMuzzlePosition(bool isFirstView)
        {
            if (isFirstView)
            {
                WeaponMuzzle = WeaponMuzzle1P;
            }
            else if (WeaponMuzzle3P != null)
            {
                WeaponMuzzle = WeaponMuzzle3P;
            }
        }

        public bool HasFPSWeapon()
        {
            return !WeaponData.IsRPGWeapon;
        }

        public bool HasRecoil()
        {
            return WeaponData.WeaponType != EWeaponType.Melee
                && WeaponData.WeaponType != EWeaponType.Grenade;
        }

        // End
    }
}