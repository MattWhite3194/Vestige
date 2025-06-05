using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.IO;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game
{
    public static class Utilities
    {
        private static Texture2D _pixel;
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            //Initialize everything with graphics device here
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
        }
        public static void DrawOutlineRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, int lineWidth = 1)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, lineWidth), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - lineWidth, rect.Width, lineWidth), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, lineWidth, rect.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - lineWidth, rect.Y, lineWidth, rect.Height), color);
        }
        public static void DrawFilledRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, rect, color);
        }
        public static void DrawRoundedRectangle(SpriteBatch spriteBatch, int x, int y, int width, int height, Color color)
        {
            //4 corners
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x - 8, y - 8, 8, 8), new Rectangle(0, 0, 8, 8), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x + width, y + height, 8, 8), new Rectangle(8, 8, 8, 8), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x + width, y - 8, 8, 8), new Rectangle(8, 0, 8, 8), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x - 8, y + height, 8, 8), new Rectangle(0, 8, 8, 8), color);

            //4 sides
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x, y - 8, width, 8), new Rectangle(5, 0, 5, 8), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x, y + height, width, 8), new Rectangle(5, 8, 5, 8), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x - 8, y, 8, height), new Rectangle(0, 5, 8, 5), color);
            spriteBatch.Draw(ContentLoader.Squircle, new Rectangle(x + width, y, 8, height), new Rectangle(8, 5, 8, 5), color);

            //fill
            DrawFilledRectangle(spriteBatch, new Rectangle(x, y, width, height), color);
        }
        public static Texture2D GenerateVerticalGradient(GraphicsDevice graphicsDevice, Color[] colors, int height, bool wrap = false)
        {
            Texture2D gradient = new Texture2D(graphicsDevice, 1, height);
            Color[] gradientData = new Color[height];
            int colorOffset = height / (colors.Length - 1);
            int colorIndex = 0;
            for (int i = 0; i < height; i++)
            {
                if (i != 0 && i % colorOffset == 0)
                    colorIndex++;
                int nextColor = (colorIndex + 1) % colors.Length;
                if (colorIndex == colors.Length - 1 && !wrap)
                    nextColor = colorIndex;
                gradientData[i] = Color.Lerp(colors[colorIndex], colors[nextColor], (i % colorOffset) / (float)colorOffset);
            }
            gradient.SetData(gradientData);
            return gradient;
        }

        public static void RunWorldGenTest(int sizeX, int sizeY, GraphicsDevice graphicsDevice, int seed = 0)
        {
            WorldGen world = new WorldGen(sizeX, sizeY);
            world.GenerateWorld(seed);

            //Save map of world to png
            Texture2D Map = new Texture2D(graphicsDevice, sizeX, sizeY);
            Color[] colorData = new Color[sizeX * sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    colorData[x + y * sizeX] = TileDatabase.GetTileData(world.GetTileID(x, y)).MapColor;
                }
            }

            Map.SetData(colorData);
            string gamePath = Path.Combine(Vestige.SavePath, "WorldGenerationTests");
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }
            string filePath = Path.Combine(gamePath, "worldGenTest.png");
            using (Stream stream = File.Create(filePath))
            {
                Map.SaveAsPng(stream, sizeX, sizeY);
            }

            //open world image
            var psi = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };
            Process process = Process.Start(psi);
            process.Dispose();
            Map.Dispose();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.WaitForFullGCComplete();
        }
    }
}
