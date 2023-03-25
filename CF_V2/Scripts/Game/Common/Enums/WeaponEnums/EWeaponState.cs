using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.FPS.Game
{
    public enum EWeaponState
    {
        Draw,

        Idle,

        /// <summary>
        /// PreFire
        /// M134, Grenade
        /// </summary>
        FireReady,
        Fire,

        Reload
    }
}