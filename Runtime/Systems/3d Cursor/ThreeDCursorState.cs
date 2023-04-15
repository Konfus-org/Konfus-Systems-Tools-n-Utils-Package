using UnityEngine;

namespace Konfus.Systems.ThreeDCursor
{
    [CreateAssetMenu(fileName = "New Cursor State", menuName = "Konfus/New 3D Cursor State", order = 1)]
    public class ThreeDCursorState : ScriptableObject
    {
        [SerializeField, Tooltip("Size in meters.")] 
        private float size = 1;
        [SerializeField, Tooltip("Offset from system cursor relative to 3d cursor scale.")] 
        private Vector2 offset;
        [SerializeField, Tooltip("This states cursor visual, think a 2d animation frame but 3d.")]
        private GameObject visual;

        public float Size => size;
        public Vector2 Offset => offset;
        public GameObject Visual => visual;
    }
}