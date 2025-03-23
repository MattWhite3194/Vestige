using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheGreen.Game.Tiles
{
    internal class TreeTopData : TreeData
    {
        private Vector2 _offset;
        public TreeTopData(TileProperty properties, Color color, Vector2 offset) : base(properties, color)
        {
            _offset = offset;
        }
        public override void Draw(SpriteBatch spriteBatch, int tileID, byte tileState, int x, int y)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[tileID], new Vector2(x * Globals.TILESIZE, y * Globals.TILESIZE) + _offset, Color.White);
        }
    }
}
