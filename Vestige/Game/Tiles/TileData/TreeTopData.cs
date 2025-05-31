using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vestige.Game.Tiles.TileData
{
    public class TreeTopData : TreeData
    {
        private Vector2 _offset;
        public TreeTopData(int tileID, TileProperty properties, Color color, Vector2 offset) : base(tileID, properties, color)
        {
            _offset = offset;
        }
        public override void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE) + _offset, light);
        }
    }
}
