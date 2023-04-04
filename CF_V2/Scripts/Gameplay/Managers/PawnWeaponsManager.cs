using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class PawnWeaponsManager : BaseBehaviour
    {
        public List<Item> startWeapons;

        // todo ref
        public List<Item> startNanoWeapons;

        // currently not in use
        protected PawnController _pawnController;
        public Transform WeaponPosition1P;
        public Transform WeaponPosition3P; // hand r

        public UnityAction<WeaponController> OnAddedWeapon;
        public UnityAction<WeaponController> OnRemovedWeapon;

        public UnityAction<WeaponController> OnSwitchedToWeapon;

        /// <summary>
        /// have null slots !!!
        /// 0, 1~5
        /// </summary>
        protected WeaponController[] _weapon1Ps = new WeaponController[6];
        public int CurrentBagPos { get; private set; } = -1;

        protected int _lastBagPos { get; set; } = -1;

        public EditorLayer Weapon1PLayer { get; set; }
        public EditorLayer Weapon3PLayer { get; set; }
        public bool IsPlayer { get; set; }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            _pawnController = GetComponentInChildren<PawnController>();
            Debug.Assert(_pawnController != null);
        }

        protected virtual void Update()
        {
            UpdateWeaponAutoReload();
        }

        private void UpdateWeaponAutoReload()
        {
            WeaponController currentWeapon = GetCurrentWeapon();

            if (currentWeapon != null)
            {
                // auto reload
                if (currentWeapon.NeedAutoReload())
                {
                    Invoke(nameof(HandleWeaponReload), 0.2f);
                    //StartCoroutine(HandleAutoReload());
                }
            }
        }

        IEnumerator HandleAutoReload()
        {
            yield return new WaitForSeconds(0.2f);

            HandleWeaponReload();
        }

        protected void HandleWeaponReload()
        {
            var currentWeapon = GetCurrentWeapon();

            if (currentWeapon != null)
            {
                if (currentWeapon.CanReload())
                {
                    StopAiming();
                    currentWeapon.WeaponReload();
                }
                else if (currentWeapon.GetRemainingAmmo() <= 0)
                {
                    if (currentWeapon.DisableOnEmpty)
                    {
                        RemoveWeapon(currentWeapon);

                        // change to next weapon called above
                    }
                }
            }
        }

        public bool AddWeapon(Item item)
        {
            var weapon1P = item.Item1P.GetComponent<WeaponController>();
            var weapon3PObj = item.Item3P;

            // same source prefab
            if (HasWeapon(weapon1P) != null)
            {
                return false;
            }

            var index = item.BagPosition.GetIntValue();
            if (_weapon1Ps[index] == null)
            {
                // 1p
                WeaponController weapon1PNew = Instantiate
                    (weapon1P, WeaponPosition1P);
                weapon1PNew.transform.localPosition = Vector3.zero;
                weapon1PNew.transform.localRotation = Quaternion.identity;

                weapon1PNew.Owner = gameObject;
                weapon1PNew.IsPlayer = IsPlayer;

                weapon1PNew.SourcePrefab = weapon1P.gameObject;
                weapon1PNew.Setlayer((int)Weapon1PLayer);

                // audio
                weapon1PNew.SetupAudio();

                // qc model
                weapon1PNew.GetComponentInChildren<Camera>().Disable();
                weapon1PNew.GetComponentInChildren<Light>().Disable();

                // 3p
                if (weapon3PObj)
                {
                    Debug.Assert(WeaponPosition3P != null);

                    var weapon3PNewObj = Instantiate
                        (weapon3PObj, WeaponPosition3P);
                    weapon3PNewObj.transform.ResetLocalTransform();
                    // offset
                    var pvModel = weapon3PNewObj.GetComponent<PVModel>();
                    if (pvModel != null)
                    {
                        weapon3PNewObj.transform.localPosition
                            = pvModel.right.Pos_OnHand;
                    }
                    weapon3PNewObj.transform
                        .Setlayer((int)Weapon3PLayer);

                    // disable physics
                    weapon3PNewObj.DisablePhysics();

                    // set controller
                    weapon1PNew.SetWeapon3P(ref weapon3PNewObj);
                }


                weapon1PNew.WeaponDraw(false);

                _weapon1Ps[index] = weapon1PNew;


                // events
                if (OnAddedWeapon != null)
                {
                    OnAddedWeapon.Invoke(weapon1PNew);
                }

                return true;
            }

            if (GetCurrentWeapon() == null)
            {
                ChangeToNextWeapon(true);
            }

            return false;
        }

        #region Change Weapon
        // Iterate on all weapon slots to find the next valid weapon to switch to
        public void ChangeToNextWeapon(bool ascendingOrder = true)
        {
            int newWeaponIndex = -1;
            int closestSlotDistance = _weapon1Ps.Length;
            for (int i = 0; i < _weapon1Ps.Length; i++)
            {
                // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
                // and select it if it's the closest distance yet
                if (i != CurrentBagPos && GetWeaponAtIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(CurrentBagPos, i, ascendingOrder);

                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            ChangeToWeapon(newWeaponIndex);
        }

        // Calculates the "distance" between two weapon slot indexes
        // For example: if we had 5 weapon slots,
        // the distance between slots #2 and #4 would be 2 in ascending
        // and 3 in descending order
        int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;

            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
            }

            if (distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = _weapon1Ps.Length + distanceBetweenSlots;
            }

            return distanceBetweenSlots;
        }

        public void ChangeToWeapon(int newBagId, bool force = false)
        {
            var currentWeapon = GetCurrentWeapon();
            var nextWeapon = GetWeaponAtIndex(newBagId);

            if (force || nextWeapon)//newBagId != CurrentBagPos
            {
                // current weapon
                if (currentWeapon != null)
                {
                    StopAiming();
                    currentWeapon.WeaponDraw(false);
                }

                // next weapon
                if (nextWeapon != null)
                {
                    nextWeapon.WeaponDraw(true);
                }

                // Set bag id
                _lastBagPos = CurrentBagPos;
                CurrentBagPos = newBagId;

                if (OnSwitchedToWeapon != null)
                {
                    OnSwitchedToWeapon.Invoke(nextWeapon);
                }
            }
        }

        public virtual void StopAiming()
        {
        }


        #endregion


        public WeaponController GetCurrentWeapon()
        {
            return GetWeaponAtIndex(CurrentBagPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">from 1</param>
        /// <returns></returns>
        public WeaponController GetWeaponAtIndex(int index)
        {
            if (index > 0
                && index < _weapon1Ps.Length)
            {
                return _weapon1Ps[index];
            }

            return null;
        }

        public WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            // Checks if we already have a weapon coming from the specified prefab
            for (var index = 0; index < _weapon1Ps.Length; index++)
            {
                var weaponHave = _weapon1Ps[index];
                if (weaponHave != null
                    && weaponHave.SourcePrefab == weaponPrefab.gameObject)
                {
                    return weaponHave;
                }
            }

            return null;
        }

        public void RemoveWeapon(EWeaponBagPosition bagPosition)
        {
            var weapon = GetWeaponAtIndex((int)bagPosition);
            RemoveWeapon(weapon);
        }

        public void RemoveWeapon(int index)
        {
            var weapon = GetWeaponAtIndex(index);
            RemoveWeapon(weapon);
        }

        protected bool RemoveWeapon(WeaponController weapon1P)
        {
            for (int i = 0; i < _weapon1Ps.Length; i++)
            {
                // when weapon found, remove it
                if (weapon1P != null // ignore null slots
                    && _weapon1Ps[i] == weapon1P)
                {
                    // 3p
                    weapon1P.weapon3P.SelfDestroy();
                    // 1p
                    _weapon1Ps[i] = null;
                    weapon1P.gameObject.SelfDestroy();

                    if (OnRemovedWeapon != null)
                    {
                        OnRemovedWeapon.Invoke(weapon1P);
                    }

                    // Handle case of removing active weapon (switch to next weapon)
                    if (i == CurrentBagPos)
                    {
                        ChangeToNextWeapon(true);
                    }

                    return true;
                }
            }

            return false;
        }

        internal void SetWeaponItems(List<Item> items)
        {
            // Add starting weapons
            foreach (var item in items)
            {
                var weapon = item.GetComponent<WeaponController>();
                if (weapon != null)
                {
                    AddWeapon(item);
                }
            }

            ChangeToNextWeapon(true);
        }

        protected virtual void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon != null)
            {
                newWeapon.WeaponDraw(true);
            }
        }

        public WeaponController GetWeapon(EWeaponBagPosition bagPosition)
        {
            return GetWeaponAtIndex((int)bagPosition);
        }

        internal void SetStartWeapons()
        {
            Debug.Assert(startWeapons.HasValue());

            SetWeaponItems(startWeapons);
        }

        internal void SetStartNanoWeapons()
        {
            Debug.Assert(startNanoWeapons.HasValue());

            SetWeaponItems(startNanoWeapons);
        }

        public void RemoveAllWeapons()
        {
            foreach (var item in _weapon1Ps)
            {
                RemoveWeapon(item);
            }
        }



        // End
    }
}