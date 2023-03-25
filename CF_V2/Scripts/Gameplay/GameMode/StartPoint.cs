using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class StartPoint : MonoBehaviour
    {
        public ETeam Team;

        [Header("Debug")]
        public float drawRadius = 1.5f;
        public Color drawColor = Color.green;

        private void OnDrawGizmos()
        {
            Gizmos.color = drawColor;
            var rad = drawRadius;
            Gizmos.DrawWireSphere(transform.position, rad);
            Gizmos.DrawLine(transform.position,
                transform.position + transform.forward * rad);
        }
    }
}