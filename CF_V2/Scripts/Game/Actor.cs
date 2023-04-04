using System;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class Actor : MonoBehaviour
    {
        public bool isPlayer = false;

        public ETeam Team;
       
        public Transform AimPoint;

        ActorsManager m_ActorsManager;

        public Action<ETeam, bool> onTeamChange;

        void Start()
        {
            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, Actor>(m_ActorsManager, this);

            // Register actor
            if (!m_ActorsManager.Actors.Contains(this))
            {
                m_ActorsManager.Actors.Add(this);
            }
        }

        void OnDestroy()
        {
            // Unregister as an actor
            if (m_ActorsManager)
            {
                m_ActorsManager.Actors.Remove(this);
            }
        }

        public void ChangeTeam(ETeam newTeam)
        {
            Team = newTeam;

            onTeamChange?.Invoke(newTeam, isPlayer);
        }
    }
}