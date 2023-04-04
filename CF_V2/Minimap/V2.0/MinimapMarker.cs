using System;
using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Marker Icon
    /// </summary>
    public class MinimapMarker : MonoBehaviour
    {
        public Image MainImage;
        public Sprite deathIcon;

        public Color DefaultColor;
        public Color AltColor;

        BotController _botController;

        public void Init(MinimapElement worldElement)
        {
            transform.forward = worldElement.transform.forward;

            var health = worldElement.transform.GetComponent<Health>();
            if (health)
            {
                health.OnDie += OnDie;
            }


            return;//todo

            _botController = worldElement.transform.GetComponent<BotController>();
            if (_botController)
            {
                _botController.onDetectedTarget += OnDetectTarget;
                _botController.onLostTarget += OnLostTarget;

                OnLostTarget();
            }

        }

        private void OnDie()
        {
            MainImage.sprite = deathIcon;
        }

        public void OnDetectTarget()
        {
            MainImage.color = AltColor;
        }

        public void OnLostTarget()
        {
            MainImage.color = DefaultColor;
        }
    }
}