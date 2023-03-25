using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.FPS.Game
{
    public enum EWeaponFireMode
    {
        Auto = 0,
        Semi = 1,
        Charge = 2,

        /// <summary>
        /// semi * 3
        /// </summary>
        Burst = 3,
    }

}