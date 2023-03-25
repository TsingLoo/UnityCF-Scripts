using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetButtonDown(ButtonNames.k_ButtonNameSubmit)
                    || Input.GetAxisRaw(ButtonNames.k_AxisNameHorizontal) != 0
                    || Input.GetAxisRaw(ButtonNames.k_AxisNameVertical) != 0)
                {
                    EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
                }
            }
        }
    }
}