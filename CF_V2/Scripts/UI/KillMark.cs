using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.FPS.Game;
using DG.Tweening;

namespace Unity.FPS.UI
{
    /// <summary>
    /// Dotween
    /// https://blog.csdn.net/zcaixzy5211314/article/details/84886663
    /// </summary>
    public class KillMark : MonoBehaviour
    {
        public static KillMark Instance { get; private set; }

        public RectTransform _mainTransform;
        public Image _mainImage;
        public RectTransform _effectTransform;
        public Image _effectImage;

        [Header("Marks")]

        public float stayTime = 0.3f;
        public float killCountResetTime = 3f;

        public float fadeTimeMain = 0.5f;//2f;
        public float fadeTimeEffect = 0.5f;//1f;
        public Sprite headShotMark;
        public Sprite knifeKillMark;
        public Sprite granadeKillMark;
        public Sprite killWallMark;
        public Sprite markBomb;
        public Sprite[] multiKillMarks;
        public Sprite[] multiKillMarkEffects;

        [Header("Kill Voice")]
        public float volume;
        public AudioClip killComboVoice;
        public AudioClip headShotVoice;
        public AudioClip knifeKillVoice;
        public AudioClip granadeKillVoice;
        public AudioClip[] killVoices;
        public AudioSource audioSource;

        AudioClip killVoiceToPlay;

        int _killCount = 1;
        string markId = "KillMarkAnim_Id";
        string markEffectId = "KillMarkEffectAnim_Id";

        // show up
        float scaleFactor;
        float scaleTime;

        float scaleBackTime;
        float fadeDuration;
        float shakeDuration;
        float shakeStrength;
        int shakeVibration;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            EventManager.AddListener<KillMarkEvent>(OnKillMarkEvent);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<KillMarkEvent>(OnKillMarkEvent);
        }

        void OnKillMarkEvent(KillMarkEvent req)
        {
            this.ShowMark(req.DamageType);
        }


        // Update is called once per frame
        void Update()
        {
        }
        private void FixedUpdate()
        {
            #region Timer helper
            if (TimerOut())
            {
                _killCount = 1;
            }
            else
            {
                UpdateTimer();
            }
            #endregion

        }


        protected void ShowMark(EDamageType killType = EDamageType.Rifle)
        {
            #region Init
            // head shot
            if (_killCount == 1
                && killType != EDamageType.Rifle)
            {
                switch (killType)
                {
                    case EDamageType.Rifle:
                        break;
                    case EDamageType.HeadShot:
                        {
                            // mark
                            _mainImage.sprite = headShotMark;

                            // sound
                            killVoiceToPlay = headShotVoice;
                        }
                        break;
                    case EDamageType.Knife:
                        {
                            // mark
                            _mainImage.sprite = knifeKillMark;

                            // sound
                            killVoiceToPlay = knifeKillVoice;
                        }
                        break;
                    case EDamageType.Granade:
                        {
                            // mark
                            _mainImage.sprite = granadeKillMark;

                            // sound
                            killVoiceToPlay = granadeKillVoice;
                        }
                        break;
                    case EDamageType.Wall:
                        {
                            // mark
                            _mainImage.sprite = killWallMark;

                            // sound
                            killVoiceToPlay = killComboVoice;
                        }
                        break;
                    case EDamageType.Bomb:
                        {
                            // mark
                            _mainImage.sprite = markBomb;

                            // sound
                            killVoiceToPlay = killComboVoice;
                        }
                        break;
                    default:
                        break;
                }
            }
            else // normal kill
            {
                // mark
                if (_killCount <= multiKillMarks.Length)
                {
                    _mainImage.sprite = multiKillMarks[_killCount - 1];

                    if (_killCount > 1)
                    {
                        _effectImage.sprite =
                            multiKillMarkEffects[_killCount - 2];
                    }
                }
                else
                {
                    _effectImage.sprite =
                            multiKillMarkEffects.LastOrDefault();
                }

                // sound
                if (_killCount <= killVoices.Length)
                {
                    killVoiceToPlay = killVoices[_killCount - 1];
                }
                else
                {
                    killVoiceToPlay = killVoices.LastOrDefault();
                }
            }
            #endregion

            PlaySound();

            #region Play Mark

            // main
            scaleFactor = 1f;
            scaleTime = 0.05f;
            //scaleBackTime = 0.15f;

            shakeDuration = 0.5f;
            shakeStrength = 5;
            shakeVibration = 20;

            fadeDuration = fadeTimeMain;
            PlayMarkAnim(markId, _mainTransform, _mainImage);

            // effect
            if (_killCount > 1)
            {
                scaleFactor = 1.2f;
                scaleTime = 0.01f;
                scaleBackTime = 0.2f;

                // no shake
                //shakeDuration = 0.5f;
                //shakeStrength = 5;
                //shakeVibration = 20;

                fadeDuration = fadeTimeEffect;
                PlayEffectAnim(markEffectId, _effectTransform, _effectImage);
            }
            #endregion

            _killCount++;
            _waitTimer = killCountResetTime;
        }

