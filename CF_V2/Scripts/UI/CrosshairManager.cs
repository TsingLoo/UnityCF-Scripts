using DG.Tweening;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CrosshairManager : MonoBehaviour
    {
        public Transform WeaponScopePosition;

        public NormalCrosshair Crosshair;

        public Sprite DefaultCrosshair;
        public Sprite NullCrosshairSprite;
        public float CrosshairUpdateSharpness = 50f;

        #region Weapon scope
        [Header("Scope")]
        GameObject _aimScope;
        // show up
        public float scaleFactor = 1.25f;
        public float scaleTime = 0.02f;

        public float scaleBackTime = 0.075f;
        #endregion
        bool _isAiming = false;

        PlayerWeaponsManager m_WeaponsManager;
        WeaponController _currentWeapon;
        bool m_WasPointingAtEnemy;

        void Start()
        {
            // weapon manager
            m_WeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(m_WeaponsManager, this);

            m_WeaponsManager.OnSwitchedToWeapon += OnWeaponChanged;
            // OnWeaponChanged is not added in weapon manager's start
            OnWeaponChanged(m_WeaponsManager.GetCurrentWeapon());

            // change script order
            // https://docs.unity3d.com/Manual/class-MonoManager.html
        }

        void Update()
        {
            UpdateCrosshairPointingAtEnemy(false);
            m_WasPointingAtEnemy = m_WeaponsManager.IsPointingAtEnemy;

            UpdateAiming();
        }

        void OnWeaponChanged(WeaponController newWeapon)
        {
            if (newWeapon)
            {
                _currentWeapon = newWeapon;
                _currentWeapon.OnWeaponFire += OnWeaponFire;
            }

            if (newWeapon)
            {
                Crosshair.Enable();
            }
            else // no weapon
            {
                Crosshair.Disable();
            }

            InitWeaponScope(newWeapon);
                
            UpdateCrosshairPointingAtEnemy(true);
        }

        private void OnWeaponFire()
        {
            if(_isAiming)
            {
                if(_aimScope != null)
                {
                    PlayScopeRecoil();
                }
            }
        }

        private void PlayScopeRecoil()
        {
            var animId = "WeaponScope_Recoil";
            DOTween.Kill(animId);

            //markTransform.anchoredPosition = Vector2.zero;
            //markTransform.localScale = Vector3.zero;
            //markImage.color = Color.white;

            var scopeRecoil = _aimScope.GetComponent<WeaponScopeRecoil>();
            if (scopeRecoil != null)
            {
                scaleFactor = scopeRecoil.scaleFactor;
                scaleTime = scopeRecoil.scaleTime;

                scaleBackTime = scopeRecoil.scaleBackTime;
            }

            // scale up
            var markTransform = _aimScope.transform;
            markTransform.DOScale(Vector3.one * scaleFactor, scaleTime).SetEase(Ease.Linear).SetId(animId)
                .OnComplete(() =>
                {
                    // scale back
                    markTransform.DOScale(Vector3.one, scaleBackTime).SetEase(Ease.Linear).SetId(animId)
                    .OnComplete(() =>
                    {
                    });
                });
        }

        private void InitWeaponScope(WeaponController newWeapon)
        {
            // reset aiming
            StopAiming();
            if(_aimScope != null)
            {
                _aimScope.SelfDestroy();
            }

            // scope
            if (newWeapon != null 
                && newWeapon.WeaponData.HasAim 
                && newWeapon.aimScope != null)
            {
                _aimScope = Instantiate(newWeapon.aimScope,
                    WeaponScopePosition,
                    false);
                _aimScope.Hide();
            }
            else
            {
                _aimScope = null;
            }

        }

        private void UpdateAiming()
        {
            // todo mouse sense
            if (m_WeaponsManager.IsAiming && !_isAiming)
            {
                StartAiming();
            }
            // not aim
            else if(!m_WeaponsManager.IsAiming && _isAiming)
            {
                StopAiming();
            }
        }

        private void StartAiming()
        {
            _isAiming = true;

            Invoke(nameof(ShowScope), 0f); // todo aim time?
            Crosshair.Hide();
        }

        private void StopAiming()
        {
            _isAiming = false;

            HideScope();
            Crosshair.Show();
        }

        void ShowScope()
        {
            if (_aimScope != null)
            {
                _aimScope.Show();
                if (_currentWeapon)
                {
                    _currentWeapon.WeaponRoot.Hide();
                }
            }
        }

        void HideScope()
        {
            if (_aimScope != null)
            {
                _aimScope.Hide();
            }

            // weapon always show
            if (_currentWeapon)
            {
                _currentWeapon.WeaponRoot.Show();
            }
        }

        void UpdateCrosshairPointingAtEnemy(bool force)
        {
            // point at enemy
            if ((force || !m_WasPointingAtEnemy) 
                && m_WeaponsManager.IsPointingAtEnemy)
            {
                // color red
            }
            // none
            else if ((force || m_WasPointingAtEnemy) 
                && !m_WeaponsManager.IsPointingAtEnemy)
            {
                // color original
            }

            //CrosshairImage.color = Color.Lerp(CrosshairImage.color, m_CurrentCrosshair.CrosshairColor,
              //  Time.deltaTime * CrosshairUpdateSharpness);

        }

    }
}