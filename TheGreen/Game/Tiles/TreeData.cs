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
            ushort bottom = WorldGen.Instance.GetTileID(x, y + 1);
            ushort left = WorldGen.Instance.GetTileID(x - 1, y);
            ushort right = WorldGen.Instance.GetTileID(x + 1, y);

            if (TileDatabase.GetTileType(bottom) != typeof(TreeData) && !TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
                return -1;
            else if ((WorldGen.Instance.GetTileState(x, y) == 62 || WorldGen.Instance.GetTileState(x, y) == 130) && left != tileID && right != tileID)
                return -1;
            return 1;
        }
    }
}
