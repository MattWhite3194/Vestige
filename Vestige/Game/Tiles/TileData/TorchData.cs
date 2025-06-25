using Microsoft.Xna.Framework;
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
            return world.GetLiquid(x, y) != 0
                ? -1
                : wall != 0 || TileDatabase.TileHasProperties(right, TileProperty.Solid) || TileDatabase.TileHasProperties(bottom, TileProperty.Solid) || TileDatabase.TileHasProperties(left, TileProperty.Solid)
                ? 1
                : -1;
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

            return TileDatabase.TileHasProperties(bottom, TileProperty.Solid)
                ? (byte)0
                : TileDatabase.TileHasProperties(left, TileProperty.Solid)
                ? (byte)34
                : TileDatabase.TileHasProperties(right, TileProperty.Solid) ? (byte)62 : (byte)0;
        }
    }
}
