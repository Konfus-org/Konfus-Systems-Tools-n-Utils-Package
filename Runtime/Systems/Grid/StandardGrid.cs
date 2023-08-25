namespace Konfus.Systems.Grid
{
    public class StandardGrid : Grid
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