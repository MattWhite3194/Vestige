

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vestige.Game.WorldMap
{
    public class Map
    {
        //TODO: This shit
        private RenderTarget2D _mapRenderTarget;
        private Texture2D _mapTileTexture;

        public Map(GraphicsDevice graphicsDevice, Point worldSize)
        {
            _mapRenderTarget = new RenderTarget2D(graphicsDevice, worldSize.X, worldSize.Y);
            _mapTileTexture = new Texture2D(graphicsDevice, 1, 1);
            _mapTileTexture.SetData([Color.White]);
        }

        public void UpdateMapTile(SpriteBatch spriteBatch, Point position, Color color)
        {
            spriteBatch.Begin();

        }
    }
}
