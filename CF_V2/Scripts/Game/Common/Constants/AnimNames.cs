using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Unity.FPS.Game
{
    public class BodyAnims
    {
        // player
        public const string Death1 = nameof(Death1);

        public const string Hit_Forward = nameof(Hit_Forward);

    }

    /// <summary>
    /// Dummy
    /// _Idle
    /// </summary>
    public class AnimNames
    {
        // pawn
        public const string BodyAnim = nameof(BodyAnim);

        // prefixs
        public const string Dummy = nameof(Dummy);
        public const string AnimSpeed = nameof(AnimSpeed);

        // for animations
        public const string Idle = nameof(Idle);
        public const string Empty = nameof(Empty);

        public const string Draw = nameof(Draw);
        public const string Reload = nameof(Reload);
        public const string Run = nameof(Run);

        public const string FireReady = nameof(FireReady);
        public const string Fire = nameof(Fire);
        public const string FireAfter = nameof(FireAfter);
        // last bullet
        public const string FireLast = nameof(FireLast);
        
        // combo1, combo2
        public const string Combo = nameof(Combo);
        public const string Combo1 = nameof(Combo1);
        public const string Combo2 = nameof(Combo2);
        public const string Combo3 = nameof(Combo3);
        public const string Combo4 = nameof(Combo4);

        public const string Heavy = nameof(Heavy);

        // for sounds
        public const string MagIn = nameof(MagIn);
        public const string MagOut = nameof(MagOut);
        public const string Circle = nameof(Circle);
        public const string Pull = nameof(Pull);

        public static string Combine(string a, string b, string c)
        {
            return Combine(Combine(a, b), c);
        }
        public static string Combine(string a, string b)
        {
            return a + "_" + b;
        }


    }
}