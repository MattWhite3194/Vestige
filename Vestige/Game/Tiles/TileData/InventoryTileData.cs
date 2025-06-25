using Microsoft.Xna.Framework;
using Vestige.Game.Entities;
using Vestige.Game.Items;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class InventoryTileData : LargeTileData, IInteractableTile
    {
        //TODO: change this file to InventoryTileData, and make it reusable for other inventory tile types
        public readonly int Cols;
        private readonly int Rows;
        public InventoryTileData(int tileID, string name, TileProperty properties, Color color, int itemID = -1, int cols = 8, int rows = 5) : base(tileID, name, properties, color, new Point(2, 2), new Point(0, 1), itemID, 0)
        {
            Cols = cols;
            Rows = rows;
        }

        public override bool CanTileBeDamaged(WorldGen world, int x, int y)
        {
            Point worldOrigin = GetTopLeft(world, x, y);
            return world.GetTileInventory(worldOrigin) == null;
        }
        public void CloseInventory(WorldGen world, int x, int y)
        {
            Point worldOrigin = GetTopLeft(world, x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    world.SetTileState(worldOrigin.X + i, worldOrigin.Y + j, (byte)((j * 10) + i));
                }
            }
            Item[] items = world.GetTileInventory(worldOrigin);
            bool emptyInventory = true;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    emptyInventory = false;
                    break;
                }
            }
            if (emptyInventory)
            {
                world.RemoveTileInventory(worldOrigin);
            }
        }

        public void OnRightClick(WorldGen world, Player player, int x, int y)
        {
            //Inventory Tiles should only have 2 frames, one for closed and one for open, maybe in the future I'll do animations
            Point worldOrigin = GetTopLeft(world, x, y);
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    world.SetTileState(worldOrigin.X + i, worldOrigin.Y + j, (byte)((j * 10) + i + TileSize.X));
                }
            }
            Item[] items = world.GetTileInventory(worldOrigin);
            if (items == null)
            {
                items = new Item[Rows * Cols];
                world.AddTileInventory(worldOrigin, items);
            }
            player.Inventory.DisplayTileInventory(this, new Point(worldOrigin.X, worldOrigin.Y), items);
        }
    }
}
