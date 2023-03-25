using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Tooltip("Sensitivity multiplier for moving the camera around")]
        public float LookSensitivity = 1f;

        [Tooltip("Additional sensitivity multiplier for WebGL")]
        public float WebglLookSensitivityMultiplier = 0.25f;

        [Tooltip("Limit to consider an input when using a trigger on a controller")]
        public float TriggerAxisThreshold = 0.4f;

        [Tooltip("Used to flip the vertical input axis")]
        public bool InvertYAxis = false;

        [Tooltip("Used to flip the horizontal input axis")]
        public bool InvertXAxis = false;

        GameFlowManager m_GameFlowManager;
        PlayerController m_PlayerController;
        bool m_FireInputWasHeld;

        void Start()
        {
            m_PlayerController = GetComponent<PlayerController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerController, PlayerInputHandler>(
                m_PlayerController, this, gameObject);
            m_GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);

            ShowCursor(false);
        }

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
        }

        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked
                && !m_GameFlowManager.GameIsEnding;
        }

        public bool GetInputDown(string buttonName)
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown(buttonName);
            }

            return false;
        }

        public bool GetInputHeld(string buttonName)
        {
            if (CanProcessInput())
            {
                return Input.GetButton(buttonName);
            }

            return false;
        }

        #region Inputs
        #region WASD
        public float GetMoveInputX()
        {
            if (CanProcessInput())
            {
                return Input.GetAxisRaw(ButtonNames.k_AxisNameHorizontal);
            }
            return 0;
        }

        public float GetMoveInputY()
        {
            if (CanProcessInput())
            {
                return Input.GetAxisRaw(ButtonNames.k_AxisNameVertical);
            }
            return 0;
        }

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                Vector3 move = new Vector3(
                    Input.GetAxisRaw(ButtonNames.k_AxisNameHorizontal), 0f,
                    Input.GetAxisRaw(ButtonNames.k_AxisNameVertical));

                // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }
        #endregion

        #region Mouse

        public float GetMouseX()
        {
            return GetMouseOrStickLookAxis(ButtonNames.MouseX,
                ButtonNames.LookX);
        }

        public float GetMouseY()
        {
            return GetMouseOrStickLookAxis(ButtonNames.k_MouseAxisNameVertical,
                ButtonNames.k_AxisNameJoystickLookVertical);
        }
        #endregion

        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown(ButtonNames.Jump);
            }

            return false;
        }

        public bool GetJumpInputHeld()
        {
            if (CanProcessInput())
            {
                return Input.GetButton(ButtonNames.Jump);
            }

            return false;
        }

        public bool GetFireInputDown()
        {
            return GetFireInputHeld() && !m_FireInputWasHeld;
        }

        public bool GetFireInputReleased()
        {
            return !GetFireInputHeld() && m_FireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                bool isGamepad = Input.GetAxis(ButtonNames.k_ButtonNameGamepadFire) != 0f;
                if (isGamepad)
                {
                    return Input.GetAxis(ButtonNames.k_ButtonNameGamepadFire) >= TriggerAxisThreshold;
                }
                else
                {
                    return Input.GetButton(ButtonNames.k_ButtonNameFire);
                }
            }

            return false;
        }

        public bool GetHeavyInputDown()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown
                    (ButtonNames.k_ButtonNameHeavy);
            }

            return false;
        }

        #region Aim
        // add aim down
        public bool GetAimInputDown()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown(ButtonNames.k_ButtonNameAim);
            }

            return false;
        }


        public bool GetAimInputHeld()
        {
            if (CanProcessInput())
            {
                bool isGamepad = Input.GetAxis(ButtonNames.k_ButtonNameGamepadAim) != 0f;
                bool i = isGamepad
                    ? (Input.GetAxis(ButtonNames.k_ButtonNameGamepadAim) > 0f)
                    : Input.GetButton(ButtonNames.k_ButtonNameAim);
                return i;
            }

            return false;
        }
        #endregion


        public bool GetCrouchInputDown()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown(ButtonNames.k_ButtonNameCrouch);
            }

            return false;
        }

        public bool GetCrouchInputReleased()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonUp(ButtonNames.k_ButtonNameCrouch);
            }

            return false;
        }

        public bool GetReloadButtonDown()
        {
            if (CanProcessInput())
            {
                return Input.GetButtonDown(ButtonNames.k_ButtonReload);
            }

            return false;
        }

        public int GetSwitchWeaponInput()
        {
            if (CanProcessInput())
            {
                bool isGamepad = Input.GetAxis(ButtonNames.k_ButtonNameGamepadSwitchWeapon) != 0f;
                string axisName = isGamepad
                    ? ButtonNames.k_ButtonNameGamepadSwitchWeapon
                    : ButtonNames.k_ButtonNameSwitchWeapon;

                if (Input.GetAxis(axisName) > 0f)
                    return -1;
                else if (Input.GetAxis(axisName) < 0f)
                    return 1;
                // todo change
                else if (Input.GetAxis(ButtonNames.k_ButtonNameNextWeapon) > 0f)
                    return -1;
                else if (Input.GetAxis(ButtonNames.k_ButtonNameNextWeapon) < 0f)
                    return 1;
            }

            return 0;
        }

        public int GetSelectWeaponInput()
        {
            if (CanProcessInput())
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    return 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    return 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    return 3;
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    return 4;
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                    return 5;
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                    return 6;
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                    return 7;
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                    return 8;
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                    return 9;
                else
                    return 0;
            }

            return 0;
        }

        float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
        {
            if (CanProcessInput())
            {
                // Check if this look input is coming from the mouse
                bool isGamepad = Input.GetAxis(stickInputName) != 0f;
                float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);

                // handle inverting vertical input
                if (InvertYAxis)
                    i *= -1f;

                // apply sensitivity multiplier
                i *= LookSensitivity;

                if (isGamepad)
                {
                    // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
                    i *= Time.deltaTime;
                }
                else
                {
                    // reduce mouse input amount to be equivalent to stick movement
                    i *= 0.01f;
#if UNITY_WEBGL
                    // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                    i *= WebglLookSensitivityMultiplier;
#endif
                }

                return i;
            }

            return 0f;
        }
        #endregion

        public void ShowCursor(bool show)
        {
            if(show) 
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

    }
}