using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BasePawnController: MonoBehaviour
{
    public EPlayerType _playerType = EPlayerType.Fox;
    public ETeam _playerTeam = ETeam.T;

    // todo move to player con
    protected bool hasControl = false;
    public float MouseSensitivity = 1.5f;
    [HideInInspector] public float _mouseSensitivityUse;

    private const float _threshold = 0.01f;
    [HideInInspector]
    public int _lastBagPos = (int)WeaponBagPosition.None;

    public AudioSource audioSource;

    [Header("Audio")]
    public RandomPlayer FootstepPlayer;
    public float Gravity = 15.0f;
    public float JumpHeight = 1.2f;
    public AudioClip JumpingAudioCLip;
    public AudioClip LandingAudioClip;

    [Tooltip("How far in degrees can you move the camera up")]
    public float MaxCameraAngle = 89.0f;
    public float MaxFallSpeed = 15.0f;

    [Tooltip("How far in degrees can you move the camera down")]

    public float MinCameraAngle = -89.0f;
    public float RunSpeed = 8.0f;

    // player ammo
    [HideInInspector] public List<AmmoInventoryEntry> startingAmmo;
    public float volume;
    public float WalkSpeed = 4.0f;

    [HideInInspector]
    public WeaponBag weaponBag;

    [Header("Weapon sockets")]
    public Transform WeaponPosition1P;
    public Transform WeaponPosition3P;

    [HideInInspector] public EditorLayer weapon1PLayer { get; set; }
    [HideInInspector] public EditorLayer weapon3PLayer { get; set; }

    // todo use weaponBag instead
    protected int _currentBagPos = (int)WeaponBagPosition.None;

    protected Dictionary<int, WeaponController> _weapon1Ps = new Dictionary<int, WeaponController>();
    protected Dictionary<int, WeaponItem> _weaponItems = new Dictionary<int, WeaponItem>();
    [Header("Animations")]

    float baseAnimSpeed = 1;
    protected bool isFirstPerson = true;
    float JumpSpeed = 5.0f;
    bool loosedGrounding;

    Dictionary<int, int> m_AmmoInventory = new Dictionary<int, int>();

    protected bool m_Grounded;
    protected float m_GroundedTimer;

    protected float m_SpeedAtJump = 0.0f;

    protected float m_VerticalSpeed = 0.0f;
    public float _hzInput { get; set; }
    public float _speed { get; set; } = 0.0f;
    public float _vInput { get; set; }

    public PawnAnimationController animController { get; private set; }

    public bool GetGrounded => m_Grounded;

    public WeaponController GetCurrentWeapon 
        => GetWeapon1P(_currentBagPos);
    public WeaponItem GetCurrentWeapon3P
        => GetWeaponItem(_currentBagPos);

    public void ChangeAmmo(int ammoType, int amount)
    {
        if (!m_AmmoInventory.ContainsKey(ammoType))
            m_AmmoInventory[ammoType] = 0;

        var previous = m_AmmoInventory[ammoType];
        m_AmmoInventory[ammoType] = Mathf.Clamp(m_AmmoInventory[ammoType] + amount, 0, 999);

        if (HasWeapon()
            && _currentBagPos != (int)WeaponBagPosition.None
            && GetCurrentWeapon.ammoType == ammoType)
        {
            //we just grabbed ammo for a weapon that
            //add non left, so it's disabled right now.
            //Reselect it.
            //todo check
            //if (previous == 0 && amount > 0)
            //{
            //    CurrentWeapon.WeaponDraw();
            //}

            WeaponUI.Instance.UpdateAmmoAmount
                (GetAmmo(ammoType));
        }
    }

    private bool HasWeapon()
    {
        return _weapon1Ps.Count > 0;
    }

    public int GetAmmo(int ammoType)
    {
        int value = 0;
        m_AmmoInventory.TryGetValue(ammoType, out value);

        return value;
    }

    // to inventory
    public virtual void InitWeapon(WeaponItem weaponItem)
    {
        var weaponController = weaponItem.Weapon1P;

        EquipWeapon1P(weaponController);

        // 3p
        // need to copy a new one, transform.setParent not allowed for assets
        var weapon3PNew = Instantiate(weaponItem, WeaponPosition3P, false);
        weapon3PNew.OnPickUp(this);
        weapon3PNew.ResetTransform();
        _weaponItems.Add(weaponController.weaponBagPos.GetValue(), weapon3PNew);
        weapon3PNew.Hide();
    }

    public bool PickupWeapon(WeaponItem newItem)
    {
        var pickedUp = false;
        if (CanPickupWeapon()
            && weaponBag.PickupWeapon(newItem))
        {
            pickedUp = true;
            InitWeapon(newItem);

            // todo secondary
            if (newItem.Weapon1P.weaponBagPos != WeaponBagPosition.Melee)
            {
                ChangeWeapon(newItem.Weapon1P.weaponBagPos.GetValue());
            }
        }
        return pickedUp;
    }

    private bool CanPickupWeapon()
    {
        return GetCurrentWeapon == null
            || GetCurrentWeapon.CurrentState == EWeaponState.Idle
            || GetCurrentWeapon.CurrentState == EWeaponState.Drawing;
    }

    public void PlayFootstep()
    {
        FootstepPlayer.PlayRandom();
    }

    internal void PlayDrawAnimation()
    {

    }

    internal void UpdateAnimator()
    {
        animController._speed = _speed;
        animController._hzInput = _hzInput;
        animController._vInput = _vInput;
        animController._grounded = GetGrounded;
        animController._falling = !GetGrounded;
    }

    protected virtual void Awake()
    {
        animController = GetComponent<PawnAnimationController>();
        weaponBag = GetComponent<WeaponBag>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning(this.name + ": BP未添加" +
                nameof(AudioSource));
        }

        if (startingAmmo.Count() == 0)
        {
            startingAmmo
                .Add(new AmmoInventoryEntry()
                { ammoType = 0, amount = 300 });

            startingAmmo
                .Add(new AmmoInventoryEntry()
                { ammoType = 1, amount = 300 });

            startingAmmo
                .Add(new AmmoInventoryEntry()
                { ammoType = 2, amount = 300 });

        }
    }

    public void ChangeWeapon()
    {
        var lastWeapon = GetWeapon1P(_lastBagPos);
        if( lastWeapon!= null)
        {
            ChangeWeapon(_lastBagPos);
        }
        else
        {
            ChangeWeapon(_weapon1Ps.FirstOrDefault().Key);
        }
    }

    public void ChangeWeapon(int newBagPos)
    {
        var nextWeapon = GetWeapon1P(newBagPos);
        if (nextWeapon != null)
        {
            if (GetCurrentWeapon != null)
            {
                GetCurrentWeapon.PutAway();
            }

            _lastBagPos = _currentBagPos;
            _currentBagPos = newBagPos;

            this.WeaponDraw(nextWeapon);
        }

    }

    private WeaponController GetWeapon1P(int weaponPos)
    {
        WeaponController wp = null;
        _weapon1Ps.TryGetValue((int)weaponPos, out wp);
        return wp;
    }
    private WeaponController GetWeapon1P(WeaponBagPosition weaponPos)
    {
        return GetWeapon1P(weaponPos.GetValue());
    }

    public WeaponItem GetWeaponItem(int weaponPos)
    {
        WeaponItem wp = null;
        _weaponItems.TryGetValue((int)weaponPos, out wp);
        return wp;
    }

    private void EquipBag()
    {
        if (weaponBag.startWeapons != null)
        {
            foreach (var item in weaponBag.startWeapons)
            {
                PickupWeapon(item);
            }
        }
    }


    private void RemoveWeapon1P()
    {
        // fps
        var currentWeapon = GetWeapon1P(_currentBagPos);
        _weapon1Ps.Remove(_currentBagPos);

        currentWeapon.PutAway();
        currentWeapon.SelfDestroy();
    }

    /// <summary>
    /// and throw weapon
    /// </summary>
    private void RemoveWeapon3P()
    {
        _weaponItems.Remove(_currentBagPos);
    }

    private IEnumerator ShowFireArm()
    {
        yield return new WaitForSeconds(0.5f);

        // show weapon
        var primary = GetWeapon1P(WeaponBagPosition.Primary.GetValue());
        var secondary = GetWeapon1P(WeaponBagPosition.Secondary.GetValue());
        if (primary != null || secondary != null)
        {
            var weaponIdToShow = WeaponBagPosition.Primary.GetValue();
            if (primary == null)
            {
                weaponIdToShow = WeaponBagPosition.Secondary.GetValue();
            }
            ChangeWeapon(weaponIdToShow);
        }
    }

    private void ShowWeapons()
    {
        // show melee
        ChangeWeapon(WeaponBagPosition.Melee.GetValue());

        //StartCoroutine(ShowFireArm());
    }



    protected virtual void Start()
    {
        m_Grounded = true;

        EquipBag();

        ShowWeapons();

        // player ammo
        //for (int i = 0; i < startingAmmo.Count(); ++i)
        //{
        //    ChangeAmmo(startingAmmo[i].ammoType, 
        //        startingAmmo[i].amount);
        //}


        //for (int i = 0; i < startingAmmo.Count(); ++i)
        //{
        //    m_AmmoInventory[startingAmmo[i].ammoType] 
        //        = startingAmmo[i].amount;
        //}

    }


    private void SwitchWeapon()
    {
        if (_lastBagPos != -1 // has last 
            && _lastBagPos != _currentBagPos
            && _weapon1Ps.Count > 1)
        {
            ChangeWeapon(_lastBagPos);

            //Debug.LogWarning("From: " +
            //    m_Weapons[m_CurrentWeapon].name + ", " +
            //    "To: " + m_Weapons[m_LastWeapon].name);
        }
    }

    // throw weapon
    private void WeaponThrow()
    {
        if (GetCurrentWeapon.weaponType != EWeaponType.Melee
            && GetCurrentWeapon.weaponType != EWeaponType.Grenade)
        {
            // throw
            var weaponItem = GetWeaponItem(_currentBagPos);
            weaponItem.Throw(WeaponPosition3P);//Camera1P_Main.transform//_characterController.velocity, 

            UnEquipCurrentWeapon();

            // change to next weapon
            ChangeWeapon();
        }

    }

    // removeCurrentWeapon
    public void UnEquipCurrentWeapon()
    {
        weaponBag.RemoveWeapon(_currentBagPos);

        RemoveWeapon1P();
        RemoveWeapon3P();
    }

    protected virtual void Update()
    {
        UpdateMovement();
        UpdateWeaponControl();
    }

    /// <summary>
    /// Move
    /// Jump
    /// </summary>
    protected virtual void UpdateMovement()
    {
       
    }

    protected void UpdateWeaponControl()
    {
        if (hasControl
            && _weapon1Ps.Count > 0
            && GetWeapon1P(_currentBagPos) != null) // Update too rapid here, weapon throw ...
        {
            // fire
            GetCurrentWeapon.TriggerDown =
                Input.GetMouseButton(0);
            GetCurrentWeapon.TriggerHolding 
                = Input.GetMouseButton(0);

            // grenade
            // determined by animation
            //if (GetCurrentWeapon._currentState == EWeaponState.PreFire)
            //{
            //    if (!Input.GetMouseButton(0))
            //    {
            //        GetCurrentWeapon.GrenadeFire();
            //    }
            //}

            // special
            if (Input.GetMouseButtonDown(1))
            {
                GetCurrentWeapon.WeaponSpecial();
            }

            // reload
            if (Input.GetButtonDown("Reload"))
                GetCurrentWeapon.WeaponReload();

            #region Switch weapon
            if (Input.GetButtonDown("SwitchWeapon"))
            {
                SwitchWeapon();
            }

            // drop
            if (Input.GetKeyDown(KeyCode.G))//Input.GetButtonDown("ThrowWeapon")
            {
                WeaponThrow();
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                ChangeWeapon(_currentBagPos - 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                ChangeWeapon(_currentBagPos + 1);
            }
            #endregion
        }
    }

    private void WeaponDraw(WeaponController weapon)
    {
        // 1p
        weapon.WeaponDraw();

        // 3p
        if (WeaponPosition3P != null)
        {
            foreach (var item in _weaponItems)
            {
                item.Value.Hide();
            }

            var pvModel = GetWeaponItem(weapon.weaponBagPos.GetValue());
            if (pvModel != null)
            {
                pvModel.Show();
            }
        }
    }

    #region Weapons
    /// <summary>
    /// make a copy
    /// </summary>
    /// <param name="weaponController"></param>
    protected void EquipWeapon1P(WeaponController weaponController)
    {
        // 1p
        var weapon1PNew = Instantiate(weaponController, WeaponPosition1P, false);
        weapon1PNew.name = weaponController.name;

        weapon1PNew.Setlayer(weapon1PLayer);
        weapon1PNew.transform.localPosition = Vector3.zero;
        weapon1PNew.transform.localRotation = Quaternion.identity;

        weapon1PNew.InitPlayer(this);

        // qc model
        weapon1PNew.GetComponentInChildren<Camera>().Disable();
        weapon1PNew.GetComponentInChildren<Light>().Disable();

        weapon1PNew.gameObject.SetActive(false);
        _weapon1Ps.Add((int)weapon1PNew.weaponBagPos, weapon1PNew);
    }

    public virtual Ray GetShotRay()
    {
        Debug.LogError("function need implement");
        return new Ray();
    }
    #endregion

    // End
}