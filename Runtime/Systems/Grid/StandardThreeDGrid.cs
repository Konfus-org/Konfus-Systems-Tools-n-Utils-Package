namespace Konfus.Systems.Grid
{
    public class StandardThreeDGrid : ThreeDGrid
    {
        private void Start()
        {
            Generate();
        }
        
        protected override void Generate()
        {
            Generate((pos) => new ThreeDNode(this, pos));
        }
    }
}