using System.Linq;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldspaceHealthBar : MonoBehaviour
    {
        public Transform HealthBarPivot;
        
        public Health Health;
        public Image HealthBarImage;
        public bool HideFullHealthBar = true;

        public TextMeshProUGUI CurrentHealthText;
        //todo
        public TextMeshProUGUI DamageAmmount;

        private void Start()
        {
            HealthBarPivot = transform.parent;
            Health = GetComponentInParent<Health>();
        }

        void Update()
        {
            // update health bar value
            HealthBarImage.fillAmount = Health.CurrentHealth / Health.MaxHealth;
            if (CurrentHealthText)
            {
                CurrentHealthText.text = Health.CurrentHealth.ToString();
            }

            // rotate health bar to face the camera/player
            Vector3 lookAtPos; 
            if(Camera.main!= null)
            {
                lookAtPos = Camera.main.transform.position;
            }
            else
            {
                lookAtPos = Camera.allCameras.FirstOrDefault(
                    it=> it.name == "Camera3P")
                    .transform.position;
            }
            HealthBarPivot.LookAt(lookAtPos);

            // hide health bar if needed
            if (HideFullHealthBar)
                HealthBarPivot.gameObject.SetActive(HealthBarImage.fillAmount != 1);
        }
    }
}