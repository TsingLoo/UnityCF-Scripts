using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [HideInInspector]
    public WeaponAnimationController _animController;

    [Header("Type")]
    public string WeaponName;
    public string WeaponAssetName;

    public EWeaponType weaponType = EWeaponType.Rifle;
    public WeaponBagPosition weaponBagPos = WeaponBagPosition.Primary;
    public EWeaponFireMode weaponFireMode = EWeaponFireMode.Auto;
    public EWeaponAnimType weaponAnimType;

    [System.Serializable]
    public class WeaponRecoil
    {
        public float spreadAngle = 0.0f;
        public float screenShakeMultiplier = 1.0f;
    }

    public LayerMask _shootLayer { get; set; }

    #region Animations and Animation Audios

    [Header("Weapon Data")]
    MeleeBoxCollider meleeBoxCollider;

    #endregion
    [Header("Bullet")]

    public int projectilePerShot = 1;
    public EProjectileType bulletType = EProjectileType.Raycast;

    public Projectile projectilePrefab;
    public float projectileForce = 200.0f;

    [Header("Aiming")]
    public bool _canAim;
    public float aimingFOV;
    public float aimSenseFactor;
    public GameObject aimScope;
    public bool stopAimWhenFire;

    [Header("Range")]
    public float _range; // grenade, luncher
    public const float _raycastRange = 1000;
    public float _meleeRangeFire = 1.4f;
    public float _meleeRangeHeavy = 1.6f;
    public bool _hasHeavy;

    #region Weapon Ammo
    [AmmoType]
    //0: first in db, not in use, see player ammo in play controller
    [HideInInspector] public int ammoType = 0;//-1

    [Header("Ammo")]
    public int _clipSize = 30;
    public int defaultClips = 3;
    public int maxClips = 6;
    public bool disableOnEmpty;

    public int _ammoContent;
    public int _ammoCarry;
    #endregion
    /// <summary>
    /// Fire time between shots
    /// 0: control by animation
    /// </summary>
    [Header("Fire")]
    public float fireGap = 0f;

    // todo set in projectile
    public int damage = 35;
    public int damageHeavy = 60;

    public WeaponRecoil weaponRecoil;

    [Header("Bullet Visual Settings")]
    public LineRenderer bulletTrail;
    /// <summary>
    /// ammo display on weapon
    /// </summary>
    public AmmoDisplay _ammoDisplay;

    public bool TriggerHolding;
    public bool TriggerDown
    {
        get { return _triggerDown; }
        set
        {
            _triggerDown = value;
            if (!_triggerDown)
            {
                _clickDone = false;
            }
        }
    }

    [HideInInspector]
    public EWeaponState CurrentState => _currentState;
    public int GetAmmoRemain => _ammoContent;
    public BasePawnController OwnerPawn => _ownerPlayer;
    bool isAiming = false;


    BasePawnController _ownerPlayer;

    public EWeaponState _currentState;
    bool _clickDone;// for semi, shot clicked
    bool _triggerDown;
    // fire gap over, can shot next (animation may still playing)
    float _shotTimer = -1.0f;

    [HideInInspector]
    public AudioSource audioSource;

    Vector3 m_ConvertedMuzzlePos;
    static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];
    
    // used to shot projectile like grenade
    public Transform _muzzleShotPosition;
    // real muzzle position
    public Transform _muzzlePosition;
    public GameObject _muzzleFlashPrefab;
    public ParticleSystem _muzzleFlash;//use pool

    class ActiveTrail
    {
        public LineRenderer renderer;
        public Vector3 direction;
        public float remainingTime;
    }

    List<ActiveTrail> _activeTrails = new List<ActiveTrail>();

    Queue<Projectile> m_ProjectilePool = new Queue<Projectile>();


    #region Start

    void Awake()
    {
        WeaponName = WeaponName.Replace("_", "-");
        if (WeaponAssetName==null || WeaponAssetName == "")
        {
            WeaponAssetName = WeaponName.Replace("-", "_");
        }

        //InitMuzzle(); // set in editor

        _animController = GetComponent<WeaponAnimationController>();

        meleeBoxCollider = GetComponentInChildren<MeleeBoxCollider>();

        InitAmmo();

        #region FX
        if (bulletTrail != null)
        {
            const int trailPoolSize = 16;
            PoolSystem.Instance.InitPool(bulletTrail, trailPoolSize);
        }

        if (projectilePrefab != null)
        {
            //a minimum of 100 is useful for weapon that have a clip size
            //of 1 and you can throw more before the previous one
            //was recycled/exploded.
            int size = Mathf.Max(100, _clipSize) * projectilePerShot;
            for (int i = 0; i < size; ++i)
            {
                Projectile projectile = Instantiate(projectilePrefab);
                projectile.gameObject.SetActive(false);
                m_ProjectilePool.Enqueue(projectile);
            }
        }
        #endregion


    }

    //private void InitMuzzle()
    //{
    //    var rootTransform = this.transform;
    //    var weaponRoot = rootTransform.GetChild(0);

    //    // todo only get 1
    //    Muzzle = weaponRoot.Find(nameof(Muzzle));
    //    if (Muzzle == null)
    //    {
    //        Debug.LogError("muzzle no found");
    //    }
    //}

    private void InitAmmo()
    {
        _ammoContent = _clipSize;
        _ammoCarry = _clipSize * defaultClips;
    }

    private void Start()
    {
        SetArms();

        //if (_muzzleFlash)
        //{
        //    PoolSystem.Instance.InitPool
        //        (_muzzleFlash, PoolSystem.poolSize);
        //}
    }

    /// <summary>
    /// arms and hands / InitHands / InitArms
    /// Set after player pickup
    /// </summary>
    private void SetArms()
    {
        var rootTransform = this.transform;
        var weaponRoot = rootTransform.GetChild(0);
        Transform armTransform;

        if(OwnerPawn != null)
        {
            var playerType = OwnerPawn._playerType;
            var team = OwnerPawn._playerTeam;
            // HAND_BL
            var handName = playerType.GetDescription() + "_" + team.GetDescription();
            armTransform = weaponRoot.Find(handName);
            if (armTransform == null)
            {
                // Swat_T
                handName = playerType.GetCode() + "_" + team.GetCode();
                armTransform = weaponRoot.Find(handName);
            }

            if (armTransform)
            {
                armTransform.gameObject.SetActive(true);
            }
        }
    }
    #endregion


    public void InitPlayer(BasePawnController pawn)
    {
        _ownerPlayer = pawn;
    }

    /// <summary>
    /// UnDraw
    /// </summary>
    public void PutAway()
    {
        StopAiming();
        _animController.TriggerPutAway();
        this.Hide();

        // trails
        for (int i = 0; i < _activeTrails.Count; ++i)
        {
            var activeTrail = _activeTrails[i];
            _activeTrails[i].renderer.gameObject.SetActive(false);
        }
        _activeTrails.Clear();
    }

    public void WeaponDraw()
    {
        this.Show();

        _animController.TriggerDraw();

        _currentState = EWeaponState.Drawing;

        TriggerDown = false;
        _clickDone = false;

        var ammoRemaining = GetAmmo(ammoType);
        if (disableOnEmpty)
        {
            gameObject.SetActive
                (_ammoContent != 0 || ammoRemaining != 0);
        }

        WeaponUI.Instance.UpdateWeaponName(this);
        WeaponUI.Instance.UpdateAmmoRemain(this);
        WeaponUI.Instance.UpdateAmmoAmount(GetAmmo(ammoType));

        if (_ammoDisplay)
            _ammoDisplay.UpdateAmount(_ammoContent, _clipSize);

        if (_ammoContent == 0 && ammoRemaining != 0)
        {
            int chargeInClip = Mathf.Min(ammoRemaining, _clipSize);
            _ammoContent += chargeInClip;

            if (_ammoDisplay)
                _ammoDisplay.UpdateAmount(_ammoContent, _clipSize);

            ChangeAmmo(-chargeInClip);

            if(OwnerPawn is PlayerController)
            {
                WeaponUI.Instance.UpdateAmmoRemain(this);
            }
        }
    }

    private void ChangeAmmo(int amount, int ammoType = 0)
    {
        // weapon carry ammo
        _ammoCarry += amount;

        if (OwnerPawn is PlayerController)
        {
            WeaponUI.Instance.UpdateAmmoAmount(GetAmmo());
        }

        // user carry ammo 
        //_ownerPlayer.ChangeAmmo(amount, ammoType);
    }

    private int GetAmmo(int ammoType = 0)
    {
        return _ammoCarry;

        // return  _ownerPlayer.GetAmmo(ammoType);
    }

    public void WeaponFire()
    {
        //_currentState == EWeaponState.Idle&&
        if (ReadyToFire()
            && HasAmmo())
        {
            //the state will only change next frame
            _currentState = EWeaponState.Firing;
            if (stopAimWhenFire)
            {
                StopAiming();
            }

            //CameraShaker.Instance.Shake(0.2f, 0.05f * weaponRecoil.screenShakeMultiplier);

            // timer
            _shotTimer = fireGap;
            // anim
            _animController.TriggerFire();
            // scope recoil
            if (isAiming)
            {
                WeaponScope.Instance.PlayRecoil();
            }
            // attack
            if (weaponType == EWeaponType.Melee)
            {
                // called in anim events
                //MeleeWeaponAttack();
            }
            else
            {
                WeaponAttack();

                ShowMuzzleFX();

                _animController.PlaySoundClip(AnimNames.Fire,
                    _animController._volume * 0.6f);
            }

        }
    }

    // draw exit time 0.75
    private bool ReadyToFire()
    {
        return (_currentState == EWeaponState.Firing && ShotTimerOut()
            || _currentState == EWeaponState.Idle);

        //&& !_animController.AnimPlaying(AnimNames.Combo3)
    }

    private bool HasAmmo()
    {
        return weaponType == EWeaponType.Melee
            || _ammoContent > 0;
    }

    public bool ShotTimerOut()
    {
        return _shotTimer <= 0;
    }

    //public void GrenadeThrow()
    //{
    //    WeaponAttack();
    //}

    public void WeaponAttack()
    {
        if (bulletType == EProjectileType.Raycast)
        {
            for (int i = 0; i < projectilePerShot; ++i)
            {
                ShotRaycast(damage);
            }
        }
        else
        {
            ShotProjectile();
        }

        // ammo ui
        _ammoContent -= 1;
        if (_ammoDisplay)
        {
            _ammoDisplay.UpdateAmount(_ammoContent, _clipSize);
        }

        if (OwnerPawn is PlayerController)
        {
            WeaponUI.Instance.UpdateAmmoRemain(this);
        }
    }

    internal void WeaponSpecial()
    {
        if (_canAim)//weaponType == EWeaponType.Sniper
        {
            ToggleAiming();
        }
        else
        {
            HeavyAttack();
        }
    }

    internal void ToggleAiming()
    {
        if (CurrentState == EWeaponState.Idle)
        {
            if (!isAiming)
            {
                StartAiming();
            }
            else
            {
                StopAiming();
            }
        }

        // todo use state, aiming state in update
        //if(CurrentState == EWeaponState.Idle)
        //{
        //    WeaponScope.Instance.ShowScope();
        //    _currentState = EWeaponState.Aiming;
        //}
        //else if(CurrentState == EWeaponState.Aiming) 
        //{
        //    WeaponScope.Instance.HideScope();
        //    _currentState = EWeaponState.Idle;
        //}
    }

    private void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;

            WeaponScope.Instance.HideScope();

        }

    }

    private void StartAiming()
    {
        if (!isAiming)
        {
            isAiming = true;

            WeaponScope.Instance
                .ShowScope(aimScope, aimingFOV, aimSenseFactor);
        }

    }

    #region Melee

    internal void HeavyAttack()
    {
        // attack call in MeleeWeaponHeavy

        // anim
        if (weaponType == EWeaponType.Melee
            || _hasHeavy)
        {
            _animController.TriggerHeavy();
        }
    }


    // meleeFire
    public void MeleeWeaponAttack()
    {
        ShotRaycast(damage, _meleeRangeFire);
    }

    /// <summary>
    /// called in anim event
    /// </summary>
    public void MeleeWeaponAttackHeavy()
    {
        // todo weapon1p not detect enemy, see meleeBoxCollider
        ShotRaycast(damageHeavy, _meleeRangeHeavy);
        return;


        if (meleeBoxCollider != null)
        {
            meleeBoxCollider.BeginCheck();
        }
        else
        {
            ShotRaycast(damageHeavy, _meleeRangeHeavy);
        }

        //var weapon3P = _ownerPlayer.GetCurrentWeapon3P;

        //// use collider for future TPP only feature
        //if(weapon3P.hitCollider != null)
        //{
        //    weapon3P.hitCollider.enabled = true;
        //}

    }

    internal void FinishHeavy()
    {
        if (meleeBoxCollider != null)
        {
            meleeBoxCollider.EndCheck();
        }
    }

    #endregion


    void ShotRaycast(int damage,
        float raycastDistance = _raycastRange)
    {
        if (raycastDistance == 0)
        {
            Debug.LogError("raycastDistance is 0");
        }

        Ray ray = OwnerPawn.GetShotRay();
        // ray from camera1P
        //Ray ray = PlayerController.Instance.Camera1P_Main
        //    .ViewportPointToRay(Vector3.one * 0.5f
        //    + (Vector3)spread);

        // cast ray
        // LayerMask shootLayer = LayerHelper.GetAllLayer();
        _shootLayer = GameSystem.Instance._shootLayer;
        if (OwnerPawn is BotController)
        {
            _shootLayer = GameSystem.Instance._enemyShootLayer;
        }

        Debug.DrawLine(ray.origin, ray.direction, 
            Color.yellow, 5f);
        //Debug.DrawLine(ray.origin, hitPosition, 
        //    Color.red, 5f);

        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction,
            raycastDistance,
            _shootLayer,
            QueryTriggerInteraction.Collide);
        if (hits.HasValue())// enemy use trigger hit box
        {
            var hit = hits[0];
            //foreach (var hit in hits)
            {
                Debug.DrawLine(ray.origin, hit.transform.position,
                Color.red, 5f);

                if (IsEnemy(hit))
                {
                    // impack FX
                    Renderer hitRenderer = hit.collider
                        .GetComponentInChildren<Renderer>();

                    ImpactManager.Instance
                        .PlayImpact(hit.point,
                        hit.normal,
                        hitRenderer == null ?
                        null
                        : hitRenderer.sharedMaterial);

                    // health
                    var hitPart = hit.transform.GetComponent<IDamageable>();
                    if (hitPart != null)
                    {
                        var damageType = EDamageType.Rifle;
                        if (weaponType == EWeaponType.Melee)
                        {
                            damageType = EDamageType.Knife;
                        }

                        hitPart.TakeDamage(damage, damageType);
                    }
                }
            }

            // todo check
            ////if too close, the trail effect would look weird if it arced to hit the wall, so only correct it if far
            //if (hit.distance > 5.0f)
            //    hitPosition = hit.point;

            //hitPosition = hit.point;
        }


        //if (bulletTrail != null)
        //{
        //    var trailPos = new Vector3[] { GetCorrectedMuzzlePlace(),
        //        hitPosition };

        //    var trail = PoolSystem.Instance
        //        .GetInstance(bulletTrail);
        //    trail.gameObject.SetActive(true);
        //    trail.SetPositions(trailPos);

        //    _activeTrails.Add(new ActiveTrail()
        //    {
        //        remainingTime = 0.3f,
        //        direction = (trailPos[1] - trailPos[0]).normalized,
        //        renderer = trail
        //    });
        //}
    }

    private bool IsEnemy(RaycastHit hit)
    {
        return OwnerPawn is PlayerController 
            && hit.collider.gameObject.layer 
                == EditorLayer.Enemy.GetValue()
        || OwnerPawn is BotController
            && hit.collider.gameObject.layer
                    == EditorLayer.Player3P.GetValue();
    }

    void ShotProjectile()
    {
        for (int i = 0; i < projectilePerShot; ++i)
        {
            // todo: shoot to point is looking position
            // https://www.youtube.com/watch?v=F20Sr5FlUlE&t=45s
            float angle = Random.Range(0.0f, weaponRecoil.spreadAngle * 0.5f);
            Vector2 angleDir = Random.insideUnitCircle * Mathf.Tan(angle * Mathf.Deg2Rad);

            var baseDirection = _muzzleShotPosition.transform.forward;
            // todo aim to screen center,
            // aim up a little bit or use fps micro game instead
            //var baseDirection = _ownerPlayer.aimPos.position - Muzzle.transform.position;
            Vector3 direction = baseDirection + (Vector3)angleDir;
            direction.Normalize();

            var projectile = m_ProjectilePool.Dequeue();

            projectile.gameObject.SetActive(true);
            projectile.Launch(this, direction, projectileForce);
        }
    }

    //For optimization, when a projectile is "destroyed"
    //it is instead disabled and return to the weapon for reuse.
    public void ReturnProjecticle(Projectile p)
    {
        m_ProjectilePool.Enqueue(p);
    }

    public void WeaponReload()
    {
        //if (_currentState != EWeaponState.Idle
        //    || _ammoContent == _clipSize)
        //    return;

        int remainingBullet = GetAmmo(ammoType);
        if(_currentState == EWeaponState.Idle
            && _ammoContent < _clipSize
            && remainingBullet > 0)
        {
            //the state will only change next frame
            _currentState = EWeaponState.Reloading;
            StopAiming();

            _animController.TriggerReload();
        }

        // grenade: disable when empty
        if (remainingBullet == 0)
        {
            if (disableOnEmpty)
            {
                OwnerPawn.UnEquipCurrentWeapon();

                //OwnerPawn._lastBagPos is reset to -1 after unequip
                OwnerPawn.ChangeWeapon();
            }
        }
    }

    protected void Update()
    {
        UpdateWeaponState();

        // UpdateTrail
        Vector3[] pos = new Vector3[2];
        for (int i = 0; i < _activeTrails.Count; ++i)
        {
            var activeTrail = _activeTrails[i];

            activeTrail.renderer.GetPositions(pos);
            activeTrail.remainingTime -= Time.deltaTime;

            pos[0] += activeTrail.direction * 50.0f * Time.deltaTime;
            pos[1] += activeTrail.direction * 50.0f * Time.deltaTime;

            _activeTrails[i].renderer.SetPositions(pos);

            if (_activeTrails[i].remainingTime <= 0.0f)
            {
                _activeTrails[i].renderer.gameObject.SetActive(false);
                _activeTrails.RemoveAt(i);
                i--;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_shotTimer > 0)
            _shotTimer -= Time.deltaTime;



    }

    void UpdateWeaponState()
    {
        UpdateAnimator();

        EWeaponState newState;
        newState = _animController.GetWeaponState();

        if (newState != _currentState)
        {
            var oldState = _currentState;
            _currentState = newState;

            if (oldState == EWeaponState.Firing)
            {//we just finished firing, so check if we need to auto reload
                if (_ammoContent == 0)
                    WeaponReload();
            }
        }

        // called in base animator
        // reload finish
        //if (animController.ReloadFinished())
        //{
        //    FinishReload();
        //}


        if (TriggerDown)
        {
            if (weaponFireMode == EWeaponFireMode.Semi)
            {
                if (!_clickDone)
                {
                    _clickDone = true;
                    WeaponFire();
                }
            }
            else if (weaponType == EWeaponType.Grenade)
            {
                GrenadeFire();
            }
            else
            {
                WeaponFire();
            }
        }

    }

    public void GrenadeFire()
    {
        if (_currentState == EWeaponState.Idle
            && HasAmmo())
        {
            // anim
            _animController.TriggerPreFire();
            // change to prefire when prefire end
            _currentState = EWeaponState.Firing;

            //CameraShaker.Instance.Shake(0.2f, 0.05f * weaponRecoil.screenShakeMultiplier);

        }
        
    }

    public void FinishReload()
    {
        UpdateAmmo();

        // ui on weapon
        if (_ammoDisplay)
        {
            _ammoDisplay.UpdateAmount(_ammoContent, _clipSize);
        }
        // ui main
        WeaponUI.Instance.UpdateAmmoRemain(this);
    }



    private void UpdateAmmo()
    {
        var remainingBullet = GetAmmo(ammoType);

        int chargeInClip = Mathf.Min(remainingBullet,
            _clipSize - _ammoContent);
        _ammoContent += chargeInClip;

        ChangeAmmo(-chargeInClip);
    }

    private void UpdateAnimator()
    {
        _animController._speed = _ownerPlayer._speed;
        _animController._grounded = _ownerPlayer.GetGrounded;
        _animController._falling = !_ownerPlayer.GetGrounded;

        _ownerPlayer.UpdateAnimator();
    }

    // todo 
    /// <summary>
    /// This will compute the corrected position of the muzzle flash in world space. Since the weapon camera use a
    /// different FOV than the main camera, using the muzzle spot to spawn thing rendered by the main camera will appear
    /// disconnected from the muzzle flash. So this convert the muzzle post from
    /// world -> view weapon -> clip weapon -> inverse clip main cam -> inverse view cam -> corrected world pos
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCorrectedMuzzlePlace()
    {
        Vector3 position = _muzzleShotPosition.position;

        // if use 2 cameras, use this line
        //position = PlayerController.Instance.Camera1P_Weapon.WorldToScreenPoint(position);
        position = PlayerController.Instance.Camera1P_Main.ScreenToWorldPoint(position);

        return position;
    }

    public AnimationClip GetAnim(string animNameAffix)
    {
        return _animController.GetAnim(animNameAffix);
    }

    internal void ShowMuzzleFX()
    {
        if (_muzzleFlashPrefab != null)
        {
            if (_muzzlePosition != null) // not use pool
            {
                var WeaponMuzzle = _muzzlePosition;
                GameObject muzzleFlashInstance =
                    Instantiate(_muzzleFlashPrefab, WeaponMuzzle.position,
                WeaponMuzzle.rotation, WeaponMuzzle.transform);
                // Unparent the muzzleFlashInstance
                if (false)
                {
                    muzzleFlashInstance.transform.SetParent(null);
                }

                Destroy(muzzleFlashInstance, 0.2f);
            }
            else // use pool
            {
                //var muzzle = muzzlePosition;

                //var muzzleFlash = PoolSystem.Instance
                //    .GetInstance(_weapon.muzzleFlash);
                //muzzleFlash.gameObject.transform.position =
                //    muzzle.transform.position;
                //muzzleFlash.gameObject.transform.rotation
                //    = muzzle.transform.rotation;
                //muzzleFlash.gameObject.transform.forward =
                //    muzzle.transform.forward;
                //// move with weapon
                //muzzleFlash.transform.SetParent(muzzle.transform,
                //    worldPositionStays: true);

                //muzzleFlash.gameObject.SetActive(true);
                //muzzleFlash.Play();
            }

        }

    }

    internal bool IsLastFire()
    {
        return _ammoContent == 1 && _ammoCarry == 0;
    }

    public float GetWeaponRange()
    {
        var range = 1f;

        if(weaponType == EWeaponType.Melee)
        {
            range = _meleeRangeHeavy;
        }
        else
        {
            if(_range > 0f)
            {
                range = _range;
            }
            else
            {
                range = _raycastRange;
            }
        }

        return range;
    }
    // End
}

