

namespace Unity.FPS.Gameplay
{
    public class JetpackPickup : MovingPickup
    {
        protected override void OnPicked(PawnController byPlayer)
        {
            var jetpack = byPlayer.GetComponent<Jetpack>();
            
            if (jetpack != null && jetpack.TryUnlock())
            {
                PlayPickupFX();
                Destroy(gameObject);
            }
        }
    }
}