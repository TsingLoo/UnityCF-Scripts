using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class EnemyCounter : MonoBehaviour
    {
        [Header("Enemies")] [Tooltip("Text component for displaying enemy objective progress")]
        public Text EnemiesText;

        BotManager m_EnemyManager;

        void Awake()
        {
            m_EnemyManager = FindObjectOfType<BotManager>();
            DebugUtility.HandleErrorIfNullFindObject<BotManager, EnemyCounter>(m_EnemyManager, this);
        }

        void Update()
        {
            EnemiesText.text = m_EnemyManager.BotLeftCount + "/" + m_EnemyManager.TotalBots;
        }
    }
}