

using UnityEngine;

public class LayerHelper
{
    public static int AddLayer(LayerMask layerMask, int layer)
    {
        return layerMask | (1 << layer);
    }

    public static int RemoveLayer(LayerMask layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }


    public static LayerMask GetAllLayer()
    {
        LayerMask allLayer = -1;
        return allLayer;

        //if (Physics.Raycast
        //    (ray, out hit, maxRaycastDistance, allLayer.value,//~(1 << 9),
    }

}
