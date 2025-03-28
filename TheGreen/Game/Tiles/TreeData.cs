using Microsoft.Xna.Framework;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    internal class TreeData : TileData
    {
        public TreeData(TileProperty properties, Color color, int itemID = -1, int health = 0) : base(properties, color, itemID, health)
        {
        }
        public override int VerifyTile(ushort tileID, int x, int y)
        {
            ushort bottom = WorldGen.World.GetTileID(x, y + 1);
            ushort left = WorldGen.World.GetTileID(x - 1, y);
            ushort right = WorldGen.World.GetTileID(x + 1, y);

            if (TileDatabase.GetTileType(bottom) != typeof(TreeData) && !TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return -1;
            else if ((WorldGen.World.GetTileState(x, y) == 62 || WorldGen.World.GetTileState(x, y) == 130) && left != tileID && right != tileID)
                return -1;
            return 1;
        }
    }
}
