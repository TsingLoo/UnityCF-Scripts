using System;
using System.Collections;
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

    }
}
