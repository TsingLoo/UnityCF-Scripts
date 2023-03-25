using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class HealthPickup : MovingPickup
    {
        [Header("Parameters")] 
        public float HealAmount;

        protected override void OnPicked(PawnController player)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth && playerHealth.CanPickup())
            {
                playerHealth.Heal(HealAmount);

                PlayPickupFX();
                Destroy(gameObject);
            }
        }
    }
}