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
            newMarker.Init(worldElement: this);

            _minimap.RegisterElement(transform, newMarker);
        }

        void OnDestroy()
        {
            if(_minimap != null)
            {
                _minimap.UnregisterElement(transform);
            }
        }
    }
}