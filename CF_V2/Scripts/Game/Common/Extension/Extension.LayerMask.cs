

using UnityEngine;

public partial class Extension
{
    public static bool Contains(this LayerMask input, int layer)
    {
        return input == (input | (1 << layer));
    }

    public static int RemoveLayer(this LayerMask layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }
}
