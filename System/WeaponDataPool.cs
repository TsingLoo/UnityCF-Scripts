using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// holds weapon3P
public class WeaponDataPool : MonoBehaviour
{
    static WeaponDataPool Instance;

    void Awake()
    {
        Instance = this;
    }

    public static void Init()
    {
    }
    
    public static WeaponItem Add(WeaponItem weapon, 
        Transform parentTransform = null,
        bool worldPositionStays = false)
    {
        var weaponInPool = Get(weapon);

        // in pool
        if (weaponInPool != null)
        {
            return weaponInPool;
        }
        else // add
        {
            PoolSystem.Instance.InitPool
                (weapon, 1, parentTransform);

            // todo worldPositionStays
            return weapon;
        }
    }

    public static WeaponItem Get(WeaponItem weapon)
    {
        if(weapon == null)
        {
            return null;
        }

        // in pool
        var weaponInPool = PoolSystem.Instance.GetInstance(weapon);

        return weaponInPool;
    }

}
