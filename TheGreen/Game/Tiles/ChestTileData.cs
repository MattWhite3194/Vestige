using Microsoft.Xna.Framework;
using TheGreen.Game.Items;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    internal class ChestTileData : LargeTileData, IInteractableTile
    {
        public ChestTileData(int tileID, TileProperty properties, Color color, int itemID = -1) : base(tileID, properties, color, new Point(2, 2), new Point(0, 1), itemID, 0)
        {
        }

        public void CloseInventory()
        {
            throw new System.NotImplementedException();
        }

        public void OnRightClick(int x, int y)
        {
            Point worldOrigin = GetTopLeft(x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    WorldGen.World.SetTileState(worldOrigin.X + i, worldOrigin.Y + j, (byte)(j * 10 + i + 2));
                }
            }
            Item[] items = WorldGen.World.GetTileInventory(worldOrigin);
            if (items == null)
            {
                items = new Item[8 * 5];
                WorldGen.World.AddTileInventory(worldOrigin, items);
            }
            Main.EntityManager.GetPlayer().Inventory.DisplayTileInventory(8, items);
        }
    }
}
