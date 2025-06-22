using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.WorldMap
{
    public class Map
    {
        public RenderTarget2D MapRenderTarget;
        private Texture2D _mapTileTexture;
        private WorldGen _world;
        public Map(WorldGen world, GraphicsDevice graphicsDevice)
        {
            _world = world;
            MapRenderTarget = new RenderTarget2D(graphicsDevice, world.WorldSize.X, world.WorldSize.Y);
            _mapTileTexture = new Texture2D(graphicsDevice, 1, 1);
            _mapTileTexture.SetData([Color.Black]);
        }
        public void UpdateMapTile(SpriteBatch spriteBatch, Point position, Color color)
        {
            
        }
        public void RevealAllMapTiles()
        {
            Color[] mapTiles = new Color[_world.WorldSize.X * _world.WorldSize.Y];
            for (int i = 0; i < _world.WorldSize.X; i++)
            {
                for (int j = 0; j < _world.WorldSize.Y; j++)
                {
                    mapTiles[_world.WorldSize.X * j + i] = TileDatabase.GetTileData(_world.GetTileID(i, j)).MapColor;
                }
            }
            MapRenderTarget.SetData(mapTiles);
        }
        public void ClearMap()
        {
            MapRenderTarget.SetData(Enumerable.Repeat(Color.Black, _world.WorldSize.X * _world.WorldSize.Y).ToArray());
        }
    }
}
