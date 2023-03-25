using UnityEngine;

namespace Unity.FPS.Game
{
    public class Actor : MonoBehaviour
    {
        public ETeam Team;
       
        public Transform AimPoint;

        ActorsManager m_ActorsManager;

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
    }
}