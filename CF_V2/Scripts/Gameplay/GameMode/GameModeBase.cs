using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class GameModeBase : BaseBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject BotPrefab;
        public bool addBot = true;

        public List<StartPoint> startPoints;
        public List<StartPoint> playerTeamStarts;
        public List<StartPoint> enemyTeamStarts;

        private void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            startPoints = FindObjectsOfType<StartPoint>().ToList();
            // todo
            var playerTeam = ETeam.T;

            playerTeamStarts = startPoints.Where(it => it.Team == playerTeam).ToList();
            enemyTeamStarts = startPoints.Where(it => it.Team != playerTeam).ToList();


            SpawnPawn(PlayerPrefab, playerTeamStarts[0]);

            if (addBot)
            {
                foreach (var botStart in startPoints)
                {
                    SpawnPawn(BotPrefab, botStart);
                }
            }
        }

        private void Start()
        {
            EventManager.AddListener<BotDeathEvent>(OnBotDeath);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<BotDeathEvent>(OnBotDeath);
        }

        void OnBotDeath(BotDeathEvent evt)
        {
            DelayAction(evt.RespawnTime, () =>
            {
                var newBot = SpawnPawn(BotPrefab, evt.Team);

                EventManager.Broadcast(new BotAddEvent() { Bot = newBot });
            });
        }

        public virtual void OnPlayerDeath()
        {
        }

        private GameObject SpawnPawn(GameObject botPrefab, ETeam team)
        {
            var teamPoints = startPoints.Where(it => it.Team == team).ToList();
            return SpawnPawn(botPrefab, teamPoints.FirstOrDefault());
        }


        private GameObject SpawnPawn(GameObject pawnPrefab,
            StartPoint spawnPoint)
        {
            var newPawn = Instantiate(pawnPrefab,
                spawnPoint.transform.position,
                Quaternion.identity);

            // todo
            // face direction set in pawn init, for camera to face correct 
            //Quaternion.LookRotation(spawnPoint.transform.forward)

            newPawn.GetComponent<Actor>().Team = spawnPoint.Team;

            return newPawn;
        }

    }
}