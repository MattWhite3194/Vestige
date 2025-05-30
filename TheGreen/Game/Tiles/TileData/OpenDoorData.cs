using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TheGreen.Game.Entities;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles.TileData
{
    public class OpenDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _closedDoorID;
        public OpenDoorData(int tileID, TileProperty properties, Color color, ushort closedDoorID, int itemID = -1) : base(tileID, properties, color, new Point(2, 3), itemID: itemID)
        {
            _closedDoorID = closedDoorID;
        }

        public override int VerifyTile(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            if (WorldGen.World.GetTileID(topLeft.X, topLeft.Y) != TileID)
                return 1;
            int closeDirection = WorldGen.World.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(topLeft.X + closeDirection, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            else if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(topLeft.X + closeDirection, topLeft.Y + TileSize.Y), TileProperty.Solid))
                return -1;
            return 1;
        }

        public override Point GetTopLeft(int x, int y)
        {
            if (WorldGen.World.GetTileID(x, y) != TileID)
                return new Point(x, y) - Origin;
            int tileState = WorldGen.World.GetTileState(x, y);
            int xOff = tileState % 10 % TileSize.X;
            int yOff = tileState % 100 / 10 % TileSize.Y;
            return new Point(x - xOff, y - yOff);
        }

        public void OnCollision(int x, int y, Entity entity)
        {
            //Check here if the door was right click opened or not force opened, either or open door if so
            if (WorldGen.World.GetTileState(x, y) < 100)
                return;
            CloseDoor(x, y);
        }

        public void OnRightClick(int x, int y)
        {
            CloseDoor(x, y);
        }
        private void CloseDoor(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            int closeDirection = WorldGen.World.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            //check if the player is colliding with any of the tiles
            for (int i = 0; i < TileSize.Y; i++)
            {
                CollisionRectangle tileCollider = new CollisionRectangle((topLeft.X + closeDirection) * TheGreen.TILESIZE, (topLeft.Y + i) * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                //possibly change this to all entities
                if (Main.EntityManager.GetPlayer().GetBounds().Intersects(tileCollider))
                    return;
            }
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    WorldGen.World.PlaceTile(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
            WorldGen.World.PlaceTile(topLeft.X + closeDirection, topLeft.Y + TileSize.Y - 1, _closedDoorID);
        }
        public override void Draw(SpriteBatch spriteBatch, byte tileState, int x, int y)
        {
            Rectangle textureAtlas = new Rectangle(tileState % 10 * TheGreen.TILESIZE, tileState % 100 / 10 * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * TheGreen.TILESIZE, textureAtlas, Main.LightEngine.GetLight(x, y));
        }
    }
}
