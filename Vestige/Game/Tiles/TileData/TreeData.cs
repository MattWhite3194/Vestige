using Microsoft.Xna.Framework;
using Vestige.Game.UI.Components;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class TreeData : DefaultTileData
    {
        public TreeData(int tileID, TileProperty properties, Color color, int itemID = -1, int health = 0) : base(tileID, properties, color, itemID, health)
        {
        }
        public override bool CanTileBeDamaged(WorldGen world, int x, int y)
        {
            return true;
        }
        public override int VerifyTile(WorldGen world, int x, int y)
        {
            ushort bottom = world.GetTileID(x, y + 1);
            ushort left = world.GetTileID(x - 1, y);
            ushort right = world.GetTileID(x + 1, y);
            byte state = world.GetTileState(x, y);
            if (TileDatabase.GetTileData(bottom) is not TreeData && !TileDatabase.TileHasProperties(bottom, TileProperty.Solid))
                return -1;
            else if ((state == 62 || state == 130) && left != TileID && right != TileID)
                return -1;
            return 1;
        }
        public override byte GetUpdatedTileState(WorldGen world, int x, int y)
        {
            ushort top = world.GetTileID(x, y - 1);
            byte state = world.GetTileState(x, y);
            if (TileDatabase.GetTileData(top) is not TreeData && state <= 34)
                return 131;
            if (state == 128 || state == 139 || state == 142)
            {
                ushort left = world.GetTileID(x - 1, y);
                ushort right = world.GetTileID(x + 1, y);
                if (left == TileID && right == TileID)
                    return 128;
                else if (left == TileID)
                    return 139;
                else if (right == TileID)
                    return 142;
                return 143;
            }
            return world.GetTileState(x, y);
        }
    }
}
