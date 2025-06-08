using Microsoft.Xna.Framework;
using System;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class TorchData : DefaultTileData
    {
        public TorchData(int tileID, string name, TileProperty properties, Color color, int itemID = -1) : base(tileID, name, properties | TileProperty.LightEmitting, color, itemID)
        {
        }
        public override int VerifyTile(WorldGen world, int x, int y)
        {
            ushort right = world.GetTileID(x + 1, y);
            ushort bottom = world.GetTileID(x, y + 1);
            ushort left = world.GetTileID(x - 1, y);
            ushort wall = world.GetWallID(x, y);
            if (world.GetLiquid(x, y) != 0)
                return -1;
            if (wall != 0 || TileDatabase.TileHasProperties(right, TileProperty.Solid) || TileDatabase.TileHasProperties(bottom, TileProperty.Solid) || TileDatabase.TileHasProperties(left, TileProperty.Solid))
                return 1;
            return -1;
        }
        public override bool CanTileBeDamaged(WorldGen world, int x, int y)
        {
            return true;
        }
        public override byte GetUpdatedTileState(WorldGen world, int x, int y)
        {
            ushort bottom = world.GetTileID(x, y + 1);
            ushort left = world.GetTileID(x - 1, y);
            ushort right = world.GetTileID(x + 1, y);

            if (TileDatabase.TileHasProperties(bottom, TileProperty.Solid))
                return 0;
            else if (TileDatabase.TileHasProperties(left, TileProperty.Solid))
                return 34;
            else if (TileDatabase.TileHasProperties(right, TileProperty.Solid))
                return 62;
            else
                return 0;
        }
    }
}
