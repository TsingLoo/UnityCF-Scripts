using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.FPS.Game
{
    public static partial class Extension
    {
        public static void Show(this Component input, bool show)
        {
            if (input != null)
            {
                input.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// call OnEnable
        /// </summary>
        /// <param name="input"></param>
        public static void Show(this Component input)
        {
            if (input != null)
            {
                input.gameObject.SetActive(true);
            }
        }

        public static void Hide(this UnityEngine.Component input)
        {
            if (input != null)
            {
                input.gameObject.SetActive(false);
            }
        }

        public static void SelfDestroy(this UnityEngine.Component input,
            float duration = 0)
        {
            if (input != null)
            {
                UnityEngine.Object.Destroy(input.gameObject, duration);
            }
        }

        public static void ResetLocalTransform(this Component input)
        {
            if (input != null)
            {
                input.transform.localPosition = Vector3.zero;
                input.transform.localRotation = Quaternion.identity;
                input.transform.localScale = Vector3.one;
            }
        }

        public static void Setlayer
            (this Component input, int newLayer)
        {
            RecursiveLayerChange(input.transform, newLayer);

            // only 1 child set
            //if (input != null)
            //{
            //    input.gameObject.layer = newLayer;

            //    foreach (Transform child in input.transform)
            //    {
            //        child.gameObject.layer = newLayer;
            //    }
            //}
        }

        public static void RecursiveLayerChange(Transform parent, int layer)
        {
            parent.gameObject.layer = layer;
            foreach (Transform childTransform in parent)
            {
                RecursiveLayerChange(childTransform, layer);
            }
        }
    }
}