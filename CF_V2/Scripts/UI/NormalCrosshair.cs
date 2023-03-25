using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    // todo: late update deltaTime 0.02, update 0.1
    public class NormalCrosshair : MonoBehaviour
    {
        public Color MainColor = Color.white;
        public float lineThickness = 3f;
        public float lineLength = 10f;
        public float mainBorderSize = 50f;

        public Image[] crosshairLines;
        public Image[] crosshairLineBorders;

        public RectTransform mainRectTransform;
        public CanvasGroup lineCanvasGroup;
        public CanvasGroup dotCanvasGroup;

        [Header("Settings")]

        [SerializeField]
        private Vector2 totalScaleFactorRange = new Vector2(0.7f, 2.5f);
        [SerializeField]
        private Vector2 moveScaleFactorRange = new Vector2(-0.5f, 0.5f);

        [SerializeField]
        private float jumpScaleAdd = 50.0f;

        [SerializeField]
        private float crouchScaleAdd = -15.0f;

        [SerializeField]
        private float walkScaleAdd = 25.0f;
        [SerializeField]
        private float runScaleAdd = 35.0f;
        [SerializeField]
        private float sprintScaleAdd = 45.0f;

        // private fields
        PlayerWeaponsManager _weaponsManager;
        PlayerController _playerController;

        protected void Awake()
        {
            // main size
            mainRectTransform.sizeDelta = new Vector2(mainBorderSize, mainBorderSize);

            // line size
            foreach (var item in crosshairLines)
            {
                item.color = MainColor;

                if (item.name.Contains("Horizontal"))
                {
                    item.rectTransform.sizeDelta = 
                        new Vector2(lineLength, lineThickness);
                }
                else // vertical
                {
                    item.rectTransform.sizeDelta =
                        new Vector2(lineThickness, lineLength);
                }
            }

            // todo, line shadow
            foreach (var item in crosshairLineBorders)
            {
                //item.Hide();
            }

            // cache
            _originSizeDelta = mainRectTransform.sizeDelta;
        }

        private void Start()
        {
            // weapon manager
            _weaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(_weaponsManager, this);
            _weaponsManager.OnSwitchedToWeapon += ResetRecoil;

            _playerController = FindObjectOfType<PlayerController>();
        }

        #region refer: Crosshair recoil smooth factor
        /// <summary>
        /// moving smooth factor?
        /// exponential moving average
        /// </summary>
        private const float exponentialAlpha = 0.8f;
        private float recoilApplyPrev; // recoilApplyPrevious
        private Vector2 _originSizeDelta;
        #endregion

        float _scaleFactor = 1f;

        private void Update()
        {
            if(_weaponsManager.GetCurrentWeapon() 
                && _weaponsManager.GetCurrentWeapon().HasRecoil())
            {
                UpdateCrosshair();

                // decrease (weapon recoil decreased in weapons manager)
                if (!_playerController.IsWalking
                    && !_playerController.IsRunning
                    && !_playerController.IsSprinting
                    && _scaleFactor > 1)
                {
                    _scaleFactor = Mathf.Max(1, _scaleFactor -
                        (Time.deltaTime * 3));
                }
            }

        }

        private void ResetRecoil(WeaponController weaponController) 
        { 
            _scaleFactor = 1f;
            mainRectTransform.sizeDelta = _originSizeDelta;
        }

        private void UpdateCrosshair()
        {
            if (lineCanvasGroup == null
                || dotCanvasGroup == null
                || mainRectTransform == null)
            {
                return;
            }

            _scaleFactor = 1;
            // weapon
            _scaleFactor += _weaponsManager.accumulatedCrosshairRecoil;
            // move
            _scaleFactor += GetMoveScaleFactor();

            // clamp
            //scaleFactor = Mathf.Clamp(scaleFactor, totalScaleFactorRange.x, totalScaleFactorRange.y);

            // decrease in fixed update

            // Apply:
            // not in use
            //float recoilApply = (scaleFactor * exponentialAlpha)
            //    + (recoilApplyPrev * (1 - exponentialAlpha));
            //recoilApplyPrev = recoilApply;
            var scaleFactorApply = Mathf.Lerp(recoilApplyPrev, _scaleFactor, Time.deltaTime);
            recoilApplyPrev = _scaleFactor;
            mainRectTransform.sizeDelta = scaleFactorApply * _originSizeDelta;
        }

        private float GetMoveScaleFactor()
        {
            float scaleFactor = 0f;

            #region Factors
            // jump, todo use simple jump?
            float fallingVelocity = jumpScaleAdd
                * (_playerController.CharacterVelocity.y >= 0 ? Mathf.Clamp01(Mathf.Abs(_playerController.CharacterVelocity.y)) : 1);

            scaleFactor += _playerController.IsGrounded ?
                0f
                : fallingVelocity;

            // crouch
            if (_playerController.IsCrouching)
            {
                scaleFactor += crouchScaleAdd;
            }

            // move
            if (_playerController.IsWalking)
            {
                scaleFactor += walkScaleAdd;
            }
            else if (_playerController.IsRunning)
            {
                scaleFactor += runScaleAdd;
            }
            else if(_playerController.IsSprinting)
            {
                scaleFactor += sprintScaleAdd;
            }
            #endregion

            scaleFactor /= 100;
            scaleFactor = Mathf.Clamp(scaleFactor, moveScaleFactorRange.x, moveScaleFactorRange.y);
            return scaleFactor;
        }
    }
}