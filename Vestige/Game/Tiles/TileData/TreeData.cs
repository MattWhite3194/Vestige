using Microsoft.Xna.Framework;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class TreeData : DefaultTileData
    {
        public TreeData(int tileID, TileProperty properties, Color color, int itemID = -1, int health = 0) : base(tileID, properties, color, itemID, health)
        {
        }
        public override bool CanTileBeDamaged(int x, int y)
        {
            return true;
        }
        public override int VerifyTile(int x, int y)
        {
            ushort bottom = WorldGen.World.GetTileID(x, y + 1);
            ushort left = WorldGen.World.GetTileID(x - 1, y);
            ushort right = WorldGen.World.GetTileID(x + 1, y);
            byte state = WorldGen.World.GetTileState(x, y);
            if (bottom != TileID && !TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return -1;
            else if ((state == 62 || state == 130) && left != TileID && right != TileID)
                return -1;
            return 1;
        }
        public override byte GetUpdatedTileState(int x, int y)
        {
            ushort top = WorldGen.World.GetTileID(x, y - 1);
            byte state = WorldGen.World.GetTileState(x, y);
            if (top != TileID && state <= 34)
                return 131;
            if (state == 128 || state == 139 || state == 142)
            {
                ushort left = WorldGen.World.GetTileID(x - 1, y);
                ushort right = WorldGen.World.GetTileID(x + 1, y);
                if (left == TileID && right == TileID)
                    return 128;
                else if (left == TileID)
                    return 139;
                else if (right == TileID)
                    return 142;
                return 143;
            }
            return WorldGen.World.GetTileState(x, y);
        }
    }
}
