using UnityEngine;

namespace _TheGame._Scripts.Helpers
{
    public class GizmoSphere : MonoBehaviour
    {
        public Color gizmoColor = Color.yellow;
        public float radius = 1f;
        public bool drawWhenSelected = false;

        private void OnDrawGizmos()
        {
            if (!drawWhenSelected)
            {
                DrawSphere();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (drawWhenSelected)
            {
                DrawSphere();
            }
        }

        private void DrawSphere()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}