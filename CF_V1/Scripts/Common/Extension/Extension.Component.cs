using System.Runtime.CompilerServices;
using UnityEngine;

public static partial class Extension
{
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

    public static void SelfDestroy(this UnityEngine.Component input)
    {
        if (input != null)
        {

            UnityEngine.Object.Destroy(input.gameObject);
        }
    }

    public static void ResetTransform(this UnityEngine.Component input)
    {
        if (input != null)
        {
            input.transform.localPosition = Vector3.zero;
            input.transform.localRotation = Quaternion.identity;
            input.transform.localScale = Vector3.one;
        }
    }

    public static void Setlayer
        (this Component input, EditorLayer newLayer)
    {
        input.Setlayer(newLayer.GetValue());
    }

    public static void Setlayer
        (this Component input, int newLayer)
    {
        Helpers.RecursiveLayerChange(input.transform, newLayer);

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
}
