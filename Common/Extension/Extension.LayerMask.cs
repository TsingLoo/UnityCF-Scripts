

using UnityEngine;

public partial class Extension
{
    public static bool Contains(this LayerMask input, int layer)
    {
        return input == (input | (1 << layer));
    }
}
