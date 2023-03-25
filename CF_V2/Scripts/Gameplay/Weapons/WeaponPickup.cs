using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class WeaponPickup : Pickup
    {
        protected override void Init()
        {
            base.Init();

            // todo set in excel?
            PickupSound = Resources.Load<AudioClip>("Sound/Weapon/Weapon_Pickup");
            PickupSoundVolume = 0.8f;
        }

        // End
    }
}