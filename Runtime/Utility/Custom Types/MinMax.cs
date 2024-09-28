namespace Konfus.Utility.Custom_Types
{
    [System.Serializable]
    public struct MinMaxFloat
    {
        public MinMaxFloat(float minimum, float maximum)
        {
            min = minimum;
            max = maximum;
        }

        [SerializeField]
        private float min;
        [SerializeField]
        private float max;

        public float Min => min;
        public float Max => max;
    }
}