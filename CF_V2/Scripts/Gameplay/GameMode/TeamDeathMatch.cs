using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class TeamDeathMatch : GameModeBase
    {
        public override void OnPlayerDeath()
        {
            base.OnPlayerDeath();

            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.Respawn(playerTeamStarts[0].transform);
            }
        }
    }
}