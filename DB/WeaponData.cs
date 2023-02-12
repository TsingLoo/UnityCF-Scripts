

using System.Collections.Generic;
using UnityEngine;


public class WeaponData
{
    public string Name  { get; set; }
    public EWeaponType WeaponType  { get; set; }
    public WeaponBagPosition BagPosition  { get; set; }
    public EWeaponAnimType WeaponAnimType { get; set; }
    public EWeaponFireMode AutoType  { get; set; }
    public List<AnimationClipDto> Anim1PDtos { get; set; } = new ();
    public List<AnimationClipDto> Anim3Ps  { get; set; } = new();

    public float Damage  { get; set; }
    public int ClipSize  { get; set; }
    public int DefaultClips  { get; set; }
    public int MaxClips { get; set; }

    public float Weight  { get; set; }
}
