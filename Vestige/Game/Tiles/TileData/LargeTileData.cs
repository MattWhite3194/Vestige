using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class LargeTileData : DefaultTileData
    {
        public readonly Point TileSize;
        /// <summary>
        /// The tiles origin point when placing this tile, (0, 0) is the top left corner.
        /// Defaults to the bottom left corner.
        /// </summary>
        public readonly Point Origin;
        private int _animations;
        public LargeTileData(int tileID, string name, TileProperty properties, Color color, Point tileSize, Point origin = default, int itemID = -1, int health = 0, int animations = 0) : base(tileID, name, properties | TileProperty.LargeTile, color, itemID, -1, health)
        {
            TileSize = tileSize;
            Origin = origin == default ? new Point(0, TileSize.Y - 1) : origin;
            _animations = animations;
        }
        public override int VerifyTile(WorldGen world, int x, int y)
        {
            //World origin is always the bottom left corner
            Point bottomLeft = GetTopLeft(world, x, y) + new Point(0, TileSize.Y - 1);

            int verification = 1;
            for (int i = 0; i < TileSize.X; i++)
            {
                if (!TileDatabase.TileHasProperties(world.GetTileID(bottomLeft.X + i, bottomLeft.Y + 1), TileProperty.Solid))
                    return -1;
                for (int j = 0; j < TileSize.Y; j++)
                {
                    //TODO: change to check if it's a replaceable tile like grass or something
                    if (world.GetTileID(bottomLeft.X + i, bottomLeft.Y - j) != 0)
                        verification = 0;
                }
            }
            return verification;
        }
        public override bool CanTileBeDamaged(WorldGen world, int x, int y)
        {
            return true;
        }
        public override byte GetUpdatedTileState(WorldGen world, int x, int y)
        {
            return world.GetTileState(x, y);
        }
        public virtual Point GetTopLeft(WorldGen world, int x, int y)
        {
            if (world.GetTileID(x, y) != TileID)
                return new Point(x, y) - Origin;
            int tileState = world.GetTileState(x, y);
            int xOff = tileState % 10 % TileSize.X;
            int yOff = tileState / 10 % TileSize.Y;
            return new Point(x - xOff, y - yOff);
        }
        public override void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            Rectangle textureAtlas = new Rectangle(state % 10 * Vestige.TILESIZE, state / 10 * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * Vestige.TILESIZE, textureAtlas, light);
        }
        public override void DrawPrimitive(GraphicsDevice graphicsDevice, BasicEffect tileDrawEffect, int x, int y, byte state, Color tl, Color tr, Color bl, Color br)
        {
            Vector2 position = new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE);
            Rectangle sourceRect = new Rectangle(state % 10 * Vestige.TILESIZE, state / 10 * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
            base.DrawPrimitive(graphicsDevice, tileDrawEffect, position, sourceRect, tl, tr, bl, br);
        }
    }
}
