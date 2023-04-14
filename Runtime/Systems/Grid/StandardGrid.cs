namespace Konfus.Systems.Grid
{
    public class StandardGrid : GridBase
    {
        private void Start()
        {
            Generate();
        }
        
        protected override void Generate()
        {
            Generate((pos) => new Node(this, pos));
        }
    }
}