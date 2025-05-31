using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using Vestige.Game.Entities;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class OpenDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _closedDoorID;
        public OpenDoorData(int tileID, TileProperty properties, Color color, ushort closedDoorID, int itemID = -1) : base(tileID, properties, color, new Point(2, 3), itemID: itemID)
        {
            _closedDoorID = closedDoorID;
        }

        public override int VerifyTile(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            if (world.GetTileID(topLeft.X, topLeft.Y) != TileID)
                return 1;
            int closeDirection = world.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            if (!TileDatabase.TileHasProperty(world.GetTileID(topLeft.X + closeDirection, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            else if (!TileDatabase.TileHasProperty(world.GetTileID(topLeft.X + closeDirection, topLeft.Y + TileSize.Y), TileProperty.Solid))
                return -1;
            return 1;
        }

        public override Point GetTopLeft(WorldGen world, int x, int y)
        {
            if (world.GetTileID(x, y) != TileID)
                return new Point(x, y) - Origin;
            int tileState = world.GetTileState(x, y);
            int xOff = tileState % 10 % TileSize.X;
            int yOff = tileState % 100 / 10 % TileSize.Y;
            return new Point(x - xOff, y - yOff);
        }

        public void OnCollision(WorldGen world, int x, int y, Entity entity)
        {
            //Check here if the door was right click opened or not force opened, either or open door if so
            if (world.GetTileState(x, y) < 100)
                return;
            CloseDoor(world, x, y);
        }

        public void OnRightClick(WorldGen world, int x, int y)
        {
            CloseDoor(world, x, y);
        }
        private void CloseDoor(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            int closeDirection = world.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            //check if the player is colliding with any of the tiles
            for (int i = 0; i < TileSize.Y; i++)
            {
                CollisionRectangle tileCollider = new CollisionRectangle((topLeft.X + closeDirection) * Vestige.TILESIZE, (topLeft.Y + i) * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
                //possibly change this to all entities
                if (Main.EntityManager.GetPlayer().GetBounds().Intersects(tileCollider))
                    return;
            }
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    world.PlaceTile(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
            world.PlaceTile(topLeft.X + closeDirection, topLeft.Y + TileSize.Y - 1, _closedDoorID);
        }
        public override void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            Rectangle textureAtlas = new Rectangle(state % 10 * Vestige.TILESIZE, state % 100 / 10 * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * Vestige.TILESIZE, textureAtlas, light);
        }
    }
}
