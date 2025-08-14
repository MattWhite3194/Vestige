using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Entities;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class OpenDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _closedDoorID;
        public OpenDoorData(int tileID, string name, TileProperty properties, Color color, ushort closedDoorID, int itemID = -1) : base(tileID, name, properties, color, new Point(2, 3), itemID: itemID)
        {
            _closedDoorID = closedDoorID;
        }

        public override int VerifyTile(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            //What is this doing? I forgor
            if (world.GetTileID(topLeft.X, topLeft.Y) != TileID)
                return 1;
            int closeDirection = world.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            if (!TileDatabase.TileHasProperties(world.GetTileID(topLeft.X + closeDirection, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            else if (!TileDatabase.TileHasProperties(world.GetTileID(topLeft.X + closeDirection, topLeft.Y + TileSize.Y), TileProperty.Solid))
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
            Point topLeft = GetTopLeft(world, x, y);
            world.OnWorldUpdate += () => CloseDoor(world, topLeft.X, topLeft.Y);
            //Update tilestate so it's not added multiple times
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    world.SetTileState(topLeft.X + i, topLeft.Y + j, (byte)(world.GetTileState(topLeft.X + i, topLeft.Y + j) - 100));
                }
            }
        }

        public void OnRightClick(WorldGen world, Player player, int x, int y)
        {
            CloseDoor(world, x, y);
        }
        private bool CloseDoor(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            int closeDirection = world.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            //check if the player is colliding with any of the tiles
            for (int i = 0; i < TileSize.Y; i++)
            {
                if (world.GetTileID(topLeft.X + closeDirection, topLeft.Y + i) != TileID)
                    return true;
                if (Main.EntityManager.TileOccupied(topLeft.X + closeDirection, topLeft.Y + i))
                    return false;
            }
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    world.PlaceTile(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
            world.PlaceTile(topLeft.X + closeDirection, topLeft.Y + TileSize.Y - 1, _closedDoorID);
            return true;
        }
        public override void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            Rectangle textureAtlas = new Rectangle(state % 10 * Vestige.TILESIZE, state % 100 / 10 * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * Vestige.TILESIZE, textureAtlas, light);
        }
        public override void DrawPrimitive(GraphicsDevice graphicsDevice, BasicEffect tileDrawEffect, int x, int y, byte state, Color tl, Color tr, Color bl, Color br)
        {
            Vector2 position = new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE);
            Rectangle sourceRect = new Rectangle(state % 10 * Vestige.TILESIZE, state % 100 / 10 * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            base.DrawPrimitive(graphicsDevice, tileDrawEffect, position, sourceRect, tl, tr, bl, br);
        }
    }
}
