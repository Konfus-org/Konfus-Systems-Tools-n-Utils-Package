namespace Konfus.Systems.Grids
{
    public class StandardGrid : Grid
    {
        private void Start()
        {
            Generate();
        }
        
        public override void Generate()
        {
            Generate((pos) => new Node(this, pos));
        }
    }
}