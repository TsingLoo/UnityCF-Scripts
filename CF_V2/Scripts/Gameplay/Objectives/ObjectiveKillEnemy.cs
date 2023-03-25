using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemy : Objective
    {
        public int KillsToCompleteObjective = 5;
        public int NotifyEnemyRemainCount = 1;

        int m_KillTotal;

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<BotDeathEvent>(OnEnemyKilled);

            if (string.IsNullOrEmpty(Title))
            {
                Title = "Kill all enemies";
            }

            if (string.IsNullOrEmpty(Description))
            {
                Description = GetUpdatedKillCount();
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<BotDeathEvent>(OnEnemyKilled);
        }

        void OnEnemyKilled(BotDeathEvent evt)
        {
            if (IsCompleted)
                return;

            m_KillTotal++;

            int targetRemaining = KillsToCompleteObjective - m_KillTotal;

            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedKillCount(), "Objective complete : " + Title);
            }
            else // if (targetRemaining >= 1)
            {
                string notificationText =
                    NotifyEnemyRemainCount >= targetRemaining
                    ? targetRemaining + " enemy left"
                    : string.Empty;

                UpdateObjective(string.Empty,
                    GetUpdatedKillCount(),
                    notificationText);
            }
        }

        /// <summary>
        /// 1 / 5
        /// </summary>
        /// <returns></returns>
        string GetUpdatedKillCount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective;
        }


    }
}