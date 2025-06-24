using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vestige.Game.Tiles.TileData
{
    public class TreeTopData : TreeData
    {
        private Vector2 _offset;
        public TreeTopData(int tileID, string name, TileProperty properties, Color color, Vector2 offset) : base(tileID, name, properties, color)
        {
            _offset = offset;
        }
        public override void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE) + _offset, light);
        }
        public override void DrawPrimitive(GraphicsDevice graphicsDevice, BasicEffect tileDrawEffect, int x, int y, byte state, Color tl, Color tr, Color bl, Color br)
        {
            Vector2 position = new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE) + _offset;
            Rectangle sourceRect = new Rectangle(0, 0, ContentLoader.TileTextures[TileID].Width, ContentLoader.TileTextures[TileID].Height);
            base.DrawPrimitive(graphicsDevice, tileDrawEffect, position, sourceRect, tl, tr, bl, br);
        }
    }
}
