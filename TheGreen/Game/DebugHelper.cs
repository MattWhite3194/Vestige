using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
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
            _pixel.SetData([Color.White]);
        }
        public static void DrawOutlineRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 1), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width, rect.Y, 1, rect.Height), color);
        }
        public static void DrawFilledRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, rect, color);
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
