using System;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class InGameMenuManager : MonoBehaviour
    {
        [Tooltip("Root GameObject of the menu used to toggle its activation")]
        public GameObject MenuRoot;

        [Range(0.001f, 1f)]
        public float VolumeWhenMenuOpen = 0.5f;

        public Slider LookSensitivitySlider;

        public Slider MasterVolumeSlider;
        public TextMeshProUGUI MasterVolumeValue;

        public Toggle ShadowsToggle;

        public Toggle InvincibilityToggle;

        public Toggle FramerateToggle;

        // todo 
        public GameObject ControlImage;

        PlayerController _playerCon;
        PlayerInputHandler _inputHandler;
        Health m_PlayerHealth;
        FramerateCounter m_FramerateCounter;

        void Start()
        {
            Init();

            MenuRoot.SetActive(false);

            LookSensitivitySlider.value = _inputHandler.LookSensitivity;
            LookSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            // volume
            MasterVolumeSlider.value = PlayerPrefs.GetFloat(GlobalSettings.MasterVolume);
            OnMasterVolumeChanged(MasterVolumeSlider.value);
            MasterVolumeValue.text = GetVolumeText(MasterVolumeSlider.value);
            MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            ShadowsToggle.isOn = QualitySettings.shadows != ShadowQuality.Disable;
            ShadowsToggle.onValueChanged.AddListener(OnShadowsChanged);

            InvincibilityToggle.isOn = m_PlayerHealth.Invincible;
            InvincibilityToggle.onValueChanged.AddListener(OnInvincibilityChanged);

            FramerateToggle.isOn = m_FramerateCounter.UIText.gameObject.activeSelf;
            FramerateToggle.onValueChanged.AddListener(OnFramerateCounterChanged);
        }

        private void Init()
        {
            _playerCon = FindAnyObjectByType<PlayerController>();

            _inputHandler = FindObjectOfType<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerInputHandler, InGameMenuManager>(_inputHandler,
                this);

            m_PlayerHealth = _inputHandler.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, InGameMenuManager>(m_PlayerHealth, this, gameObject);

            m_FramerateCounter = FindObjectOfType<FramerateCounter>();
            DebugUtility.HandleErrorIfNullFindObject<FramerateCounter, InGameMenuManager>(m_FramerateCounter, this);

        }

        void Update()
        {
            #region Cursor

            // Lock cursor when clicking outside of menu
            if (!MenuRoot.activeSelf
                && !_playerCon.IsInventoryShowing()
                && Input.GetMouseButtonDown(0))
            {
                _inputHandler.ShowCursor(false);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _inputHandler.ShowCursor(true);
            }
            #endregion

            // Inventory
            if (Input.GetButtonDown(ButtonNames.ShowInventory))
            {
                ToggleInventory();

                _inputHandler.ShowCursor(_playerCon.IsInventoryShowing());
            }


            // Pause
            if (Input.GetButtonDown(ButtonNames.k_ButtonNamePauseMenu)
                || (MenuRoot.activeSelf && Input.GetButtonDown(ButtonNames.k_ButtonNameCancel)))
            {
                if (ControlImage.activeSelf)
                {
                    ControlImage.SetActive(false);
                    return;
                }

                SetPauseMenuActivation(!MenuRoot.activeSelf);
            }

            // todo ?
            if (Input.GetAxisRaw(ButtonNames.k_AxisNameVertical) != 0)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    LookSensitivitySlider.Select();
                }
            }
        }


        private string GetVolumeText(float value)
        {
            return ((int)(value * 100)).ToString() + "%";
        }

        private void ToggleInventory()
        {
            _playerCon.ToggleInventory();
        }

        public void ClosePauseMenu()
        {
            SetPauseMenuActivation(false);
        }

        void SetPauseMenuActivation(bool active)
        {
            MenuRoot.SetActive(active);

            if (MenuRoot.activeSelf)
            {
                _inputHandler.ShowCursor(true);

                Time.timeScale = 0f;
                AudioUtility.SetMasterVolume(VolumeWhenMenuOpen);

                EventSystem.current.SetSelectedGameObject(null);
            }
            else // in game
            {
                _inputHandler.ShowCursor(false);

                Time.timeScale = 1f;
                AudioUtility.SetMasterVolume(1);
            }
        }

        void OnMouseSensitivityChanged(float newValue)
        {
            _inputHandler.LookSensitivity = newValue;
        }

        void OnMasterVolumeChanged(float newValue)
        {
            AudioListener.volume = newValue;
            MasterVolumeValue.text = GetVolumeText(newValue);

            PlayerPrefs.SetFloat(GlobalSettings.MasterVolume, newValue);
        }

        void OnShadowsChanged(bool newValue)
        {
            QualitySettings.shadows = newValue ? 
                ShadowQuality.All 
                : ShadowQuality.Disable;
        }

        void OnInvincibilityChanged(bool newValue)
        {
            m_PlayerHealth.Invincible = newValue;
        }

        void OnFramerateCounterChanged(bool newValue)
        {
            m_FramerateCounter.UIText.gameObject.SetActive(newValue);
        }

        // todo del
        public void OnShowControlButtonClicked(bool show)
        {
            ControlImage.SetActive(show);
        }

        // End
    }
}