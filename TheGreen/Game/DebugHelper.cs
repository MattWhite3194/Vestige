using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game
{
    public static class DebugHelper
    {
        private static Texture2D _pixel;
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            //Initialize everything with graphics device here
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }
        public static void DrawDebugRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width, rect.Y, 1, rect.Height), color);
        }

        public static void RunWorldGenTest(int sizeX, int sizeY, GraphicsDevice graphicsDevice, int seed = 0)
        {
            WorldGen.World.GenerateWorld(sizeX, sizeY, seed);

            //Save map of world to png
            Texture2D Map = new Texture2D(graphicsDevice, sizeX, sizeY);
            Color[] colorData = new Color[sizeX * sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    colorData[x + y * sizeX] = TileDatabase.GetTileData(WorldGen.World.GetTileID(x, y)).MapColor;
                }
            }

            Map.SetData(colorData);
            string gamePath = Path.Combine(TheGreen.SavePath, "WorldGenerationTests");
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }
            Stream stream = File.Create(gamePath + "/worldGenTest.jpg");
            Map.SaveAsJpeg(stream, sizeX, sizeY);
            stream.Close();
        }
    }
}
