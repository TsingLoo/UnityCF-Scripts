using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// todo change to 
// https://assetstore.unity.com/packages/tools/gui/scriptable-dynamic-crosshair-229593
public class CrossHair : MonoBehaviour
{
    public static CrossHair Instance { get; private set; }

    public GameObject _crosshairGroup;
    public GameObject _normalCrosshair;
    public GameObject _launcherCrosshair;

    void OnEnable()
    {
        Instance = this;
    }


    public void ShowCrosshair()
    {
        var currentWeapon = PlayerController.Instance.GetCurrentWeapon;
        
        if (currentWeapon != null
             && currentWeapon.weaponType != EWeaponType.Sniper)
        {
            _crosshairGroup.Show();

            // todo add enum CrosshairType
            if (currentWeapon.weaponType == EWeaponType.Launcher)
            {
                CrossHair.Instance._normalCrosshair.Hide();
                CrossHair.Instance._launcherCrosshair.Show();
            }
            else
            {
                CrossHair.Instance._normalCrosshair.Show();
                CrossHair.Instance._launcherCrosshair.Hide();
            }
        }
    }

    public void HideCrosshair()
    {
        _crosshairGroup.Hide();
    }
}
