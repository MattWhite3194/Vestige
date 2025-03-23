using Microsoft.Xna.Framework;
using System;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    internal class TorchData : TileData
    {
        public TorchData(TileProperty properties, Color color, int itemID = -1) : base(properties, color, itemID)
        {
        }
        public override int VerifyTile(ushort tileID, int x, int y)
        {
            ushort right = WorldGen.Instance.GetTileID(x + 1, y);
            ushort bottom = WorldGen.Instance.GetTileID(x, y + 1);
            ushort left = WorldGen.Instance.GetTileID(x - 1, y);
            ushort wall = WorldGen.Instance.GetWallID(x, y);
            if (Math.Sign(wall) == 1 || TileDatabase.TileHasProperty(right, TileProperty.Solid) || TileDatabase.TileHasProperty(bottom, TileProperty.Solid) || TileDatabase.TileHasProperty(left, TileProperty.Solid))
                return 1;
            return -1;
        }
        public override byte GetUpdatedTileState(ushort tileID, int x, int y)
        {
            ushort bottom = WorldGen.Instance.GetTileID(x, y + 1);
            ushort left = WorldGen.Instance.GetTileID(x - 1, y);
            ushort right = WorldGen.Instance.GetTileID(x + 1, y);

            if (TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return 0;
            else if (TileDatabase.TileHasProperty(left, TileProperty.Solid))
                return 34;
            else if (TileDatabase.TileHasProperty(right, TileProperty.Solid))
                return 62;
            else
                return 0;
        }
    }
}
