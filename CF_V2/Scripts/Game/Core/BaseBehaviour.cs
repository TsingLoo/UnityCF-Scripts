using System;
using System.Collections;
using System.Drawing;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class BaseBehaviour : MonoBehaviour
    {
        #region Action
        // TPS Shooter Military Style 8.0

        /// <summary>
        /// Invoke action after delay.
        /// Returns Coroutine, so you can stop it if it is needed.
        /// </summary>
        public Coroutine DelayAction(float delay,
            Action action,
            bool timeIndependent = true)
        {
            return StartCoroutine(DelayCoroutine(delay, action, timeIndependent));
        }

        private IEnumerator DelayCoroutine(float delay, Action action, bool timeIndependent = true)
        {
            float time = delay;
            while (time >= 0)
            {
                time -= timeIndependent ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            action.Invoke();
        }
        #endregion

        public void PlaySoundMix(AudioClip clip,
            AudioUtility.AudioGroups audioGroup,
            float spatialBlend = 0f) // 2D / 3D
        {
            AudioUtility.CreateSFX(clip,
                transform.position,
                audioGroup,
                spatialBlend,
                1f,
                1f);
        }

        //
    }
}
