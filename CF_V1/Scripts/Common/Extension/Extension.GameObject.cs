using System.Runtime.CompilerServices;
using UnityEngine;

public static partial class Extension
{
    public static void Show(this GameObject input)
    {
        if (input != null)
        {
            input.gameObject.SetActive(true);
        }
    }

    public static void Hide(this GameObject input)
    {
        if (input != null)
        {
            input.gameObject.SetActive(false);
        }
    }

}
