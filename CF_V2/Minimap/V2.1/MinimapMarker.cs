using NaughtyAttributes;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Marker Icon
    /// </summary>
    public class MinimapMarker : MonoBehaviour
    {
        [Header("Icons")]
        [ShowAssetPreview]
        public Sprite teamIcon;
        [ShowAssetPreview]
        public Sprite enemyIcon;
        [ShowAssetPreview]
        public Sprite deathIcon;

        // todo
        //public Color DefaultColor;
        //public Color AltColor;

        [Header("Refs")]
        public Image MainImage;

        BotController _botController;
        ETeam _playerTeam;
        ETeam _team;

        public void Init(MinimapElement worldElement)
        {
            transform.forward = worldElement.transform.forward;

            // health
            var health = worldElement.transform.GetComponent<Health>();
            if (health)
            {
                health.onDie += OnDie;
            }

            // this actor
            var actor = worldElement.transform.GetComponent<Actor>();
            if (actor)
            {
                actor.onTeamChange += OnTeamChange;
            }

            // player actor
            var player = FindObjectOfType<PlayerController>();
            if (player)
            {
                player.GetComponent<Actor>().onTeamChange += OnTeamChange;
            }

            // 
            _botController = worldElement.transform.GetComponent<BotController>();
            if (_botController)
            {
                // use actor instead
                //_botController.onTurnNano += OnBotTurnNano;

                // todo
                //_botController.onDetectedTarget += OnDetectTarget;
                //_botController.onLostTarget += OnLostTarget;

                //OnLostTarget();
            }

        }

        private void OnTeamChange(ETeam newTeam, bool isPlayer)
        {
            // change team
            if(isPlayer)
            {
                _playerTeam = newTeam;
            }
            else
            {
                _team = newTeam;
            }

            // change icon
            if(MainImage != null) // todo check why
            {
                if (_team == _playerTeam)
                {
                    MainImage.sprite = teamIcon;
                }
                else
                {
                    MainImage.sprite = enemyIcon;
                }
            }
        }

        //private void OnBotTurnNano()
        //{
        //    MainImage.sprite = enemyIcon;
        //}

        private void OnDie()
        {
            MainImage.sprite = deathIcon;
        }

        //public void OnDetectTarget()
        //{
        //    MainImage.color = AltColor;
        //}

        //public void OnLostTarget()
        //{
        //    MainImage.color = DefaultColor;
        //}
    }
}