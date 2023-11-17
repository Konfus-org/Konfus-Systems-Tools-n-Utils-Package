namespace Konfus.Systems.Grids
{
    public class StandardGrid : GridBase
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