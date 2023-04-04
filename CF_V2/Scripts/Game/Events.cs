using UnityEngine;

namespace Unity.FPS.Game
{
    public static class Events
    {
        public static ObjectiveUpdateEvent ObjectiveUpdateEvent = new ObjectiveUpdateEvent();
        public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new AllObjectivesCompletedEvent();
        public static GameOverEvent GameOverEvent = new GameOverEvent();

        public static PlayerDeathEvent PlayerDeathEvent = new PlayerDeathEvent();
        public static BotAddEvent BotAddEvent = new BotAddEvent();
        public static BotDeathEvent BotDeathEvent = new BotDeathEvent();
        public static KillMarkEvent KillMarkEvent = new KillMarkEvent();

        public static TurnNanoEvent TurnNanoEvent = new TurnNanoEvent();

        public static PickupEvent PickupEvent = new PickupEvent();
        public static AmmoPickupEvent AmmoPickupEvent = new AmmoPickupEvent();

        public static DamageEvent DamageEvent = new DamageEvent();
        public static DisplayMessageEvent DisplayMessageEvent = new DisplayMessageEvent();
    }

    public class ObjectiveUpdateEvent : GameEvent
    {
        public Objective Objective;
        public string DescriptionText;
        public string CounterText;
        public bool IsComplete;
        public string NotificationText;
    }

    public class AllObjectivesCompletedEvent : GameEvent { }

    public class GameOverEvent : GameEvent
    {
        public bool Win;
    }

    public class PlayerDeathEvent : GameEvent
    {
    }

    public class KillMarkEvent : GameEvent
    {
        public EDamageType DamageType;
    }

    public class BotAddEvent : GameEvent
    {
        public GameObject Bot;
    }

    public class BotDeathEvent : GameEvent
    {
        public string PawnName;

        public ETeam Team;
        public GameObject Bot;
        
        public float RespawnTime;
        public int BotLeftCount;
    }

    public class TurnNanoEvent: GameEvent
    {

    }

    public class PickupEvent : GameEvent
    {
        public GameObject Pickup;
    }

    public class AmmoPickupEvent : GameEvent
    {
        // todo
        //public WeaponController Weapon;
    }

    public class DamageEvent : GameEvent
    {
        public GameObject Sender;
        public float DamageValue;
    }

    public class DisplayMessageEvent : GameEvent
    {
        public string Message;
        public float DelayBeforeDisplay;
    }
}
