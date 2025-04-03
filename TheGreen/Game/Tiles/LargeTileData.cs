using Microsoft.Xna.Framework;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    public class LargeTileData : TileData
    {
        public readonly Point TileSize;
        public readonly Point Origin;
        public LargeTileData(int tileID, TileProperty properties, Color color, Point tileSize, Point Origin = default, int itemID = -1, int health = 0, ushort baseTileID = 0) : base(tileID, properties, color, itemID, health, baseTileID)
        {
            this.TileSize = tileSize;
        }
        public override int VerifyTile(int x, int y)
        {
            Point origin = GetTileOrigin(x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(origin.X + i, origin.Y + TileSize.Y), TileProperty.Solid))
                    return -1;
                for (int j = 0; j < TileSize.Y; j++)
                {
                    //TODO: change to check if it's a replaceable tile like grass or something
                    if (WorldGen.World.GetTileID(origin.X + i, origin.Y + j) != 0)
                        return 0;
                }
            }
            return 1;
        }
        public virtual Point GetTileOrigin(int x, int y)
        {
            int tileState = WorldGen.World.GetTileState(x, y);
            int xOff = tileState % 10;
            int yOff = tileState / 10;
            return new Point (x - xOff, y - yOff);
        }
    }
}
