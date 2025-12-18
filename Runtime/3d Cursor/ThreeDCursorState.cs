using UnityEngine;

namespace Konfus.ThreeDCursor
{
    [CreateAssetMenu(fileName = "New Cursor State", menuName = "Konfus/New 3D Cursor State", order = 1)]
    public class ThreeDCursorState : ScriptableObject
    {
        [SerializeField, Tooltip("Size in meters.")] 
        private float size = 1;
        [SerializeField, Tooltip("This states cursor visual, think a 2d animation frame but 3d.")]
        private GameObject visual;

        public float Size => size;
        public GameObject Visual => visual;
    }
}