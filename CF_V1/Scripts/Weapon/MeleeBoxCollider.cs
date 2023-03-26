using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

/// <summary>
/// for heavy
/// </summary>
public class MeleeBoxCollider : MonoBehaviour
{
    float damageHeavy = 65;
    [HideInInspector] public BoxCollider boxCollider;


    void Start()
    {
        SetCollider();


        // 1p
        var weapon1P = GetComponentInParent<WeaponController>();
        if (weapon1P != null)
        {
            damageHeavy = weapon1P.damageHeavy;
        }
        else
        {
            // 3p
            var weaponItem = GetComponentInParent<WeaponItem>();
            if (weaponItem != null)
            {
                damageHeavy = weaponItem.Weapon1P.damageHeavy;
            }
        }
    }

    private void SetCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.enabled = false;

        // todo both way is not effecting enemy layer
        // https://docs.unity3d.com/Manual/LayerBasedCollision.html
        //Physics.IgnoreLayerCollision(AllLayers.Weapon1P.GetValue(), 
        //    AllLayers.Player3P.GetValue());
    }

    private void OnTriggerEnter(Collider other)
    {
        var layer = other.gameObject.layer;
        var pawn = other.GetComponent<IDamageable>();
        if (pawn != null)
        {
            pawn.TakeDamage(damageHeavy, EDamageType.Knife);

            Vector3 impactNormal = Vector3.up;
            ImpactManager.Instance
               .PlayImpact(this.transform.position,
               Vector3.up,
               null);
        }
    }

    internal void BeginCheck()
    {
        boxCollider.enabled = true;
    }

    internal void EndCheck()
    {
        //boxCollider.enabled = false;
    }
}
