using System;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    [InitializeOnLoad]
    static class FullscreenShortcut
    {
        static FullscreenShortcut()
        {
            EditorApplication.update += Update;
        }

        static void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                // todo try catch not working
                if (TryToggleMaximize())
                {
                    EditorWindow.focusedWindow.maximized = !EditorWindow.focusedWindow.maximized;
                }

                if (TryExitPlayMode())
                {
                    EditorApplication.ExitPlaymode();
                }
            }
#endif
        }

        private static bool TryToggleMaximize()
        {
            return Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.Return);
        }

        private static bool TryExitPlayMode()
        {
            return Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.Alpha4)
                ||
                Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.Alpha1);

            //return Input.GetKey(KeyCode.LeftAlt)
            //    && Input.GetKey(KeyCode.D);

        }

    }
}