using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.FPS.Game
{
    public enum ETeam
    {
        Unknown,

        //Terrorist
        [Description("BL")] // todo ref
        T = 1,

        //Counter Terrorist
        [Description("GR")]
        CT = 2,

        Human = 3,
        Nano = 4,
    }
}