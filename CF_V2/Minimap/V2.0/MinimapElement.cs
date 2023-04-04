using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    /// <summary>
    /// World element
    /// </summary>
    public class MinimapElement : MonoBehaviour
    {
        public MinimapMarker MarkerPrefab;

        Minimap _minimap;

        void Start()
        {
            _minimap = FindObjectOfType<Minimap>();

            var newMarker = Instantiate(MarkerPrefab);
            newMarker.Init(this);

            _minimap.RegisterElement(transform, newMarker);
        }

        void OnDestroy()
        {
            _minimap.UnregisterElement(transform);
        }
    }
}