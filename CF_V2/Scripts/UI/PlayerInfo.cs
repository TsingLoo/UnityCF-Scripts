using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class PlayerInfo : MonoBehaviour
    {
        public Image HealthFillImage;
        public TextMeshProUGUI ArmorValue;
        public TextMeshProUGUI HealthValue;

        Health m_PlayerHealth;

        void Start()
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerController, PlayerInfo>(
                playerController, this);

            m_PlayerHealth = playerController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerInfo>(m_PlayerHealth, this,
                playerController.gameObject);
        }

        void Update()
        {
            if (m_PlayerHealth && m_PlayerHealth.IsAlive())
            {
                // update health bar value
                HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
            
                ArmorValue.text = m_PlayerHealth.CurrentArmor.ToString();
                HealthValue.text = m_PlayerHealth.CurrentHealth.ToString();
            }
        }
    }
}