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
            // select buttons by arraw key
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetButtonDown(ButtonNames.Submit)
                    || Input.GetAxisRaw(ButtonNames.Horizontal) != 0
                    || Input.GetAxisRaw(ButtonNames.Vertical) != 0)
                {
                    EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
                }
            }
        }
    }
}