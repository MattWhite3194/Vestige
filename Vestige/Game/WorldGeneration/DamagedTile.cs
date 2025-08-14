namespace Vestige.Game.WorldGeneration
{
    public class DamagedTile
    {
        /// <summary>
        /// The health left on the tile
        /// </summary>
        public int Health;
        /// <summary>
        /// The total health in the tile
        /// </summary>
        public int TotalTileHealth;
        /// <summary>
        /// The time left before the tile is removed from the dictionary and any damage done is reset
        /// </summary>
        public double TimeLeft;
        public int X, Y;
        public ushort TileID;

        //TODO: use this for drawing order of these
        public int Layer;
        public DamagedTile(int X, int Y, ushort tileID, int health, int timeLeft)
        {
            TotalTileHealth = health;
            Health = health;
            TimeLeft = timeLeft;
            this.X = X;
            this.Y = Y;
        }
    }
}
