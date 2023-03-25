using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponAnimationController : MonoBehaviour
{
    #region Init
    protected Animator _animator;
    protected int weaponLayer = 0;

    WeaponController _weapon;
    WeaponData _weaponData;
    // legacy code

    [Header("Animations")]
    public WeaponAnimSpeed animSpeedData;

    [HideInInspector] public float drawAnimSpeed;
    [HideInInspector] public float reloadAnimSpeed;
    [HideInInspector] public float fireAnimSpeed;
    [HideInInspector] public float heavyAnimSpeed;
    [HideInInspector] public float runAnimSpeed;

    [HideInInspector] public bool isFiring;

    /// <summary>
    /// fbx animator
    /// </summary>
    Animator _oldAnimator;
    [HideInInspector]
    public AnimatorOverrideController _animOverrideCon;
    List<AnimationClip> _allAnimations = new List<AnimationClip>();
    List<AnimationClip> _comboAnimations = new List<AnimationClip>();

    [Header("Animations - Combos")]
    /// <summary>
    /// cool down time to reset combo click to 0
    /// </summary>
    public float comboCoolDown = 1f;
    [HideInInspector] public int clickCount = 0;

    [Header("Sound")]
    AudioSource _audioSource;
    public float _volume = 1.0f;
    public AudioClip[] _audioClips;


    public float _speed { get; set; } = 0.0f;
    public float _hzInput { get; set; }
    public float _vInput { get; set; }
    public bool _grounded { get; set; }
    public bool _falling { get; set; }

    public bool mouseHolding { get; set; }

    void Awake()
    {
        _weapon = GetComponentInParent<WeaponController>();

        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = _volume;

        InitAnimations();
    }

    private void Update()
    {
        UpdateParameters();

        if (AnimFinished(AnimNames.Heavy))
        {
            _weapon.FinishHeavy();
        }

        if (GameSystem.Instance.ComboTimerOut())
        {
            clickCount = 0;
        }
    }


    // and anim audios
    private void InitAnimations()
    {
        #region load anims in folder
        var assetName = _weapon.WeaponAssetName;
        if (!_allAnimations.HasValue())
        {
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

            // load from folder
            var changedAnims = Resources
                .LoadAll<AnimationClip>($"Weapons/{assetName}/Animations");
            _allAnimations.AddRange(changedAnims);

            #region Looping
            foreach (var animationClip in _allAnimations)
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

            var comboAnims = _allAnimations
                .Where(it => it.name.Contains(AnimNames.Combo))
                .OrderBy(it => it.name)
                .ToArray();
            if (comboAnims.HasValue())
            {
                _comboAnimations.AddRange(comboAnims);
            }
        }
        #endregion
        #region load sounds
        if (!_audioClips.HasValue())
        {
            _audioClips = Resources
                .LoadAll<AudioClip>($"Weapons/{assetName}/Sounds");
        }
        #endregion

        #region Animations Override
        // fbx animator
        var animators = GetComponentsInChildren<Animator>();
        if (animators.Count() > 1)
        {
            _oldAnimator = animators[1];
            _animator.avatar = _oldAnimator.avatar;

            //if (_weaponAnimations.IsEmpty())
            //{
            //    Debug.LogWarning("weapon1P anims not set");

            //    if(_oldAnimator != null)
            //    {
            //        _allAnimations = _oldAnimator
            //            .runtimeAnimatorController
            //            .animationClips
            //            .ToList();
            //    }
            //}
        }

        // override in code (player side in unity editor)
        _animOverrideCon = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _animOverrideCon;

        RelpaceAllAnimations();
        #endregion

        InitAnimData();
    }

    // AWP_Fire
    private string GetNewName(string assetName, string qcName)
    {
        if (qcName.EndsWith("|fire_1")) return assetName + "_" + AnimNames.Fire;

        if (qcName.EndsWith("|idle")) return assetName + "_" + AnimNames.Idle;
        if (qcName.EndsWith("|select")) return assetName + "_" + AnimNames.Draw;
        if (qcName.EndsWith("|reload")) return assetName + "_" + AnimNames.Reload;
        if (qcName.EndsWith("|run")) return assetName + "_" + AnimNames.Run;

        return qcName;
    }

    private async void InitAnimData()
    {
        _weaponData = new WeaponData()
        {
            Name = _weapon.name,
            WeaponType = _weapon.weaponType,
            WeaponAnimType = _weapon.weaponAnimType,
        };

        // init anim1Ps
        var fileName = _weapon.WeaponAssetName;
        var jsonFilePath = Path.Combine(GlobalConstants.DBFolder,
            nameof(WeaponData),
            fileName + FileExts._txt);
        var animExcelFilePath = Path.Combine(GlobalConstants.DBFolder,
            nameof(WeaponData),
            fileName + "_Anims" + ExcelHelper.Extension);

        var excelEventsFilePath = Path.Combine(GlobalConstants.DBFolder,
            nameof(WeaponData),
            fileName + "_Events" + ExcelHelper.Extension);

        // todo add db
        //if (File.Exists(jsonFilePath))
        //{
        //    var jsonStr = File.ReadAllText(jsonFilePath);

        //    _weaponData.Anim1Ps = JsonUtility
        //        .FromJson<List<AnimationClipDto>>(jsonStr);
        //    //_weaponData = JsonConvert
        //    //    .DeserializeObject<WeaponData>(jsonStr);
        //}
        //else

        if (File.Exists(animExcelFilePath))
        {
            var clipDtos = MiniExcelLibs.MiniExcel
                .Query<AnimationClipDto>
                (animExcelFilePath)
                .ToList();

            var eventList = new List<AnimationEventDto>();
            if (File.Exists(excelEventsFilePath))
            {
                eventList = MiniExcelLibs.MiniExcel
                    .Query<AnimationEventDto>
                    (excelEventsFilePath)
                    .ToList();
            }

            foreach (var clipDto in clipDtos)
            {
                // set time by frame, data from QC file
                if (clipDto.TotalFrame > 0)
                {
                    clipDto.RealTime = clipDto.TotalFrame / clipDto.FPS;
                }

                // set speed by time
                clipDto.AnimClip = _allAnimations
                    .FirstOrDefault(it => it.name == clipDto.AnimName);

                // in case reads more dto
                if (clipDto.AnimClip == null)
                {
                    Debug.LogError(clipDto.AnimName + " not exist");
                }

                clipDto.Speed = clipDto.AnimClip.GetSpeedByTime(clipDto.RealTime);
                clipDto.TotalFrame = clipDto.GetTotalFrame();

                #region Add events to anim
                var fullAnimTime = clipDto.RealTime;
                foreach (var animEventDto in eventList)
                {
                    // match animation
                    // use short name
                    if (clipDto.AnimName.EndsWith(animEventDto.AnimName))// == clipDto.AnimName
                    {
                        float eventTime = animEventDto.RealTime;
                        animEventDto.TotalFrame = clipDto.TotalFrame;
                        if (animEventDto.Frame > 0
                            && animEventDto.TotalFrame > 0
                            && fullAnimTime > 0)
                        {
                            //EventRealTime = item.RealTime * (animEvent.time / item.Length),
                            var rate = animEventDto.Frame / animEventDto.TotalFrame;
                            eventTime = fullAnimTime * rate;
                        }

                        var stringPara = animEventDto.StringParameter;
                        if (animEventDto.SoundName.IsValid())
                        {
                            stringPara = animEventDto.SoundName;

                            if (animEventDto.FunctionName.IsNotValid())
                            {
                                animEventDto.FunctionName = nameof(PlaySound);
                            }
                        }

                        var floatPara = animEventDto.FloatParameter;
                        if (animEventDto.SoundVolume != 0)
                        {
                            floatPara = animEventDto.SoundVolume;
                        }
                        AnimationEvent animEvent = new AnimationEvent
                        {
                            // since anim speeds up, so the eventTime should update
                            time = eventTime * clipDto.Speed,
                            functionName = animEventDto.FunctionName,

                            // only accept 1 para
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
            }

            _weaponData.Anim1PDtos = clipDtos;
        }
        else
        {
            await CreateWeaponAnimDataAsync();
        }

        if (_weapon.OwnerPawn != null
            && _weapon.OwnerPawn is PlayerController)
        {
            #region create a new one
            var eventListOutput = new List<AnimationEventDto>();

            foreach (var item in _weaponData.Anim1PDtos)
            {
                if (item.RealTime == 0)
                {
                    item.RealTime = item.AnimClip
                        .GetTimeBySpeed(item.Speed);
                }

                #region Anim events
                var events = item.AnimClip.events;
                // todo mapper
                //var dtos = animEvents.MapTo<AnimationEventDto>();
                foreach (var animEvent in events)
                {
                    item.AnimEventDtos.Add(new AnimationEventDto()
                    {
                        AnimName = item.AnimName,
                        FunctionName = animEvent.functionName,
                        //todo extensions:
                        RealTime = animEvent.time,// item.RealTime * (animEvent.time / item.Length),
                        TotalFrame = item.TotalFrame,

                        StringParameter = animEvent.stringParameter,
                        IntParameter = animEvent.intParameter,
                        FloatParameter = animEvent.floatParameter,
                    });
                }


                eventListOutput.AddRange(item.AnimEventDtos);
                #endregion
            }

            await ExcelHelper.SaveAsReplaceAsync
                (Path.Combine(GlobalConstants.TempFoler,
                    fileName + ExcelHelper.Extension),
                    _weaponData.Anim1PDtos);

            await ExcelHelper.SaveAsReplaceAsync
                (Path.Combine(GlobalConstants.TempFoler,
                    fileName + "_Events" + ExcelHelper.Extension),
                    eventListOutput);
            #endregion
            Debug.Assert(File.Exists(animExcelFilePath), animExcelFilePath);
        }

        // fire gap
        var fireAnim = _weaponData.Anim1PDtos
            .FirstOrDefault(it => it.AnimName.EndsWith(AnimNames.Fire)
                || it.AnimName.EndsWith(AnimNames.Combo1));
        Debug.Assert(fireAnim != null, _weapon);

        // control by animation
        if (fireAnim != null
            && (_weapon.fireGap == 0 || _weapon.weaponType == EWeaponType.Sniper))
        {
            _weapon.fireGap = fireAnim.RealTime;
        }


    }

    private async Task CreateWeaponAnimDataAsync()
    {
        drawAnimSpeed = 1;
        reloadAnimSpeed = 1;
        fireAnimSpeed = 1;
        heavyAnimSpeed = 1;
        runAnimSpeed = 1;

        Debug.LogWarning(_weapon.name
            + ": animSpeed set to 1");

        #region add clips

        Debug.Assert(_allAnimations.HasValue());
        // add clips
        foreach (var item in _allAnimations)
        {
            // set speed
            var speed = 1f;
            var animNameAffix = item.name.Split("_")
                .LastOrDefault();
            switch (animNameAffix)
            {
                case AnimNames.Draw:
                    {
                        speed = drawAnimSpeed;
                        break;
                    }
                case AnimNames.Reload:
                    {
                        speed = reloadAnimSpeed;
                        break;
                    }
                case AnimNames.Fire:
                    {
                        speed = fireAnimSpeed;
                        break;
                    }
                case AnimNames.Heavy:
                    {
                        speed = heavyAnimSpeed;
                        break;
                    }
                default:
                    break;
            }

            var animRealTime = item.GetTimeBySpeed(speed);
            var events = item.events;
            var animEvents = new List<AnimationEventDto>();
            // todo mapper
            //var dtos = animEvents.MapTo<AnimationEventDto>();
            foreach (var animEvent in events)
            {
                animEvents.Add(new AnimationEventDto()
                {
                    FunctionName = animEvent.functionName,
                    //todo extensions:
                    RealTime = animRealTime * (animEvent.time / item.length),
                    Time = animEvent.time,
                    StringParameter = animEvent.stringParameter,
                    IntParameter = animEvent.intParameter,
                    FloatParameter = animEvent.floatParameter,
                });
            }

            _weaponData.Anim1PDtos.Add(new AnimationClipDto()
            {
                AnimName = item.name,
                AnimClip = item,
                Speed = speed,
                RealTime = animRealTime,
                FrameRate = item.frameRate,
                Length = item.length,
                AnimEventDtos = animEvents,
            });
        }
        #endregion

        // combo anim use fire and heavy time
        foreach (var item in _weaponData.Anim1PDtos)
        {
            var animAffx = item.GetNameAffix();
            if (animAffx == AnimNames.Combo1
                || animAffx == AnimNames.Combo2)
            {
                item.RealTime = _weaponData.Anim1PDtos
                    .FirstOrDefault(it => it.GetNameAffix()
                    == AnimNames.Fire)
                    .RealTime;
            }
            else if (animAffx == AnimNames.Combo3)
            {
                item.RealTime = _weaponData.Anim1PDtos
                    .FirstOrDefault(it => it.GetNameAffix()
                    == AnimNames.Heavy)
                    .RealTime;
            }
        }

        // save data, excel
        var fileName = this.name.Replace("(Clone)", "");
        await ExcelHelper.SaveAsReplaceAsync
            (Path.Combine(GlobalConstants.TempFoler,
            fileName + ExcelHelper.Extension),
            _weaponData.Anim1PDtos);
        // json
        var jsonStr = _weaponData.Anim1PDtos.ToJsonString();
        FileHelper.CreateFileReplace(Path.Combine(GlobalConstants.TempFoler,
            fileName + FileExts._txt), jsonStr);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dummyAnimAffix">Idle</param>
    private void RelpaceAllAnimations()
    {
        foreach (var clip in _allAnimations)
        {
            var replacedAnim = _allAnimations
                .Where(it => it.name.EndsWith(clip.GetNameAffix()))
                .FirstOrDefault();
            if (replacedAnim != null)
            {
                _animOverrideCon[Join(AnimNames.Dummy, clip.GetNameAffix())]
                    = replacedAnim;
            }
        }
    }

    #endregion


    // set parameters from weapon
    private void UpdateParameters()
    {
        _animator.SetFloat("Speed", _speed);
        _animator.SetBool("Falling", !_grounded);

        _animator.SetBool("TriggerHolding", _weapon.TriggerHolding);
        _animator.SetInteger("ClickCount", clickCount);

        //_animator.SetBool("FireDone", _weapon.ShotTimerOut());

        //_animator.SetFloat("XInput", _hzInput);
        //_animator.SetFloat("YInput", _vInput);
        //_animator.SetBool("Grounded", _grounded);
    }

    #region Clip events
    /// <summary>
    ///    only accept 1 para
    /// </summary>
    /// <param name="animName_soundName">a:b</param>
    void PlaySound(string animName_soundName)//, float volume
    {
        var animName = animName_soundName.Split(":").FirstOrDefault();
        var soundName = animName_soundName.Split(":").LastOrDefault();
        var clipEventDtos = _weaponData.Anim1PDtos
            .FirstOrDefault(it => it.AnimName.EndsWith(animName))
            .AnimEventDtos;
        var clipEventDto = clipEventDtos
            .FirstOrDefault(it => it.FunctionName == nameof(PlaySound));

        PlaySoundClip(soundName, clipEventDto.FloatParameter);
    }
    #endregion

    #region Audio Clip Events

    void Draw(float volume)
    {
        PlaySoundClip(AnimNames.Draw, volume);
    }

    void MagIn(float volume)
    {
        PlaySoundClip(AnimNames.MagIn, volume);
    }

    void MagOut(float volume)
    {
        PlaySoundClip(AnimNames.MagOut, volume);
    }

    void Pull(float volume)
    {
        PlaySoundClip(AnimNames.Pull, volume);
    }

    /// <summary>
    /// weapon circle around
    /// 左轮Draw或者换完子弹后的旋转
    /// </summary>
    void Circle(float volume)
    {
        PlaySoundClip(AnimNames.Circle, volume);
    }
    #endregion

    #region Functions
    void Fire()
    {
        _weapon.MeleeWeaponAttack();

        PlaySoundClip(AnimNames.Fire);
    }

    void HeavyAttack()
    {
        _weapon.MeleeWeaponAttackHeavy();
    }

    void Heavy()
    {
        _weapon.MeleeWeaponAttackHeavy();

        PlaySoundClip(AnimNames.Heavy);
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animAffix">_Idle</param>
    public void PlaySoundClip(string animAffix, float volume = 0f)
    {
        var volumnUse = _volume;
        if (volume > 0)
        {
            volumnUse = volume;
        }

        var audioClip = _audioClips
            .FirstOrDefault(it => it.name.EndsWith(animAffix));

        if (audioClip != null)
        {
            _audioSource.PlayOneShot(audioClip, volumnUse);

            // has bug playing multi times
            //_audioSource.clip = audioClip;
            //_audioSource.Play();
        }
    }

    public void PlayFootstep()
    {
        _weapon.OwnerPawn.PlayFootstep();
    }

    #region Hank cf legacy code for resource refecence
    public void gj(int id)
    {
        //weapon.gj(id);
    }
    /// <summary>
    /// PlaySound
    /// </summary>
    /// <param name="id"></param>
    public void bfsx(int id)
    {
        //weapon.playSound(id);
    }
    public void jczd()
    {
        //weapon.jczd();
    }

    public void jczd(int id) { }

    public void szsd(float id)
    {
        //weapon.szsd(id);
    }
    public void jszd()
    {
        //weapon.jszd();
    }
    public void over()
    {
        //weapon.over();
    }

    #endregion




    internal EWeaponState GetWeaponState()
    {
        var newState = EWeaponState.Idle;

        if (!_weapon.ShotTimerOut() // IsAnimState(AnimNames.Fire)
            || IsAnimState(AnimNames.Heavy))
        {
            newState = EWeaponState.Firing;
        }
        else if (IsAnimState(AnimNames.Reload))
        {
            newState = EWeaponState.Reloading;
        }
        else if (IsAnimState(AnimNames.Draw))
        {
            newState = EWeaponState.Drawing;
        }


        return newState;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animAffix">_Idle</param>
    /// <returns></returns>
    private bool IsAnimState(string animAffix)
    {
        int stateNameHash = Animator.StringToHash
            (Join(AnimNames.Dummy, animAffix));

        var animatorInfo = _animator
            .GetCurrentAnimatorStateInfo(weaponLayer);

        return animatorInfo.shortNameHash == stateNameHash;
    }

    internal bool ReloadFinished()
    {
        return AnimFinished(AnimNames.Reload);
    }

    public bool AnimFinished(string animNameAffix)
    {
        var stateInfo = _animator
            .GetCurrentAnimatorStateInfo(weaponLayer);

        if (stateInfo.IsName(Join(AnimNames.Dummy,
            animNameAffix))
            && stateInfo.normalizedTime >= 0.9f)
        {
            return true;
        }

        return false;
    }

    public bool AnimPlaying(string animNameAffix, float normalizedTime)
    {
        var stateInfo = _animator
            .GetCurrentAnimatorStateInfo(weaponLayer);

        if (stateInfo.IsName(Join(AnimNames.Dummy,
            animNameAffix))
            && stateInfo.normalizedTime >= normalizedTime)
        {
            return true;
        }

        return false;
    }


    public bool AnimPlaying(string animNameAffix)
    {
        var stateInfo = _animator
            .GetCurrentAnimatorStateInfo(weaponLayer);

        if (stateInfo.IsName(Join(AnimNames.Dummy, animNameAffix)))
        {
            return true;
        }

        return false;
    }

    #region Trigger anims
    internal void TriggerReload()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.Reload),
            weaponLayer);

        _weapon.OwnerPawn.animController.TriggerReload();

        PlaySoundClip(AnimNames.Reload);
    }



    internal void TriggerFire()
    {
        // trigger combo
        if (_comboAnimations.Count > 0)
        {
            TriggerCombo();
        }
        else // no combo, normal fire
        {

            if (_weapon.IsLastFire())
            {
                _animator.Play(Join(AnimNames.Dummy, AnimNames.FireLast),
                    weaponLayer);
            }
            else
            {
                // when fireGap is small,
                // fire animation is not over, play from beginning
                _animator.Play(Join(AnimNames.Dummy, AnimNames.Fire),
                    weaponLayer,
                    normalizedTime: 0f);
            }
            _weapon.OwnerPawn.animController.TriggerFire();

        }
    }

    internal void TriggerPreFire()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.PreFire),
               weaponLayer);
        //_weapon.OwnerPawn.animController.TriggerPreFire();

        // sound called in events
    }


    private void TriggerCombo()
    {
        // reset timer
        GameSystem.Instance._comboTimer = comboCoolDown;

        PlayAnimClip(_comboAnimations[clickCount]);
        clickCount++;

        if (clickCount >= _comboAnimations.Count)
        {
            clickCount = 0;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    internal void TriggerDraw()
    {
        // draw animation auto called in animator
        //_animator.Play(AnimAffix.Dummy + AnimAffix.Draw,
        //    weaponLayer);

        // init animations for player (no draw anim on pawn)

        this.SwitchWeaponAnims();

        // called via data
        //PlaySoundClip(AnimNames.Draw);
    }

    /// <summary>
    /// attack called in void Heavy()
    /// </summary>
    public void TriggerHeavy()
    {
        // weapon anim
        _animator.Play(Join(AnimNames.Dummy, AnimNames.Heavy),
            weaponLayer);

        // player anim
        _weapon.OwnerPawn.animController.TriggerHeavy();
    }

    internal void TriggerPutAway()
    {
        _animator.WriteDefaultValues();
    }

    internal AnimationClip GetAnim(string animNameAffix)
    {
        return _animOverrideCon.animationClips
            .FirstOrDefault(it => it.name.Contains(animNameAffix));
    }


    /// <summary>
    /// Set after Draw, since parameters in Animator 
    /// should be overriten for each weapon
    /// </summary>
    public void SwitchWeaponAnims()
    {
        ChangeAnimSpeed1P();

        _weapon.OwnerPawn.animController
            .ChangeAnim3P(_weaponData);
    }

    private void ChangeAnimSpeed1P()
    {
        foreach (var clipDto in _weaponData.Anim1PDtos)
        {
            var animSpeed = clipDto.Speed;
            //.AnimClip.GetSpeedByTime(clipDto.RealTime);

            SetDummyAnimSpeed(clipDto.GetNameAffix(), animSpeed);
        }
    }



    #endregion

    #region Helper function
    public void SetDummyAnimSpeed(string animNameAffix, float animSpeed)
    {
        SetAnimSpeed
            (Join(AnimNames.AnimSpeed, AnimNames.Dummy, animNameAffix)
                , animSpeed);
    }

    public void SetAnimSpeed(string animSpeedName, float speedValue)
    {
        _animator.SetFloat(animSpeedName, speedValue);
    }

    public void PlayAnimState(string animStateName)
    {
        _animator.Play(animStateName,
            weaponLayer);
    }

    protected void PlayAnimClip(AnimationClip animationClip)
    {
        // Dummy_Combo1
        var stateName = Join(AnimNames.Dummy,
            animationClip.GetNameAffix());

        _weapon.OwnerPawn.animController.PlayAnimState(stateName);
        this.PlayAnimState(stateName);
    }
    #endregion

    #region Join
    protected string Join(string a, string b)
    {
        return AnimNames.Combine(a, b);
    }

    protected string Join(string a, string b, string c)
    {
        return AnimNames.Combine(a, b, c);
    }

    internal bool IsInCombo()
    {
        // todo use isInCombo, set to false when combo end
        return false;
    }

    #endregion
}
