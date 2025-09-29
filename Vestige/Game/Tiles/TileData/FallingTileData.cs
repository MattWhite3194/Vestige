using Microsoft.Xna.Framework;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class FallingTileData : DefaultTileData
    {
        private int _projectileID;
        public FallingTileData(int tileID, string name, TileProperty properties, Color mapColor, int projectileID, int itemID = -1, int wallID = -1, int health = 0, ushort[] tileMerges = null) : base(tileID, name, properties, mapColor, itemID, wallID, health, tileMerges)
        {
            this._projectileID = projectileID;
        }

        //TODO: Need to fix sand creating item drop when removed
        public override int VerifyTile(WorldGen world, int x, int y)
        {
            bool top = TileDatabase.TileHasProperties(world.GetTileID(x, y - 1), TileProperty.Solid | TileProperty.Platform);
            bool right = TileDatabase.TileHasProperties(world.GetTileID(x + 1, y), TileProperty.Solid | TileProperty.Platform);
            bool bottom = TileDatabase.TileHasProperties(world.GetTileID(x, y + 1), TileProperty.Solid | TileProperty.Platform);
            bool left = TileDatabase.TileHasProperties(world.GetTileID(x - 1, y), TileProperty.Solid | TileProperty.Platform);
            bool wall = world.GetWallID(x, y) != 0;
            
            //if the tile is already placed and being updated, always return valid so the tile won't be removed and create an item drop when it falls
            if (world.GetTileID(x, y) == this.TileID && !bottom)
            {
                Main.EntityManager.CreateProjectile(_projectileID, new Vector2(x, y) * Vestige.TILESIZE, 200.0f, new Vector2(0, 1));
                return -1;
            }
            return top || right || bottom || left || wall ? 1 : 0; ;
        }
    }
}
