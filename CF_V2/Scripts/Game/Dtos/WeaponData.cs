

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class WeaponData
    {
        public string WeaponName { get; set; }
        public string WeaponAssetName { get; set; }

        public EWeaponType WeaponType { get; set; }
        public EWeaponBagPosition WeaponBagPos { get; set; }
        public EWeaponFireMode WeaponFireMode { get; set; }
        public EWeaponAnimType WeaponAnimType { get; set; }
        /// <summary>
        /// TPS only
        /// </summary>
        public bool IsRPGWeapon { get; set; } = false;

        public float FireGap { get; set; }
        public float HeavyGap { get; set; }

        public int Damage { get; set; }
        public int DamageHeavy { get; set; }

        public int ClipSize { get; set; }
        public int DefaultClips { get; set; }
        public int MaxClips { get; set; }

        public float MeleeRangeFire { get; set; }
        public float MeleeRangeHeavy { get; set; }
        public bool HasHeavy { get; set; }

        public bool HasAim { get; set; }

        public List<AnimationClipDto> AnimDtos { get; set; } = new();
        public List<AudioClip> AudioClips { get; set; }

        public AnimationClipDto GetAnimDto(string clipName)
        {
            var nameAffix = clipName.Split('_').LastOrDefault();
            var res = AnimDtos
                .FirstOrDefault(it => it.AnimNameAffix == nameAffix);
            
            return res;
        }
    }
}