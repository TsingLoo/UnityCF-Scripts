using System.Collections.Generic;
using UnityEngine;

public class WeaponBag : MonoBehaviour
{
    public List<WeaponItem> startWeapons;

    [HideInInspector]
    public Dictionary<int, WeaponItem> weaponItems = new Dictionary<int, WeaponItem>();

    BasePawnController _ownerPawn;

    private void Awake()
    {
        _ownerPawn = GetComponent<BasePawnController>();
    }

    //public void AddWeaponsToBag()
    //{
    //    weaponItems = new Dictionary<int, WeaponItem>();

    //    foreach (var item in startWeapons)
    //    {
    //        AddWeapon(item);
    //    }
    //}

    public WeaponItem GetWeapon(int index)
    {
        return weaponItems[index];
    }

    public void AddWeapon(WeaponItem weapon)
    {
        if (weaponItems.ContainsKey(weapon.Weapon1P.weaponBagPos.GetValue()))
        {
            RemoveWeapon(weapon);
        }

        weaponItems.Add(weapon.Weapon1P.weaponBagPos.GetValue(), weapon);
    }

    public void RemoveWeapon(WeaponItem weapon)
    {
        weaponItems.Remove(weapon.Weapon1P.weaponBagPos.GetValue());
    }

    public void RemoveWeapon(int bagPos)
    {
        weaponItems.Remove(bagPos);
    }

    public void RemoveWeapon(WeaponBagPosition bagPos)
    {
        weaponItems.Remove(bagPos.GetValue());
    }

    public bool PickupWeapon(WeaponItem weaponItem)
    {
        bool pickedUp = false;

        if(!weaponItems.ContainsKey(weaponItem.Weapon1P.weaponBagPos.GetValue()))
        {
            weaponItem.OnPickUp(_ownerPawn);

            AddWeapon(weaponItem);
            pickedUp = true;
        }

        return pickedUp;
    }

}
