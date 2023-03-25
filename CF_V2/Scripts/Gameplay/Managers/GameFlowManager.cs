using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Gameplay
{
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        public LayerMask Camera1PLayer;
        public LayerMask Camera1PWeaponLayer;
        public LayerMask Camera3PLayer;

        [Header("Parameters")]
        [Tooltip("Duration of the fade-to-black at the end of the game")]
        public float EndSceneLoadDelay = 3f;

        [Tooltip("The canvas group of the fade-to-black screen")]
        public CanvasGroup EndGameFadeCanvasGroup;

        [Header("GameMode")]
        public GameModeBase GameMode;

        [Header("Win")]
        public string WinSceneName = "WinScene";
        public float DelayBeforeFadeToBlack = 4f;

        public string WinGameMessage;
        public float DelayBeforeWinMessage = 2f;
        
        //todo ref
        public AudioClip VictorySound;

        [Header("Lose")]
        public string LoseSceneName = "LoseScene";

        [Header("Bullet VFX")]
        public List<GameObject> BulletVFXs;
        public List<GameObject> BulletHoles;
        public float BulletVfxTime = 5f;
        public float BulletHoleTime = 60f;
        public float BulletHoleOffset = 0.02f;

        public bool GameIsEnding { get; private set; }

        float m_TimeLoadEndGameScene;
        string m_SceneToLoad;

        [HideInInspector] public float ComboTimer = -1f;

        void Awake()
        {
            Instance = this;

            EventManager.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Start()
        {
            AudioUtility.SetMasterVolume(1);

        }

        void Update()
        {
            if (GameIsEnding)
            {
                float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / EndSceneLoadDelay;
                EndGameFadeCanvasGroup.alpha = timeRatio;

                AudioUtility.SetMasterVolume(1 - timeRatio);

                // See if it's time to load the end scene (after the delay)
                if (Time.time >= m_TimeLoadEndGameScene)
                {
                    SceneManager.LoadScene(m_SceneToLoad);
                    GameIsEnding = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (ComboTimer > 0)
                ComboTimer -= Time.deltaTime;
        }

        public bool ComboTimerOut()
        {
            return ComboTimer <= 0;
        }

        void OnAllObjectivesCompleted(AllObjectivesCompletedEvent evt)
        {
            EndGame(true);
        }

        void OnPlayerDeath(PlayerDeathEvent evt)
        {
            if(GameMode != null)
            {
                GameMode.OnPlayerDeath();
            }
            else
            {
                EndGame(false);
            }
        }

        void EndGame(bool win)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Remember that we need to load the appropriate end scene after a delay
            GameIsEnding = true;
            EndGameFadeCanvasGroup.gameObject.SetActive(true);
            if (win)
            {
                m_SceneToLoad = WinSceneName;
                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay + DelayBeforeFadeToBlack;

                // play a sound on win
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = VictorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.PlayScheduled(AudioSettings.dspTime + DelayBeforeWinMessage);

                // create a game message
                //var message = Instantiate(WinGameMessagePrefab).GetComponent<DisplayMessage>();
                //if (message)
                //{
                //    message.delayBeforeShowing = delayBeforeWinMessage;
                //    message.GetComponent<Transform>().SetAsLastSibling();
                //}

                DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
                displayMessage.Message = WinGameMessage;
                displayMessage.DelayBeforeDisplay = DelayBeforeWinMessage;
                EventManager.Broadcast(displayMessage);
            }
            else // not win
            {
                m_SceneToLoad = LoseSceneName;
                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay;
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}