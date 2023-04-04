using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class TeamDeathMatch : GameModeBase
    {
        public override void OnPlayerDeath()
        {
            base.OnPlayerDeath();

            // todo spawn time
            DelayAction(3f, () =>
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.Respawn(playerTeamStarts[0].transform);
                }
            });
        }
    }
}