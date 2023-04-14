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

        public readonly float min;
        public readonly float max;
    }
}