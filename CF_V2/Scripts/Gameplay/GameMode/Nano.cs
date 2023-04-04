using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Gameplay
{
    public class Nano : GameModeBase
    {
        public bool canPlayerTurnNano = false;

        List<AudioClip> _nanoClips = new List<AudioClip>();

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<TurnNanoEvent>(OnTurnNano);

            _nanoClips = Resources.LoadAll<AudioClip>(ResPaths.Sound_Nano).ToList();

            base.onBotAdd += OnBotAdd;

            // todo ref, delay in case start is not last
            DelayAction(1f, () => StartGame());
        }


        private void OnTurnNano(TurnNanoEvent evt)
        {
            if (_gameStarted)
            {
                // check all human
                var allActors = _actorsManager.GetAllActors();
                if (!allActors.Exists(it => it.Team == ETeam.Human))
                {
                    // ghost win
                    PlaySoundClip("GhostWin");

                    DelayAction(3f, () => RestartGame());
                }
            }
        }


        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        protected override void StartGame()
        {
            base.StartGame();

            var actors = _actorsManager.Actors;

            // set human
            foreach (var actor in actors)
            {
                actor.ChangeTeam(ETeam.Human);
            }

            StartCoroutine(StartCountdown());
            
        }

        private void GenerateNano()
        {
            var allActors = _actorsManager.GetAllActors();
            // nano
            if (!canPlayerTurnNano)
            {
                allActors.Remove(allActors.FirstOrDefault(it => it.isPlayer));
            }

            if (allActors.HasValue())
            {
                var id = allActors.GetRandomId();
                var pawn = allActors[id].GetComponent<PawnController>();
                if (pawn != null)
                {
                    pawn.TurnNano();

                    // todo sound in characterModel
                    PlaySoundClip("NanoAppearSnd"); // todo NanoAppearSnd2
                }
            }
        }

        float _countDown = 0f;
        public IEnumerator StartCountdown(float countdownValue = 10)
        {
            _countDown = countdownValue;
            while (_countDown >= 0)
            {
                Debug.Log("Countdown: " + _countDown);

                // sound
                if( _countDown > 0)
                {
                    int clipNum = (int)_countDown;
                    PlaySoundClip("Ghost_Count_" + clipNum);
                }
                // nano
                if(_countDown == 0)
                {
                    GenerateNano();

                    _gameStarted = true;
                }

                yield return new WaitForSeconds(1.0f);
                
                _countDown--;
            }
        }

        void PlaySoundClip(string clipName)
        {
            var clip = _nanoClips.FirstOrDefault
                        (it => it.name.StartsWith(clipName));

            PlaySoundMix(clip, AudioUtility.AudioGroups.Hud);
        }

        private void OnBotAdd(GameObject newBot)
        {
            var actor = newBot.GetComponent<Actor>();
            if (actor.Team == ETeam.Nano)
            {
                newBot.GetComponent<PawnController>().TurnNano();
            }
        }
        // 
    }
}