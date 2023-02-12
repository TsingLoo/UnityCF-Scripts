using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// holds weapon1P
public class WeaponPool : MonoBehaviour
{
    static WeaponPool Instance;
    //static public ImpactManager Instance { get; protected set; }

    void Awake()
    {
        Instance = this;
    }

    public static void Init()
    {
    }
    
    public static WeaponController Add(WeaponController weapon, 
        Transform weaponPosition1P,
        bool worldPositionStays)
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
                (weapon, 1, weaponPosition1P);

            // todo worldPositionStays
            return weapon;
        }
    }

    public static WeaponController Get(WeaponController weapon)
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
