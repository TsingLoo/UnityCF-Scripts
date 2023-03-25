
namespace Unity.FPS.Game
{
    public enum EditorLayer
    {
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Player = 3, // player
        Water = 4,
        UI = 5,

        Bot = 6,
        Pickup = 7,
        PostProcessing = 8,

        Weapon1P = 9,
        Weapon3P = 10,

        Projectile = 11,

        // bot pawn
        BotWeapon1P = 12, 
        BotWeapon3P = 13,
    }
}