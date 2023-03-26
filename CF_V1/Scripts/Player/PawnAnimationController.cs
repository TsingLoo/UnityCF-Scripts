using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PawnAnimationController : MonoBehaviour
{
    #region Init
    protected Animator _animator;

    /// <summary>
    /// lower body, should be the 1st
    /// </summary>
    protected int baseLayer = 0;
    /// <summary>
    /// upper body
    /// </summary>
    protected int weaponLayer = 1;
    protected int fullBodyLayer = 2;

    BasePawnController owner_Player;
    public List<AnimatorOverrideController> animOverrideCons;
    [HideInInspector]
    protected AnimatorOverrideController animOverrideCon;

    public float _speed { get; set; } = 0.0f;
    public float _hzInput { get; set; }
    public float _vInput { get; set; }
    public bool _grounded { get; set; }
    public bool _falling { get; set; }

    // death audio clip
    AudioClip death;
    AudioSource audioSource;

    // player
    float AnimSpeed_Run = 2.5f;
    float AnimSpeed_Jump = 2f;
    float AnimSpeed_Death = 2.5f;

    void Awake()
    {
        owner_Player = GetComponentInParent<BasePawnController>();

        InitAnims();

        audioSource = owner_Player.audioSource;
        audioSource.volume = owner_Player.volume;
    }

    private void Start()
    {
    }

    // and anim audios
    private void InitAnims()
    {
        _animator = GetComponent<Animator>();
        _animator.Enable();

        // Animations Override
        animOverrideCon = new AnimatorOverrideController
            (_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = animOverrideCon;


        SetAnimSpeed(nameof(AnimSpeed_Run), AnimSpeed_Run);
        SetAnimSpeed(nameof(AnimSpeed_Jump), AnimSpeed_Jump);
        SetAnimSpeed(nameof(AnimSpeed_Death), AnimSpeed_Death);
    }

    #endregion

    private void Update()
    {
        _animator.SetFloat("Speed", _speed);
        _animator.SetFloat("XInput", _hzInput);
        _animator.SetFloat("YInput", _vInput);

        _animator.SetBool("Grounded", _grounded);
        _animator.SetBool("Falling", !_grounded);
    }

    #region Audio Clip Events
    void Die()
    {
        if (death != null)
        {
            audioSource.clip = death;
            audioSource.Play();
        }
    }

    #endregion

    // todo delete after change character
    #region legacy events
    public void ExecuteEvent() { } // "OnAnimatorJump"
    #region Hank CF, not in use
    public void ret() { }
    public void sethp() { } // 100
    public void ychy() { }// on run forward
    public void ofs() { }
    public void over() { }
    public void xshy() { }
    public void fire() { }
    public void ofs3() { }


    #endregion
    #endregion



    public void TriggerJump()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Jump");
        }
    }

    /// <summary>
    /// call in draw
    /// </summary>
    /// <param name="weaponData"></param>
    public void ChangeAnim3P(WeaponData weaponData)
    {
        // override by weapon
        var overrideCon = animOverrideCons.FirstOrDefault
            (x => x.name.EndsWith(weaponData.WeaponAnimType.GetCode()));
        if (overrideCon == null)
        {
            Debug.LogError("Anim override controller null");
        }
        _animator.runtimeAnimatorController
            = overrideCon;

        // set speed by weapon
        ChangeAnimSpeed3P(weaponData);
    }

    private void ChangeAnimSpeed3P(WeaponData weaponData)
    {
        // upper body anims
        var animsNeedSet = new List<string>()
        {
            AnimNames.Fire,
            AnimNames.Heavy,
            AnimNames.Reload,
            AnimNames.Combo1,
            AnimNames.Combo2,
            AnimNames.Combo3,
        };

        foreach (var animClip1P in weaponData.Anim1PDtos)
        {
            var nameAffix = animClip1P.GetNameAffix();
            if (animsNeedSet.Contains(nameAffix))
            {
                var anim3Ps = _animator.runtimeAnimatorController
                    .animationClips;
                var animClip3P = anim3Ps
                    .FirstOrDefault(it => it.name
                .EndsWith(nameAffix));

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

    internal void TriggerFire()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.Fire),
            weaponLayer);
    }

    internal void TriggerPreFire()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.PreFire),
            weaponLayer);
    }


    internal void TriggerReload()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.Reload),
            weaponLayer);
    }

    internal void TriggerHeavy()
    {
        _animator.Play(Join(AnimNames.Dummy, AnimNames.Heavy),
            weaponLayer);
    }

    #region Helper function

    public void PlayAnimState(string animStateName)
    {
        _animator.Play(animStateName, weaponLayer);
    }

    protected void PlayAnimClip(AnimationClip animationClip)
    {
        var dummyState = Join(AnimNames.Dummy, animationClip.GetNameAffix());
        this.PlayAnimState(dummyState);
    }

    public void SetAnimSpeed(string animSpeedName, float speed)
    {
        _animator.SetFloat(animSpeedName, speed);
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

    #endregion
}