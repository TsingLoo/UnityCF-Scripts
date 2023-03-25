namespace Unity.FPS.Game
{
    // game constants
    public class ButtonNames
    {
        // all the constant string used across the game
        public const string k_AxisNameVertical = "Vertical";
        public const string k_AxisNameHorizontal = "Horizontal";
        public const string k_MouseAxisNameVertical = "Mouse Y";
        public const string MouseX = "Mouse X";
        public const string k_AxisNameJoystickLookVertical = "Look Y";
        public const string LookX = "Look X";
        
        public const string k_ButtonNameAim = "Aim";
        public const string k_ButtonNameFire = "Fire";
        public const string k_ButtonNameHeavy = "HeavyAttack";
        public const string k_ButtonNameGamepadFire = "Gamepad Fire";
        public const string k_ButtonNameGamepadAim = "Gamepad Aim";

        public const string k_ButtonReload = "Reload";

        public const string Sprint = nameof(Sprint);
        public const string Walk = "Walk";
        public const string Jump = "Jump";
        public const string Roll = "Roll";
        public const string k_ButtonNameCrouch = "Crouch";

        public const string k_ButtonNameNextWeapon = "NextWeapon";
        public const string k_ButtonNameSwitchWeapon = "Mouse ScrollWheel";
        public const string k_ButtonNameGamepadSwitchWeapon = "Gamepad Switch";
        
        public const string k_ButtonNamePauseMenu = "Pause Menu";
        public const string k_ButtonNameSubmit = "Submit";
        public const string k_ButtonNameCancel = "Cancel";

        /// <summary>
        /// SwitchView, todo rename
        /// </summary>
        public const string SwitchCamera = nameof(SwitchCamera);
        public const string ShowInventory = nameof(ShowInventory);

        public const string ThrowWeapon = nameof(ThrowWeapon);
    }
}