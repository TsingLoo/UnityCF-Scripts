using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponUIManager : MonoBehaviour
    {
        public WeaponInfo WeaponInfo;
        public GameObject WeaponSwitchPrefab;
        public RectTransform WeaponSwitchPanel;

        PlayerWeaponsManager m_PlayerWeaponsManager;
        List<WeaponInfo> _weaponInfos = new List<WeaponInfo>();

        void Start()
        {
            m_PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponUIManager>(m_PlayerWeaponsManager,
                this);

            WeaponController activeWeapon = m_PlayerWeaponsManager.GetCurrentWeapon();
            if (activeWeapon)
            {
                OnAddWeapon(activeWeapon);
                ChangeWeapon(activeWeapon);
            }

            m_PlayerWeaponsManager.OnAddedWeapon += OnAddWeapon;
            m_PlayerWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            m_PlayerWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void OnAddWeapon(WeaponController newWeapon)
        {
            GameObject ammoCounterInstance = Instantiate(WeaponSwitchPrefab, 
                WeaponSwitchPanel);
            WeaponInfo newAmmoCounter = ammoCounterInstance.GetComponent<WeaponInfo>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponInfo, WeaponUIManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.UpdateInfo = false;
            newAmmoCounter.InitWeapon(newWeapon);

            _weaponInfos.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newWeapon)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < _weaponInfos.Count; i++)
            {
                // todo use id, or bag position
                if (_weaponInfos[i].WeaponAssetName == newWeapon.WeaponAssetName)
                {
                    foundCounterIndex = i;
                    Destroy(_weaponInfos[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                _weaponInfos.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder
                .ForceRebuildLayoutImmediate(WeaponSwitchPanel);

            if (weapon)
            {
                WeaponInfo.InitWeapon(weapon);
            }
        }

        // End
    }
}