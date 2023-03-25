using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class BotManager : MonoBehaviour
    {
        public List<BotController> Bots { get; private set; }
        public int TotalBots { get; private set; }
        public int BotLeftCount => Bots.Count;

        void Awake()
        {
            Bots = new List<BotController>();
        }

        public void RegisterBot(BotController enemy)
        {
            Bots.Add(enemy);

            TotalBots++;
        }

        public void UnregisterBot(BotController botKilled)
        {
            Bots.Remove(botKilled);

            // send event
            BotDeathEvent evt = Events.BotDeathEvent;
            evt.Bot = botKilled.gameObject;
            evt.RespawnTime = botKilled.DeathDuration;

            evt.Team = evt.Bot.GetComponent<Actor>().Team;
            evt.BotLeftCount = BotLeftCount;

            EventManager.Broadcast(evt);
        }
    }
}