using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public static WeaponUI Instance { get; private set; }

    [Header("Weapon Info")]
    public Text WeaponName;

    public RawImage WeaponIconBG;
    public RawImage WeaponIconEffect;
    public RawImage WeaponIconLine;

    [Header("Ammo")]
    public GameObject AmmoUI;
    public Text AmmoContent;
    public Text AmmoCarry;

    void OnEnable()
    {
        Instance = this;
        //WeaponName = FindObjectOfType<TextMeshProUGUI>();
    }


    public void UpdateWeaponName(WeaponController weapon)
    {
        WeaponName.text = weapon.WeaponName;

        SetWeaponIcon(weapon);
    }
    public void SetWeaponIcon(WeaponController weapon)
    {
        // must be in: Assets/Resources
        //var baseDir = "UI/Weapon_Icon/";

        #region Icons

        var asset = weapon.WeaponAssetName;
        var iconDir = $"Weapons/{asset}/Icons";
        var icons = Resources.LoadAll<Texture2D>(iconDir);
        if (icons.HasValue())
        {
            // BG
            var iconBG = icons
                .Where(it => it.name.EndsWith("_BG"))
                .FirstOrDefault();

            if (iconBG != null)
            {
                WeaponIconBG.texture = iconBG;
                WeaponIconBG.Show();
            }
            else
            {
                WeaponIconBG.Hide();
            }

            // Effect

            // Line
            var iconLine = icons
                .Where(it => it.name.EndsWith("_LINE")
                    || it.name.EndsWith("_Line")
                    || it.name.EndsWith("_line"))
                .FirstOrDefault();

            if (iconLine != null)
            {
                WeaponIconLine.texture = iconLine;
                WeaponIconLine.Show();
            }
            else
            {
                WeaponIconLine.Hide();
            }
        }

        // todo add effect, change alpha
        //var effectRes = Resources.Load<Texture2D>
        //    (baseDir + weapon.WeaponResourceName + "_EFFECT");
        //if (effectRes != null)
        //{
        //    WeaponIconEffect.texture = effectRes;
        //}

        #endregion
        #region Ammo

        if (weapon.weaponType == EWeaponType.Melee)
        {
            AmmoUI.Hide();
        }
        else
        {
            AmmoUI.Show();
        }
        #endregion

        #region Crosshair

        if (weapon.weaponType == EWeaponType.Sniper)
        {
            CrossHair.Instance.HideCrosshair();
        }
        else
        {
            CrossHair.Instance.ShowCrosshair();
        }
        #endregion
    }


    public void UpdateAmmoRemain(WeaponController weapon)
    {
        AmmoContent.text = weapon.GetAmmoRemain.ToString();
    }

    public void UpdateAmmoAmount(int amount)
    {
        AmmoCarry.text = amount.ToString();
    }
}
