namespace Blace.Server
{
    public class TileNotFoundException : Exception
    {
        public TileNotFoundException() : base("The requested Tile was not found.")
        {
        }
    }
}