        /// <summary>
        /// PlayMarkSound
        /// </summary>
        private void PlaySound()
        {
            if (volume > 0)
            {
                audioSource.PlayOneShot(killComboVoice, volume);
                if (killVoiceToPlay != killComboVoice)
                {
                    audioSource.PlayOneShot(killVoiceToPlay, volume);
                }
            }
        }

        private void PlayMarkAnim(string markId,
            RectTransform markTransform,
            Image markImage)
        {
            DOTween.Kill(markId);

            markTransform.anchoredPosition = Vector2.zero;
            markTransform.localScale = Vector3.zero;
            markImage.color = Color.white;

            // scale up
            markTransform.DOScale(Vector3.one * scaleFactor, scaleTime).SetEase(Ease.Linear).SetId(markId)
                .OnComplete(() =>
                {
                    // shake
                    markTransform.DOShakePosition
                    (shakeDuration, shakeStrength, shakeVibration)
                    .SetId(markId)
                        .OnComplete(() =>
                        {
                            var timeCount = 0f;
                            DOTween.To(() => timeCount, a => timeCount = a, 1, stayTime)
                            .OnComplete(() =>
                            {
                                // fade
                                DOTween.ToAlpha(() => markImage.color, x => markImage.color = x, 0, fadeDuration);
                                
                                // todo change
                                //markImage.DOColor(new Color(1, 1, 1, 0), "", fadeDuration).SetEase(Ease.Linear).SetId(markId);

                                #region refer
                                //var toDuration = fadeDuration;
                                //var temp = 1f; // to 2
                                //DOTween.To(() => temp, x => temp = x, endValue: 2,
                                //    duration: toDuration)
                                //    .OnComplete(() => { markTransform.Hide(); });
                                #endregion
                            });
                        });


                    //// scale back
                    //markTransform.DOScale(Vector3.one, scaleBackTime).SetEase(Ease.Linear).SetId(markId)
                    //.OnComplete(() =>
                    //{

                    //});
                });

            #region refer
            //// scale up
            //markTransform.DOScale(Vector3.one * scaleFactor, 0.01f).SetEase(Ease.Linear).SetId(markId)
            //    .OnComplete(() =>
            //    {
            //        // scale back
            //        markTransform.DOScale(Vector3.one, scaleBackTime).SetEase(Ease.Linear).SetId(markId)
            //        .OnComplete(() =>
            //        {
            //            // shake
            //            markTransform.DOShakeAnchorPos
            //            (shakeDuration, shakeStrength, shakeVibration).SetId(markId)
            //                .OnComplete(() =>
            //                {
            //                    // fade
            //                    markImage.DOColor(new Color(1, 1, 1, 0), fadeDuration).SetEase(Ease.Linear).SetId(markId);
            //                });
            //        });
            //    });
            #endregion
        }

        private void PlayEffectAnim(string markId,
            RectTransform markTransform,
            Image markImage)
        {
            DOTween.Kill(markId);

            markTransform.anchoredPosition = Vector2.zero;
            markTransform.localScale = Vector3.zero;
            markImage.color = Color.white;

            // scale up
            markTransform.DOScale(Vector3.one * scaleFactor, scaleTime).SetEase(Ease.Linear).SetId(markId)
                .OnComplete(() =>
                {
                    // scale back
                    markTransform.DOScale(Vector3.one, scaleBackTime).SetEase(Ease.Linear).SetId(markId)
                    .OnComplete(() =>
                    {
                        // fade
                        DOTween.ToAlpha(() => markImage.color, x => markImage.color = x, 0, fadeDuration);

                        // todo change
                        //markImage.DOColor(new Color(1, 1, 1, 0), fadeDuration).SetEase(Ease.Linear).SetId(markId);
                    });

                    //// shake
                    //markTransform.DOShakePosition
                    //(shakeDuration, shakeStrength, shakeVibration)
                    //.SetId(markId)
                    //    .OnComplete(() =>
                    //    {
                    //    });


                });

        }

        #region Timer Helper

        float _waitTimer = -1.0f;

        private bool TimerNotOut()
        {
            return _waitTimer > 0;
        }
        private bool TimerOut()
        {
            return _waitTimer <= 0;
        }
        private void UpdateTimer()
        {
            _waitTimer -= Time.deltaTime;
        }
        #endregion


        // end
    }
}