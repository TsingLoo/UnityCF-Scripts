using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class GameModeBase : BaseBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject BotPrefab;
        public int botAddCount = 1;
        public bool equalSides = true;

        public List<StartPoint> startPoints;
        public List<StartPoint> playerTeamStarts;
        public List<StartPoint> enemyTeamStarts;

        public UnityAction<GameObject> onBotAdd;

        protected ActorsManager _actorsManager;
        protected bool _gameStarted = false;

        // private
        int _playerTeamCount;
        int _enemyTeamCount;

        // todo
        ETeam _playerTeam = ETeam.T;
        ETeam _enemyTeam = ETeam.CT;


        private void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            _gameStarted = false;

            startPoints = FindObjectsOfType<StartPoint>().ToList();

            playerTeamStarts = startPoints.Where(it => it.Team == _playerTeam).ToList();
            enemyTeamStarts = startPoints.Where(it => it.Team != _playerTeam).ToList();

            _playerTeamCount = 0;
            _enemyTeamCount = 0;

            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                SpawnPawn(PlayerPrefab, playerTeamStarts[0]);
            }

            if (botAddCount > 0)
            {
                for (int i = 0; i < botAddCount; i++)
                {
                    SpawnBot(BotPrefab);
                }
            }

        }

        /// <summary>
        /// Set to last: (restart needed?)
        /// Project setting - Script Execution Order
        /// https://docs.unity3d.com/Manual/class-MonoManager.html
        /// </summary>
        protected virtual void Start()
        {
            _actorsManager = FindObjectOfType<ActorsManager>();

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
                // Respawn:
                var newBot = SpawnBot(BotPrefab, evt.Team);
                newBot.GetComponent<Actor>().Team = evt.Team;

                if (onBotAdd != null)
                {
                    onBotAdd.Invoke(newBot);
                }

                EventManager.Broadcast(new BotAddEvent() { Bot = newBot });
            });
        }

        public virtual void OnPlayerDeath()
        {
        }

        protected virtual void StartGame() 
        {
        }

        #region Spawn
        private void SpawnBot(GameObject botPrefab)
        {
            // enemy first
            if(_enemyTeamCount <= _playerTeamCount)
            {
                SpawnBot(botPrefab, _enemyTeam);
                _enemyTeamCount++;
            }
            else// if(_playerTeamCount <= _enemyTeamCount)
            {
                SpawnBot(botPrefab, _playerTeam);
                _playerTeamCount++;
            }
        }

        private GameObject SpawnBot(GameObject botPrefab, ETeam team)
        {
            var teamPoints = startPoints.Where(it => it.Team == team).ToList();
            // in case nano change team
            if (!teamPoints.HasValue())
            {
                teamPoints = startPoints;
            }

            // spawn
            var id = teamPoints.GetRandomId();
            return SpawnPawn(botPrefab, teamPoints[id]);
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

            return newPawn;
        }
        #endregion

    }
}