// todo move outside
public abstract class AmmoDisplay : MonoBehaviour
{
    public abstract void UpdateAmount(int current, int max);
}

#region custom ammo display in editor
public class AmmoTypeAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR

// todo db model / AmmoType Enum in code
//[CustomPropertyDrawer(typeof(AmmoTypeAttribute))]
//public class AmmoTypeDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        AmmoDatabase ammoDB = GameDatabase.Instance.ammoDatabase;

//        if (ammoDB.entries == null || ammoDB.entries.Length == 0)
//        {
//            EditorGUI.HelpBox(position, "Please define at least 1 ammo type in the Game Database", MessageType.Error);
//        }
//        else
//        {
//            int currentID = property.intValue;
//            int currentIdx = -1;

//            //this is pretty ineffective, maybe find a way to cache that if prove to take too much time
//            string[] names = new string[ammoDB.entries.Length];
//            for (int i = 0; i < ammoDB.entries.Length; ++i)
//            {
//                names[i] = ammoDB.entries[i].name;
//                if (ammoDB.entries[i].id == currentID)
//                    currentIdx = i;
//            }

//            EditorGUI.BeginChangeCheck();
//            int idx = EditorGUI.Popup(position, "Ammo Type", currentIdx, names);
//            if (EditorGUI.EndChangeCheck())
//            {
//                property.intValue = ammoDB.entries[idx].id;
//            }
//        }
//    }
//}


#endif
#endregion
