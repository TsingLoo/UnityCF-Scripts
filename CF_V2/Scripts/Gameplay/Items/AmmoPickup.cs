using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class AmmoPickup : MovingPickup
    {
        // todo use ammoType?
        [Tooltip("Weapon those bullets are for")]
        public WeaponController Weapon;

        [Tooltip("Number of bullets the player gets")]
        public int BulletCount = 30;

        protected override void OnPicked(PawnController byPlayer)
        {
            PlayerWeaponsManager playerWeaponsManager = byPlayer.GetComponent<PlayerWeaponsManager>();
            if (playerWeaponsManager)
            {
                WeaponController weapon = playerWeaponsManager.HasWeapon(Weapon);
                if (weapon != null)
                {
                    weapon.AddAmmo(BulletCount);

                    AmmoPickupEvent evt = Events.AmmoPickupEvent;
                    //evt.Weapon = weapon;//todo
                    EventManager.Broadcast(evt);

                    PlayPickupFX();
                    Destroy(gameObject);
                }
            }
        }
    }
}
