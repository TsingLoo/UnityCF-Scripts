

using System.Collections.Generic;
using UnityEngine;


public class WeaponDataDto
{
    public string Name  { get; set; }
    //public EWeaponType WeaponType  { get; set; }
    //public EWeaponAutoType AutoType  { get; set; }
    //public WeaponBagPosition BagPosition  { get; set; }
    public List<AnimationClipDto> Anim1Ps { get; set; } = new ();
    //public List<AnimClipData> Anim3Ps  { get; set; } = new();

    public float Damage  { get; set; }
    public int ClipSize  { get; set; }
    public int DefaultClips  { get; set; }
    public int MaxClips { get; set; }

    public float Weight  { get; set; }
}
